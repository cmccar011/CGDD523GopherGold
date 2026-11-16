using UnityEngine;

public class HatProjectile : MonoBehaviour
{
    private HatBoomerang owner;
    private MinecartController thrower;

    private Vector2 direction;
    private float targetDistance;
    private float throwZ;

    private bool returning;
    private float legTravelled;
    private float returnLegLength;
    private float age;

    public void Launch(HatBoomerang hatOwner, MinecartController who, Vector2 dir, float distance)
    {
        owner = hatOwner;
        thrower = who;
        direction = dir;
        targetDistance = Mathf.Max(0.1f, distance);
        throwZ = transform.position.z;
    }

    private void Update()
    {
        if (owner == null || thrower == null || thrower.IsOut)
        {
            Destroy(gameObject);
            return;
        }

        float dt = Time.deltaTime;
        age += dt;
        if (age > owner.MaxFlightTime)
        {
            Destroy(gameObject);
            return;
        }

        transform.Rotate(0f, 0f, owner.SpinSpeed * dt, Space.World);

        Vector3 pos = transform.position;

        if (!returning)
        {
            float speed = owner.SpeedAt(legTravelled / targetDistance);
            float step = speed * dt;

            pos += new Vector3(direction.x, direction.y, 0f) * step;
            pos.z = throwZ;
            legTravelled += step;

            if (legTravelled >= targetDistance) BeginReturn(pos);
        }
        else
        {
            Vector3 to = owner.CatchPoint - pos;
            float dist = to.magnitude;

            float t = owner.ReturnMirrorsDistance
                ? dist / targetDistance
                : legTravelled / Mathf.Max(0.1f, returnLegLength);

            float speed = owner.SpeedAt(t) + Mathf.Abs(thrower.CurrentSpeed);
            float step = speed * dt;

            if (dist <= owner.CatchDistance || dist <= step)
            {
                Destroy(gameObject);
                return;
            }

            pos += to / dist * step;
            legTravelled += step;
        }

        transform.position = pos;
        CheckHits(pos);
    }

    private void BeginReturn(Vector3 from)
    {
        returning = true;
        legTravelled = 0f;
        returnLegLength = Mathf.Max(0.1f, (owner.CatchPoint - from).magnitude);
    }

    private void CheckHits(Vector3 pos)
    {
        if (returning && !owner.CanHitOnReturn) return;

        float radiusSqr = owner.HitRadius * owner.HitRadius;

        foreach (MinecartController cart in MinecartController.All)
        {
            if (cart == thrower || cart.IsOut) continue;

            CartStatus status = cart.Status;
            if (status == null || status.IsProtected) continue;
            if (Mathf.Abs(cart.LaneZ - pos.z) > owner.LaneTolerance) continue;

            Vector3 p = cart.transform.position;
            float dx = p.x - pos.x, dy = p.y - pos.y;
            if (dx * dx + dy * dy > radiusSqr) continue;

            if (status.Stun(owner.StunDuration, owner.StunAnimDuration, owner.LiftHeight, owner.LiftDuration))
            {
                if (owner.DebugLogging) Debug.Log($"[{thrower.name}] hat hit {cart.name}");
                ScreenShake.Shake(owner.ShakeAmplitude, owner.ShakeDuration);
                if (!returning) BeginReturn(pos);
                return;
            }
        }
    }
}