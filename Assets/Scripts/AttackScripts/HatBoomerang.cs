using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MinecartController))]
public class HatBoomerang : MonoBehaviour
{
    [Header("Hat")]
    [SerializeField] private GameObject hatPrefab;
    [Tooltip("Used when there's no prefab")]
    [SerializeField] private Sprite hatSprite;
    [SerializeField] private Transform throwOrigin;
    [Tooltip("Where the hat appears relative to the cart when there's no Throw Origin (x is flipped to face forward)")]
    [SerializeField] private Vector2 originOffset = new Vector2(1f, 0.6f);
    [Tooltip("Optional: the helmet on the sprite, hidden while the hat is out")]
    [SerializeField] private GameObject helmetToHide;
    [SerializeField] private float spinSpeed = 720f;

    [Header("Charge (hold the button)")]
    [SerializeField] private float minDistance = 4f;
    [SerializeField] private float maxDistance = 16f;
    [SerializeField] private float maxChargeTime = 1.2f;

    [Header("Flight")]
    [SerializeField] private float minSpeed = 8f;
    [SerializeField] private float maxSpeed = 28f;
    [Tooltip("0 = start of a leg, 1 = end of a leg. Same curve is used going out and coming back.")]
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("Off: the return leg speeds up again from the far point (same cadence as the throw). On: the return is fastest at the far point and slows near you.")]
    [SerializeField] private bool returnMirrorsDistance = false;
    [SerializeField] private float catchDistance = 1.2f;
    [SerializeField] private float maxFlightTime = 8f;

    [Header("Hit")]
    [SerializeField] private float hitRadius = 1f;
    [Tooltip("How far apart in Z the hat and a cart can be and still hit. Keep it smaller than the gap between lanes.")]
    [SerializeField] private float laneTolerance = 0.75f;
    [SerializeField] private bool canHitOnReturn = true;
    [SerializeField] private float stunDuration = 1.5f;
    [Tooltip("How long the two stun sprites swap for. Clamped to the stun duration.")]
    [SerializeField] private float stunAnimDuration = 0.8f;
    [SerializeField] private float liftHeight = 1f;
    [SerializeField] private float liftDuration = 1f;
    [SerializeField] private float shakeAmplitude = 0.3f;
    [SerializeField] private float shakeDuration = 0.25f;

    [Header("Debug")]
    [SerializeField] private bool debugLogging = true;

    private MinecartController cart;
    private HatProjectile activeHat;
    private bool charging;
    private float chargeTime;
    private InputAction heldAction;
    private bool helmetHidden;

    public bool HatOut => activeHat != null;
    public bool DebugLogging => debugLogging;
    public float SpinSpeed => spinSpeed;
    public float CatchDistance => catchDistance;
    public float MaxFlightTime => maxFlightTime;
    public float HitRadius => hitRadius;
    public float LaneTolerance => laneTolerance;
    public bool CanHitOnReturn => canHitOnReturn;
    public bool ReturnMirrorsDistance => returnMirrorsDistance;
    public float StunDuration => stunDuration;
    public float StunAnimDuration => stunAnimDuration;
    public float LiftHeight => liftHeight;
    public float LiftDuration => liftDuration;
    public float ShakeAmplitude => shakeAmplitude;
    public float ShakeDuration => shakeDuration;
    public Vector3 CatchPoint => throwOrigin != null ? throwOrigin.position : cart.transform.position + Vector3.up * originOffset.y;

    private void Awake()
    {
        cart = GetComponent<MinecartController>();

        if (hatPrefab == null && hatSprite == null)
            Debug.LogWarning($"[{name}] HatBoomerang: no Hat Prefab or Hat Sprite assigned, so the hat will be invisible.", this);
    }

    private bool CanAct => cart.enabled && !cart.IsOut && !cart.Status.IsStunned;

    public void OnBoomerang(InputValue value)
    {
        if (debugLogging)
            Debug.Log($"[{name}] OnBoomerang {(value.isPressed ? "pressed" : "released")} (canAct={CanAct}, hatOut={HatOut}, charging={charging})");

        if (value.isPressed)
        {
            if (charging)
            {
                if (heldAction == null) FinishCharge();
                return;
            }

            if (!CanAct || HatOut) return;

            charging = true;
            chargeTime = 0f;
            heldAction = FindBoomerangAction();

            if (heldAction == null)
                Debug.LogWarning($"[{name}] HatBoomerang: couldn't find a 'Boomerang' action to read the release from. Press the button again to throw.", this);
        }
        else if (charging)
        {
            FinishCharge();
        }
    }

    private InputAction FindBoomerangAction()
    {
        PlayerInput playerInput = GetComponentInParent<PlayerInput>();
        if (playerInput == null) return null;

        InputAction action = playerInput.currentActionMap != null ? playerInput.currentActionMap.FindAction("Boomerang", false) : null;
        if (action == null && playerInput.actions != null) action = playerInput.actions.FindAction("Boomerang", false);
        return action;
    }

    private void FinishCharge()
    {
        charging = false;
        heldAction = null;

        if (CanAct) Throw(Mathf.Lerp(minDistance, maxDistance, Mathf.Clamp01(chargeTime / Mathf.Max(0.01f, maxChargeTime))));
    }

    private void Update()
    {
        if (charging)
        {
            if (!CanAct)
            {
                charging = false;
                heldAction = null;
            }
            else
            {
                chargeTime = Mathf.Min(chargeTime + Time.deltaTime, maxChargeTime);
                if (heldAction != null && !heldAction.IsPressed()) FinishCharge();
            }
        }

        if (helmetToHide != null && helmetHidden != HatOut)
        {
            helmetHidden = HatOut;
            helmetToHide.SetActive(!helmetHidden);
        }
    }

    private void Throw(float distance)
    {
        Vector3 fwd = cart.transform.forward;
        Vector2 dir = new Vector2(fwd.x, fwd.y);
        if (dir.sqrMagnitude < 0.01f) dir = Vector2.right;
        dir.Normalize();

        Vector3 origin = throwOrigin != null
            ? throwOrigin.position
            : cart.transform.position + new Vector3(Mathf.Sign(dir.x == 0f ? 1f : dir.x) * originOffset.x, originOffset.y, 0f);
        origin.z = cart.LaneZ;

        if (debugLogging) Debug.Log($"[{name}] throwing hat, distance {distance:F1}, direction {dir}");

        GameObject go = hatPrefab != null ? Instantiate(hatPrefab, origin, Quaternion.identity) : MakeDefaultHat(origin);

        if (!go.TryGetComponent(out HatProjectile projectile)) projectile = go.AddComponent<HatProjectile>();
        projectile.Launch(this, cart, dir, distance);
        activeHat = projectile;
    }

    private GameObject MakeDefaultHat(Vector3 position)
    {
        GameObject go = new GameObject("HatBoomerang");
        go.transform.position = position;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = hatSprite;
        sr.sortingOrder = 10;
        return go;
    }

    public float SpeedAt(float t)
    {
        return Mathf.Lerp(minSpeed, maxSpeed, speedCurve.Evaluate(Mathf.Clamp01(t)));
    }
}