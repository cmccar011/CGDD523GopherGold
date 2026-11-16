using UnityEngine;

[DefaultExecutionOrder(1000)]
public class ScreenShake : MonoBehaviour
{
    private static ScreenShake instance;

    private const float Frequency = 40f;

    private float amplitude;
    private float duration;
    private float timer;
    private float seed;

    private Vector3 lastOffset;
    private Vector3 appliedPosition;
    private bool applied;

    public static void Shake(float amplitude, float duration)
    {
        if (amplitude <= 0f || duration <= 0f) return;

        if (instance == null)
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            instance = cam.GetComponent<ScreenShake>();
            if (instance == null) instance = cam.gameObject.AddComponent<ScreenShake>();
        }

        instance.Begin(amplitude, duration);
    }

    private void Begin(float newAmplitude, float newDuration)
    {
        float remaining = timer > 0f ? amplitude * Mathf.Clamp01(timer / duration) : 0f;

        amplitude = Mathf.Max(remaining, newAmplitude);
        duration = newDuration;
        timer = newDuration;
        seed = Random.value * 100f;
    }

    private void LateUpdate()
    {
        Transform t = transform;
        Vector3 pos = t.position;

        if (applied && (pos - appliedPosition).sqrMagnitude < 1e-10f) pos -= lastOffset;

        Vector3 offset = Vector3.zero;

        if (timer > 0f)
        {
            timer -= Time.unscaledDeltaTime;

            float k = Mathf.Clamp01(timer / duration);
            float time = Time.unscaledTime * Frequency;
            float x = Mathf.PerlinNoise(seed, time) - 0.5f;
            float y = Mathf.PerlinNoise(seed + 17f, time) - 0.5f;

            offset = (t.right * x + t.up * y) * 2f * amplitude * k * k;
        }

        t.position = pos + offset;
        lastOffset = offset;
        appliedPosition = t.position;
        applied = true;
    }
}