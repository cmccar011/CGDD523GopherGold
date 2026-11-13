using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

public class RailScript : MonoBehaviour
{
    public bool Dir = true; // True = Forward, False = Backward
    public SplineContainer railSpline;
    public float totalSplineLength;

    private void Awake()
    {
        railSpline = GetComponent<SplineContainer>();
        if (railSpline != null)
        {
            totalSplineLength = railSpline.CalculateLength();
        }
    }

    public Vector3 LocalToWorldConversion(float3 localPoint)
    {
        return transform.TransformPoint(localPoint);
    }

    public float3 WorldToLocalConversion(Vector3 worldPoint)
    {
        return transform.InverseTransformPoint(worldPoint);
    }

    public float CalculateTargetRailPoint(Vector3 playerPos, out Vector3 worldPosOnSpline)
    {
        float3 nearestPoint;
        float time;
        SplineUtility.GetNearestPoint(railSpline.Spline, WorldToLocalConversion(playerPos), out nearestPoint, out time);
        worldPosOnSpline = LocalToWorldConversion(nearestPoint);
        return time;
    }

    public void CalculateDirection(Vector3 railForwardWorld, Vector3 playerForwardWorld)
    {
        float dot = Vector3.Dot(railForwardWorld.normalized, playerForwardWorld.normalized);
        Dir = dot >= 0f;
    }
}