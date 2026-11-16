using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MinecartController))]
public class CartStatus : MonoBehaviour
{
    [Header("Stun Anim")] //swaps btwn two sprites for simulated animation
    [SerializeField] private SpriteRenderer stunRenderer;
    [SerializeField] private Sprite stunSpriteA;
    [SerializeField] private Sprite stunSpriteB;
    [SerializeField] private float stunSpriteInterval = 0.12f;

    [Header("Invulnerability")]
    [SerializeField] private float blinkHz = 8f; //visual for invuln
    [SerializeField] private float graceAfterStun = 0.5f;

    public bool IsStunned { get; private set; }
    public bool IsInvulnerable => invulnTimer > 0f;
    public bool IsProtected => IsStunned || IsInvulnerable;
    public float Lift { get; private set; }

    private MinecartController cart;

    private float stunTimer;
    private float animTimer;
    private float animClock;
    private Sprite savedSprite;
    private bool savedEnabled;
    private bool animating;

    private float liftTimer, liftDuration, liftHeight;

    private float invulnTimer;
    private float blinkClock;
    private readonly List<Renderer> blinkRenderers = new List<Renderer>();

    private void Awake()
    {
        cart = GetComponent<MinecartController>();
    }

    private void Update()
    {
        float dt = Time.deltaTime; //gonna be using this a lot

        if (stunTimer > 0f)
        {
            stunTimer -= dt;
            if (stunTimer <= 0f) EndStun();
        }

        if (animating)
        {
            animTimer -= dt;
            animClock += dt;

            if (animTimer <= 0f) EndStunSprites();
            else stunRenderer.sprite = ((int)(animClock / Mathf.Max(0.01f, stunSpriteInterval)) % 2 == 0) ? stunSpriteA : stunSpriteB;
        }

        if (liftTimer > 0f)
        {
            liftTimer -= dt;
            float k = Mathf.Clamp01(1f - liftTimer / liftDuration);
            Lift = liftHeight * 4f * k * (1f - k);
            if (liftTimer <= 0f) Lift = 0f;
        }

        if (invulnTimer > 0f)
        {
            invulnTimer -= dt;
            blinkClock += dt * blinkHz;
            bool visible = blinkClock % 1f < 0.5f;
            foreach (Renderer r in blinkRenderers) if (r != null) r.enabled = visible;
            if (invulnTimer <= 0f) StopBlink();
        }
    }

    public bool Stun(float duration, float animDuration, float height, float liftTime)
    {
        if (IsProtected || cart.IsOut || duration <= 0f) return false;

        IsStunned = true;
        stunTimer = duration;

        if (cart.IsGrounded && height > 0f && liftTime > 0f)
        {
            liftHeight = height;
            liftDuration = liftTime;
            liftTimer = liftTime;
        }

        BeginStunSprites(Mathf.Min(animDuration, duration));
        return true;
    }

    public void SetInvulnerable(float seconds)
    {
        if (seconds <= 0f) return;

        invulnTimer = Mathf.Max(invulnTimer, seconds);

        if (blinkRenderers.Count == 0)
        {
            Transform root = cart.VisualRoot;
            foreach (Renderer r in root.GetComponentsInChildren<Renderer>())
                if (r.enabled) blinkRenderers.Add(r);
            blinkClock = 0f;
        }
    }

    public void CancelAll() //ends all of the haptics
    {
        IsStunned = false;
        stunTimer = 0f;
        liftTimer = 0f;
        Lift = 0f;
        invulnTimer = 0f;
        if (animating) EndStunSprites();
        StopBlink();
    }

    private void EndStun()
    {
        IsStunned = false;
        SetInvulnerable(graceAfterStun);
    }

    private void BeginStunSprites(float duration)
    {
        if (stunRenderer == null || stunSpriteA == null || stunSpriteB == null || duration <= 0f) return;

        if (!animating)
        {
            savedSprite = stunRenderer.sprite;
            savedEnabled = stunRenderer.enabled;
            if (!savedEnabled) stunRenderer.enabled = true;
            animating = true;
        }

        animTimer = duration;
        animClock = 0f;
    }

    private void EndStunSprites()
    {
        animating = false;
        if (stunRenderer == null) return;

        stunRenderer.sprite = savedSprite;
        if (!savedEnabled) stunRenderer.enabled = false;
    }

    private void StopBlink()
    {
        foreach (Renderer r in blinkRenderers) if (r != null) r.enabled = true;
        blinkRenderers.Clear();
    }
}