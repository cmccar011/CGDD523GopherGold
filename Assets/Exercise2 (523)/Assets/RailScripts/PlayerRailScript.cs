using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;

public class PlayerRailScript : MonoBehaviour
{
    [Header("Inputs")]
    [SerializeField] private bool jumpInput;

    [Header("Variables")]
    public bool onRail;
    public bool isJumping;
    [SerializeField] private float grindSpeed = 15f;
    [SerializeField] private float heightOffset = 0.5f; // Normal riding height above spline
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = 20f;
    [SerializeField] private float lerpSpeed = 20f;

    private float timeForFullSpline;
    private float elapsedTime;
    private float currentJumpVelocity;
    private float currentHeightOffset;

    [Header("Scripts")]
    [SerializeField] private RailScript currentRailScript;

    private void Start()
    {
        currentHeightOffset = heightOffset;
    }

    public void HandleJump(InputAction.CallbackContext context)
    {
        if (context.performed && onRail && !isJumping)
        {
            PerformJump();
        }
    }

    private void PerformJump()
    {
        isJumping = true;
        currentJumpVelocity = jumpForce;
    }

    private void FixedUpdate()
    {
        if (onRail)
        {
            MovePlayerAlongRail();
        }
    }

    private void MovePlayerAlongRail()
    {
        if (currentRailScript == null) return;

        // Advance progress along the spline
        float delta = Time.fixedDeltaTime;
        if (currentRailScript.Dir)
            elapsedTime += delta;
        else
            elapsedTime -= delta;

        float progress = elapsedTime / timeForFullSpline;

        // Check if reaching the end of spline
        if (progress < 0f || progress > 1f)
        {
            ThrowOffRail();
            return;
        }

        // Evaluate positions and orientations in Local space
        float3 localPos, localTangent, localUp;
        SplineUtility.Evaluate(currentRailScript.railSpline.Spline, progress, out localPos, out localTangent, out localUp);

        // Convert coordinates to World space
        Vector3 worldPos = currentRailScript.LocalToWorldConversion(localPos);
        Vector3 worldForward = currentRailScript.transform.TransformDirection(localTangent);
        Vector3 worldUp = currentRailScript.transform.TransformDirection(localUp);

        // Reverse forward vector if traveling backward
        if (!currentRailScript.Dir)
        {
            worldForward = -worldForward;
        }

        // Apply jump vertical physics relative to the rail offset
        if (isJumping)
        {
            currentJumpVelocity -= gravity * Time.fixedDeltaTime;
            currentHeightOffset += currentJumpVelocity * Time.fixedDeltaTime;

            // Landing check
            if (currentHeightOffset <= heightOffset)
            {
                currentHeightOffset = heightOffset;
                isJumping = false;
                currentJumpVelocity = 0f;
            }
        }

        // Align cart position & rotation with rail alignment
        Vector3 targetPosition = worldPos + (worldUp * currentHeightOffset);
        Quaternion targetRotation = Quaternion.LookRotation(worldForward, worldUp);

        transform.position = targetPosition;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lerpSpeed * Time.fixedDeltaTime);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Rail") && !onRail)
        {
            currentRailScript = hit.gameObject.GetComponent<RailScript>();
            if (currentRailScript != null)
            {
                AttachToRail();
            }
        }
    }

    private void AttachToRail()
    {
        onRail = true;
        isJumping = false;
        currentHeightOffset = heightOffset;

        // Calculate travel time based on distance and speed
        timeForFullSpline = currentRailScript.totalSplineLength / grindSpeed;

        // Find initial attachment time along spline
        Vector3 splinePoint;
        float normalizedTime = currentRailScript.CalculateTargetRailPoint(transform.position, out splinePoint);
        elapsedTime = timeForFullSpline * normalizedTime;

        // Evaluate tangent to resolve direction facing
        float3 pos, tangent, up;
        SplineUtility.Evaluate(currentRailScript.railSpline.Spline, normalizedTime, out pos, out tangent, out up);
        Vector3 worldTangent = currentRailScript.transform.TransformDirection(tangent);

        currentRailScript.CalculateDirection(worldTangent, transform.forward);
    }

    private void ThrowOffRail()
    {
        onRail = false;
        isJumping = false;
        currentRailScript = null;
    }
}