using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class MinecartController : MonoBehaviour
{
    // A lane = all splines sharing the same Z, ridden left to right as segments.
    private class Lane
    {
        public float z;
        public float minY;
        public List<SplineContainer> segs = new List<SplineContainer>();
        public float[] lengths;
    }

    [Header("Lane Setup")]
    [SerializeField] private string ignoreNameContains = "Reference"; //Splines whose name contains this text are ignored

    [SerializeField] private float laneZTolerance = 0.1f;//splines within this Z distance of each other are treated as the same lane; keep this smaller than the gap between lanes

    [SerializeField] private int startLane = -1; //pick the lane nearest this cart's starting z (so -1)

    [SerializeField] private bool assignLaneByPlayerIndex = false; // in the case of startLane being -1, uses the playerinput index instead of z (player = -> lowest z lane)
    [SerializeField] private float currentDistance = 0f;

    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 25f;

    [SerializeField] private float acceleration = 15f;

    [SerializeField] private float deceleration = 10f;

    [Header("Jump Settings")]

    [SerializeField] private float jumpImpulse = 12f; //jump launches perpendicular to track surface; on a slope it pushes away from it to look more natural

    [SerializeField] private float baseGravity = -25f;

    [SerializeField] private float fallGravityMultiplier = 1.8f;

    [Header("Jump Rotation")]

    [SerializeField, Range(0f, 1f)] private float airTiltAmount = 0.6f; //tilt value, follows an arc when falling (0 stays the way it left the track, 1 fully points along its direction)


    [SerializeField] private float airRotateSpeed = 7f; //how fast the tilt happens (higher = more snappeh)

    [Tooltip("How quickly the cart rotates to match the track on the ground. Also smooths the landing. Lower = softer, higher = rigid.")]
    [SerializeField] private float groundAlignSpeed = 25f; //speed the cart rotates to match the track and smooths the landing. (# lower = softer, higher = sharper)

    [Header("Landing")]

    [SerializeField] private float surfaceOffset = 0f; //height of cart when riding

    [SerializeField] private float minSnapDistance = 1.5f; //safety for ensuring a snap (solved bug with falling through ground randomly)

    [SerializeField] private float maxFallSpeed = 45f; //helped with falling through ground from falling too quick and clipping

    [SerializeField] private float killDepth = 25f; //respawn if you're below this point (!!TEMP for playtesting!!)

    [SerializeField] private bool flipSurfaceNormal = false; //safety for spline traversal

    [SerializeField] private int nearestResolution = 64;

    [SerializeField] private int nearestIterations = 4;

    [Header("Lane Switching (Up & Down Input)")]
    [SerializeField] private bool upGoesToHigherZ = true;

    [SerializeField] private float switchDuration = 0.2f;

    [SerializeField] private float hopHeight = 0.25f; //spline hop landing shmovement

    [SerializeField] private float maxHorizontalGap = 1.5f; // check for neighboring lane horizontally
    [Tooltip("...and within this vertical distance of the cart.")]
    [SerializeField] private float maxVerticalGap = 3f; //vertical check for neighboring lane

    [SerializeField] private float inputThreshold = 0.6f; //timing for input

    [Header("Respawn")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float respawnMaxRailGap = 4f;

    [Header("Debug")]
    [Tooltip("Logs the lanes found at startup and WHY a lane switch was refused. Turn off when it works.")]
    [SerializeField] private bool debugLogging = true;

    // Hook these up to UI arrows if you want to show "you can switch here".
    public bool CanSwitchUp { get; private set; }
    public bool CanSwitchDown { get; private set; }
    public bool IsGrounded => isGrounded;
    public bool IsOut => isOut;
    public bool IsBoosting => boostTimer > 0f;
    public float LaneZ => lanes.Count > 0 ? lanes[currentLane].z : transform.position.z;

    ////////////////////////////////// character state /////////////////////////////////////////////////
    private Rigidbody rb;
    private float currentSpeed = 0f;      // speed along the track while grounded
    private Vector2 airVelocity;          // velocity while airborne
    private bool isGrounded = true;

    ////////////////////////////////// active spline state /////////////////////////////////////////////////
    private List<Lane> lanes = new List<Lane>();
    private int currentLane = 0;
    private int currentSplineIndex = 0;
    private SplineContainer activeSplineContainer;
    private float activeSplineLength;
    private Vector3 airbornePosition;
    private bool wasAboveSurface = true;
    private Quaternion visualRotation = Quaternion.identity;
    private float facing = 1f;

    // last safe spot for the respawn failsafe
    private int safeLane, safeSeg;
    private float safeDistance;

    // lane switch
    private bool switching;
    private float switchT;
    private float switchStartZOffset;
    private float zOffset;
    private float yHop;

    private bool isOut;
    private float boostTimer;
    private float boostSpeed;
    private readonly List<Renderer> hiddenRenderers = new List<Renderer>();
    private readonly List<Collider> hiddenColliders = new List<Collider>();

    ////////////////////////////////// input system /////////////////////////////////////////////////
    private Vector2 moveInput;
    private float prevMoveY;
    private bool jumpPressed;

    UnityEvent playerJoin;

    [Header("Rolling Audio")]
    [SerializeField] private AudioSource rollingAudioSource;

    public void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        // Z is NOT frozen
        // lanes are separated in depth (z), so the cart has to be free to slide along it
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;

        visualRotation = rb.rotation;
        BuildLanes();
        PickStartLane();
        ActivateSegment(currentSplineIndex);
        RememberSafeSpot();

        if (playerJoin == null) playerJoin = new UnityEvent();
    }

    /////////////////////////////////////////////////// setup  /////////////////////////////////////////////////
    private void BuildLanes()
    {
        lanes.Clear();

        var all = Object.FindObjectsByType<SplineContainer>(FindObjectsInactive.Exclude)
            .Where(c => c.Spline != null && c.Spline.Count > 1)
            .Where(c => string.IsNullOrEmpty(ignoreNameContains) || !c.name.Contains(ignoreNameContains))
            .Select(c => (c, z: FirstKnotZ(c)))
            .OrderBy(p => p.z)
            .ToList();

        Lane lane = null;
        foreach (var p in all)
        {
            if (lane == null || p.z - lane.z > laneZTolerance)
            {
                lane = new Lane { z = p.z };
                lanes.Add(lane);
            }
            lane.segs.Add(p.c);
        }

        foreach (var l in lanes)
        {
            l.segs = l.segs.OrderBy(c => c.transform.TransformPoint(c.Spline[0].Position).x).ToList();
            l.lengths = l.segs.Select(c => c.CalculateLength()).ToArray();
            l.minY = float.MaxValue;
            foreach (var c in l.segs)
                foreach (var k in c.Spline)
                    l.minY = Mathf.Min(l.minY, c.transform.TransformPoint(k.Position).y);
        }

        if (debugLogging)
        {
            string report = string.Join(" | ", lanes.Select((l, i) =>
                $"lane {i}: z={l.z:F2} [{string.Join(", ", l.segs.Select(s => s.name))}]"));
            Debug.Log($"[{name}] splines considered: {all.Count}, lanes found: {lanes.Count} -> {report}");
        }
    }

    private static float FirstKnotZ(SplineContainer c) => c.transform.TransformPoint(c.Spline[0].Position).z;

    private void PickStartLane()
    {
        if (lanes.Count == 0) return;

        int lane = startLane;
        if (lane < 0 && assignLaneByPlayerIndex && TryGetComponent(out PlayerInput pi))
            lane = pi.playerIndex % lanes.Count;

        if (lane < 0)
        {
            float best = float.MaxValue;
            for (int i = 0; i < lanes.Count; i++)
            {
                float d = Mathf.Abs(lanes[i].z - rb.position.z);
                if (d < best) { best = d; lane = i; }
            }
        }
        currentLane = Mathf.Clamp(lane, 0, lanes.Count - 1);
        currentSplineIndex = 0;

        if (debugLogging) Debug.Log($"[{name}] starting on lane {currentLane} (cart z={rb.position.z:F2})");
    }

    private void ActivateSegment(int index)
    {
        if (lanes.Count == 0) return;
        var lane = lanes[currentLane];
        currentSplineIndex = Mathf.Clamp(index, 0, lane.segs.Count - 1);
        activeSplineContainer = lane.segs[currentSplineIndex];
        activeSplineLength = lane.lengths[currentSplineIndex];
    }

    ////////////////////////////////////////////////// input /////////////////////////////////////////////////
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

        if (debugLogging && moveInput.y != 0f)
            Debug.Log($"[{name}] Move input y = {moveInput.y:F2}");

        float y = moveInput.y;
        bool up = y > inputThreshold && prevMoveY <= inputThreshold;
        bool down = y < -inputThreshold && prevMoveY >= -inputThreshold;
        prevMoveY = y;

        if (isOut) return;
        if (up) TrySwitchLane(+1);
        else if (down) TrySwitchLane(-1);
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded && !isOut)
            jumpPressed = true;
    }

    public void OnSwitchUp(InputValue value)
    {
        if (value.isPressed && !isOut) TrySwitchLane(+1);
    }

    public void OnSwitchDown(InputValue value)
    {
        if (value.isPressed && !isOut) TrySwitchLane(-1);
    }

    /////////////////////////////////////////////////// update loop /////////////////////////////////////////////////
    private void Update()
    {
        if (lanes.Count == 0 || isOut) return;

        if (boostTimer > 0f) boostTimer -= Time.deltaTime;

        UpdateSwitchAnimation();

        if (isGrounded)
        {
            UpdateForwardSpeed();
            UpdateGroundedSplineMovement();
            UpdateSwitchAvailability();
        }
        else
        {
            UpdateAirborneMovement();
            CheckForRailLanding();
            CheckFellOutOfWorld();
        }

        UpdateRollingAudio();
        HandleJumpInput();
    }

    // unit tangent and surface normal (both XY) of the active spline at the given distance
    private void GetSurfaceBasis(float distance, out Vector2 tangent2D, out Vector2 normal2D)
    {
        float t = activeSplineLength > 0f ? distance / activeSplineLength : 0f;
        activeSplineContainer.Evaluate(t, out float3 _p, out float3 tan, out float3 _u);

        tangent2D = new Vector2(tan.x, tan.y);
        if (tangent2D.sqrMagnitude < 1e-6f) tangent2D = Vector2.right;
        tangent2D.Normalize();

        normal2D = new Vector2(-tangent2D.y, tangent2D.x);
        if (flipSurfaceNormal) normal2D = -normal2D;
    }

    private void HandleJumpInput()
    {
        if (!(jumpPressed && isGrounded)) return;
        jumpPressed = false;

        //perpendicular jump maintaining speed
        GetSurfaceBasis(currentDistance, out Vector2 tangent, out Vector2 normal);
        airVelocity = tangent * currentSpeed + normal * jumpImpulse;

        isGrounded = false;
        wasAboveSurface = true;
        CanSwitchUp = CanSwitchDown = false;

        // lift off along the normal so we start clearly above the surface
        airbornePosition = rb.position + new Vector3(normal.x, normal.y, 0f) * 0.3f;
        rb.position = airbornePosition;
    }

    private float TargetSpeed()
    {
        float target = moveInput.x * maxSpeed;
        if (boostTimer > 0f) target = Mathf.Max(target, boostSpeed);
        return target;
    }

    public void Eliminate()
    {
        if (isOut) return;

        isOut = true;
        currentSpeed = 0f;
        airVelocity = Vector2.zero;
        jumpPressed = false;
        boostTimer = 0f;
        switching = false;
        zOffset = 0f;
        yHop = 0f;
        CanSwitchUp = CanSwitchDown = false;

        if (rollingAudioSource != null && rollingAudioSource.isPlaying) rollingAudioSource.Stop();
        SetPresent(false);
    }

    public void RespawnAbove(float worldX, float height, float boostSpeedValue, float boostSeconds)
    {
        if (lanes.Count == 0) return;

        int bestLane = currentLane, bestSeg = 0;
        float bestT = 0f, bestDx = float.MaxValue;
        Vector3 bestPoint = rb.position;

        for (int offset = 0; offset < lanes.Count; offset++)
        {
            for (int sign = -1; sign <= 1; sign += 2)
            {
                int li = currentLane + offset * sign;
                if (li < 0 || li >= lanes.Count || (offset == 0 && sign == 1)) continue;

                if (FindRailAtX(li, worldX, out int seg, out float t, out Vector3 point, out float dx) && dx < bestDx)
                {
                    bestDx = dx; bestLane = li; bestSeg = seg; bestT = t; bestPoint = point;
                }
            }
            if (bestDx <= respawnMaxRailGap) break;
        }

        currentLane = bestLane;
        ActivateSegment(bestSeg);
        currentDistance = Mathf.Clamp(bestT * activeSplineLength, 0f, activeSplineLength);

        airbornePosition = new Vector3(bestPoint.x, bestPoint.y + height, lanes[currentLane].z);
        transform.position = airbornePosition;
        rb.position = airbornePosition;

        facing = 1f;
        visualRotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
        transform.rotation = visualRotation;
        rb.rotation = visualRotation;

        airVelocity = new Vector2(boostSpeedValue, 0f);
        currentSpeed = boostSpeedValue;
        boostSpeed = boostSpeedValue;
        boostTimer = boostSeconds;

        isGrounded = false;
        wasAboveSurface = true;
        jumpPressed = false;
        switching = false;
        zOffset = 0f;
        yHop = 0f;
        CanSwitchUp = CanSwitchDown = false;

        RememberSafeSpot();

        isOut = false;
        SetPresent(true);

        if (debugLogging) Debug.Log($"[{name}] respawned on lane {currentLane} at x={airbornePosition.x:F1}, rail gap {bestDx:F2}");
    }

    private bool FindRailAtX(int laneIndex, float x, out int seg, out float t, out Vector3 point, out float dx)
    {
        seg = 0; t = 0f; point = default; dx = float.MaxValue;
        var lane = lanes[laneIndex];
        const int samples = 96;

        for (int i = 0; i < lane.segs.Count; i++)
        {
            for (int s = 0; s <= samples; s++)
            {
                float tt = s / (float)samples;
                lane.segs[i].Evaluate(tt, out float3 p, out float3 _tan, out float3 _up);
                float d = Mathf.Abs(p.x - x);
                if (d < dx) { dx = d; seg = i; t = tt; point = (Vector3)p; }
            }
        }
        return dx < float.MaxValue;
    }

    private void SetPresent(bool present)
    {
        if (present)
        {
            foreach (var r in hiddenRenderers) if (r != null) r.enabled = true;
            foreach (var c in hiddenColliders) if (c != null) c.enabled = true;
            hiddenRenderers.Clear();
            hiddenColliders.Clear();
            return;
        }

        Transform root = visualRoot != null ? visualRoot : transform.root;
        foreach (var r in root.GetComponentsInChildren<Renderer>())
        {
            if (!r.enabled) continue;
            r.enabled = false;
            hiddenRenderers.Add(r);
        }
        foreach (var c in root.GetComponentsInChildren<Collider>())
        {
            if (!c.enabled) continue;
            c.enabled = false;
            hiddenColliders.Add(c);
        }
    }

    private void UpdateForwardSpeed() // grounded acceleration
    {
        float targetSpeed = TargetSpeed();

        if (Mathf.Abs(targetSpeed) > 0.01f)
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        else
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
    }

    private void UpdateGroundedSplineMovement()
    {
        if (activeSplineContainer == null) return;
        var lane = lanes[currentLane];

        currentDistance += currentSpeed * Time.fixedDeltaTime;

        // segment -> segment inside this lane
        if (currentDistance >= activeSplineLength && currentSplineIndex + 1 < lane.segs.Count)
        {
            float excess = currentDistance - activeSplineLength;
            ActivateSegment(currentSplineIndex + 1);
            currentDistance = excess;
        }
        else if (currentDistance < 0f && currentSplineIndex > 0)
        {
            ActivateSegment(currentSplineIndex - 1);
            currentDistance = activeSplineLength + currentDistance;
        }

        currentDistance = Mathf.Clamp(currentDistance, 0f, activeSplineLength);

        float t = activeSplineLength > 0f ? currentDistance / activeSplineLength : 0f;
        activeSplineContainer.Evaluate(t, out var rawPos, out var rawTangent, out var rawUp);

        Vector3 railPosition = (Vector3)rawPos + new Vector3(0f, yHop, zOffset);
        Vector3 tangent = Vector3.Normalize((Vector3)rawTangent);
        if (currentSpeed < -0.1f) tangent = -tangent;

        rb.MovePosition(railPosition);
        if (tangent != Vector3.zero)
        {
            // smoothed so that landing eases into the slope instead of snapping
            SmoothRotate(Quaternion.LookRotation(tangent, Vector3.up), groundAlignSpeed);
            if (Mathf.Abs(currentSpeed) > 0.1f) facing = Mathf.Sign(currentSpeed);
        }

        RememberSafeSpot();
    }

    private void UpdateAirborneMovement()
    {
        // horizontal air control, same feel as the old ground-speed logic
        float targetX = TargetSpeed();
        float rate = Mathf.Abs(targetX) > 0.01f ? acceleration : deceleration;
        airVelocity.x = Mathf.MoveTowards(airVelocity.x, targetX, rate * Time.fixedDeltaTime);

        float g = (airVelocity.y < 0f) ? baseGravity * fallGravityMultiplier : baseGravity;
        airVelocity.y = Mathf.Max(airVelocity.y + g * Time.fixedDeltaTime, -maxFallSpeed);

        airbornePosition.x += airVelocity.x * Time.fixedDeltaTime;
        airbornePosition.y += airVelocity.y * Time.fixedDeltaTime;

        rb.MovePosition(airbornePosition);

        currentSpeed = airVelocity.x;   // keeps rolling audio working
        if (Mathf.Abs(airVelocity.x) > 0.1f) facing = Mathf.Sign(airVelocity.x);

        // tilt to follow the arc; nose up while rising, nose down while falling
        // The cart starts from the slope angle it took off at and eases toward this
        float arcAngle = Mathf.Atan2(airVelocity.y, Mathf.Max(Mathf.Abs(airVelocity.x), 0.1f)) * airTiltAmount;
        Vector3 airForward = new Vector3(facing * Mathf.Cos(arcAngle), Mathf.Sin(arcAngle), 0f).normalized;
        SmoothRotate(Quaternion.LookRotation(airForward, Vector3.up), airRotateSpeed);
    }

    // frame-rate independent exponential smoothing toward a target rotation
    private void SmoothRotate(Quaternion target, float speed)
    {
        float k = 1f - Mathf.Exp(-speed * Time.fixedDeltaTime);
        visualRotation = Quaternion.Slerp(visualRotation, target, k);
        rb.MoveRotation(visualRotation);
    }

    /////////////////////////////////////////////////// landing /////////////////////////////////////////////////
    // Asks the spline MATH where the track surface is and lands when the cart crosses it
    private void CheckForRailLanding()
    {
        var lane = lanes[currentLane];
        float allowed = Mathf.Max(minSnapDistance, airVelocity.magnitude * Time.fixedDeltaTime * 2f);

        int bestSeg = -1;
        float bestSigned = 0f, bestT = 0f, bestAbs = float.MaxValue;
        Vector2 bestUp = Vector2.up;

        for (int i = 0; i < lane.segs.Count; i++)
        {
            SampleSurface(lane.segs[i], airbornePosition, lane.z, out Vector3 near, out Vector3 up, out _, out float t);

            Vector2 d = new Vector2(airbornePosition.x - near.x, airbornePosition.y - near.y);
            float signed = Vector2.Dot(d, up) - surfaceOffset;
            if (d.magnitude > allowed + 0.5f && signed > 0f) continue; // far above, not a candidate yet

            if (Mathf.Abs(signed) < bestAbs)
            {
                bestAbs = Mathf.Abs(signed);
                bestSigned = signed;
                bestSeg = i;
                bestT = t;
                bestUp = up;
            }
        }

        if (bestSeg < 0) { wasAboveSurface = true; return; }

        // land when we cross the surface while moving INTO it
        bool movingIntoSurface = Vector2.Dot(airVelocity, bestUp) <= 0f;
        bool crossed = wasAboveSurface && bestSigned <= 0f && bestSigned >= -allowed && movingIntoSurface;

        if (crossed)
        {
            ActivateSegment(bestSeg);
            currentDistance = Mathf.Clamp(bestT * activeSplineLength, 0f, activeSplineLength);

            float limit = boostTimer > 0f ? Mathf.Max(maxSpeed, boostSpeed) : maxSpeed;
            currentSpeed = Mathf.Clamp(airVelocity.x, -limit, limit);
            airVelocity = Vector2.zero;
            isGrounded = true;
            switching = false;
            zOffset = 0f;
            yHop = 0f;
            return;
        }

        wasAboveSurface = bestSigned > 0f;
    }

    private void SampleSurface(SplineContainer c, Vector3 worldPos, float laneZ,
                               out Vector3 nearest, out Vector3 up, out Vector2 tangent2D, out float t)
    {
        // query at the lane's own Z so lane depth never affects the XY result
        float3 local = c.transform.InverseTransformPoint(new Vector3(worldPos.x, worldPos.y, laneZ));
        SplineUtility.GetNearestPoint(c.Spline, local, out float3 _, out t, nearestResolution, nearestIterations);

        c.Evaluate(t, out float3 p, out float3 tan, out float3 _u);
        nearest = (Vector3)p;

        tangent2D = new Vector2(tan.x, tan.y);
        if (tangent2D.sqrMagnitude < 1e-6f) { tangent2D = Vector2.right; up = Vector3.up; return; }
        tangent2D.Normalize();
        up = new Vector3(-tangent2D.y, tangent2D.x, 0f);
        if (flipSurfaceNormal) up = -up;
    }

    private void RememberSafeSpot()
    {
        safeLane = currentLane;
        safeSeg = currentSplineIndex;
        safeDistance = currentDistance;
    }

    private void CheckFellOutOfWorld()
    {
        if (airbornePosition.y >= lanes[currentLane].minY - killDepth) return;

        currentLane = safeLane;
        ActivateSegment(safeSeg);
        currentDistance = safeDistance;
        airVelocity = Vector2.zero;
        isGrounded = true;
        switching = false;
        zOffset = 0f;
        yHop = 0f;
    }

    ////////////////////////////////////////////////// lane switching /////////////////////////////////////////////////
    private bool FindOnLane(int laneIndex, out int seg, out float t, out string why)
    {
        seg = 0; t = 0f; why = null;
        if (laneIndex < 0 || laneIndex >= lanes.Count)
        {
            why = $"no lane in that direction (on lane {currentLane}, {lanes.Count} lane(s) exist)";
            return false;
        }

        var lane = lanes[laneIndex];
        float bestDist = float.MaxValue;
        float minDx = float.MaxValue, minDy = float.MaxValue;
        bool found = false;
        Vector3 p = rb.position;

        for (int i = 0; i < lane.segs.Count; i++)
        {
            SampleSurface(lane.segs[i], p, lane.z, out Vector3 near, out _, out _, out float tt);
            float dx = Mathf.Abs(near.x - p.x), dy = Mathf.Abs(near.y - p.y);
            minDx = Mathf.Min(minDx, dx);
            minDy = Mathf.Min(minDy, dy);

            if (dx > maxHorizontalGap) continue;      // no track beside us
            if (dy > maxVerticalGap) continue;        // track is at a different height
            float d = dx + dy;
            if (d < bestDist) { bestDist = d; seg = i; t = tt; found = true; }
        }

        if (!found)
            why = $"lane {laneIndex} has no track beside the cart (closest: {minDx:F2} horizontal, {minDy:F2} vertical; limits {maxHorizontalGap}/{maxVerticalGap})";
        return found;
    }

    private int Dir(int input) => upGoesToHigherZ ? input : -input;

    private void UpdateSwitchAvailability()
    {
        if (switching) { CanSwitchUp = CanSwitchDown = false; return; }
        CanSwitchUp = FindOnLane(currentLane + Dir(+1), out _, out _, out _);
        CanSwitchDown = FindOnLane(currentLane + Dir(-1), out _, out _, out _);
    }

    private void TrySwitchLane(int input)
    {
        if (debugLogging) Debug.Log($"[{name}] vertical input {(input > 0 ? "UP" : "DOWN")} received");

        if (!isGrounded || switching)
        {
            if (debugLogging) Debug.Log($"[{name}] switch refused: {(switching ? "already switching" : "cart is airborne")}");
            return;
        }

        int target = currentLane + Dir(input);
        if (!FindOnLane(target, out int seg, out float t, out string why))
        {
            if (debugLogging) Debug.Log($"[{name}] switch refused: {why}");
            return;
        }

        float fromZ = rb.position.z;
        currentLane = target;
        ActivateSegment(seg);
        currentDistance = Mathf.Clamp(t * activeSplineLength, 0f, activeSplineLength);

        switchStartZOffset = fromZ - lanes[currentLane].z;
        zOffset = switchStartZOffset;
        switchT = 0f;
        switching = true;

        if (debugLogging)
            Debug.Log($"[{name}] switched to lane {currentLane}: Z {fromZ:F2} -> {lanes[currentLane].z:F2} (gap {lanes[currentLane].z - fromZ:F2})");
    }

    private void UpdateSwitchAnimation()
    {
        if (!switching) return;

        switchT += Time.fixedDeltaTime / Mathf.Max(0.01f, switchDuration);
        float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(switchT));
        zOffset = Mathf.Lerp(switchStartZOffset, 0f, k);
        yHop = hopHeight * Mathf.Sin(Mathf.PI * k);

        if (switchT >= 1f)
        {
            switching = false;
            zOffset = 0f;
            yHop = 0f;
            if (debugLogging)
                Debug.Log($"[{name}] switch finished: lane {currentLane} z={lanes[currentLane].z:F2}, cart rb z={rb.position.z:F2}, transform z={transform.position.z:F2}");
        }
    }

    /////////////////////////////////////////////////// audio /////////////////////////////////////////////////
    private void UpdateRollingAudio()
    {
        bool isMoving = Mathf.Abs(currentSpeed) > 0.1f;
        if (rollingAudioSource == null) return;

        if (isMoving && !rollingAudioSource.isPlaying) rollingAudioSource.Play();
        else if (!isMoving && rollingAudioSource.isPlaying) rollingAudioSource.Stop();
    }
}