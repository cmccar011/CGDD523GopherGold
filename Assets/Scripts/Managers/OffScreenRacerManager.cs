using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OffScreenRacerManager : MonoBehaviour
{
    private class Entry
    {
        public MinecartController cart;
        public int gopherIndex;
        public Image icon;
        public bool inGroup = true;
        public bool respawning;
        public float offTimer;
        public float immunity;
        public float phase;
    }

    [Header("References")]
    [SerializeField] private RaceCameraTargets targets;
    [SerializeField] private Camera cam;
    [SerializeField] private Canvas overlayCanvas;
    [SerializeField] private Sprite[] gopherIcons = new Sprite[4]; //same order as the character select buttons: Jane, Timmy, Toby, Pope

    [Header("Falling behind")]
    [SerializeField] private bool dropBehindRacersFromCamera = true;
    [SerializeField] private float maxLeaderGap = 35f;
    [SerializeField, Range(0.3f, 1f)] private float rejoinGapFraction = 0.7f;

    [Header("Warning icon")]
    [SerializeField] private float iconDelay = 1f;
    [SerializeField] private float iconSize = 96f;
    [SerializeField] private float edgePadding = 16f;
    [SerializeField] private float minBlinkHz = 1.5f;
    [SerializeField] private float maxBlinkHz = 12f;
    [SerializeField, Range(0f, 1f)] private float blinkOffAlpha = 0.15f;

    [Header("Despawn")] //how far past the left edge, in screen widths, before they are removed
    [SerializeField] private float despawnDistance = 0.5f;
    [SerializeField] private float despawnAfterSeconds = 8f;

    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 1.25f;
    [SerializeField, Range(0f, 1f)] private float respawnViewportX = 0.3f;
    [SerializeField] private float respawnHeight = 8f;
    [SerializeField] private float boostSpeed = 40f; //testing this bc they need to catch up somehow, 40 was way too freakin' fast,
                                                     //lowered it to like 15/20 and it feels *okay*, def needs more testing
    [SerializeField] private float boostSeconds = 3f; //this was OP, brought that shit down to like 2 seconds, might need to be 1 haha
    [SerializeField] private float respawnImmunity = 2.5f; //logic for when/if we add the boomerang hats!

    private readonly List<Entry> entries = new List<Entry>();
    private bool built; //as in camera built

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        if (targets == null) targets = FindAnyObjectByType<RaceCameraTargets>();
    }

    private void Build()
    {
        if (targets == null || targets.Racers.Count == 0) return;

        EnsureCanvas();

        foreach (RaceCameraTargets.Racer r in targets.Racers)
        {
            Sprite sprite = gopherIcons != null && r.gopherIndex >= 0 && r.gopherIndex < gopherIcons.Length
                ? gopherIcons[r.gopherIndex]
                : null;

            entries.Add(new Entry
            {
                cart = r.cart,
                gopherIndex = r.gopherIndex,
                icon = MakeIcon(sprite, r.cart.name)
            });
        }

        built = true;
    }

    private void EnsureCanvas()
    {
        if (overlayCanvas != null) { overlayCanvas = overlayCanvas.rootCanvas; return; }

        GameObject go = new GameObject("OffscreenIconCanvas", typeof(Canvas));
        overlayCanvas = go.GetComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 50;
    }

    private Image MakeIcon(Sprite sprite, string racerName)
    {
        GameObject go = new GameObject("OffscreenIcon_" + racerName, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(overlayCanvas.transform, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(iconSize, iconSize);

        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;

        go.SetActive(false);
        return img;
    }

    private void Update()
    {
        if (cam == null) { cam = Camera.main; return; }
        if (!built) { Build(); return; }

        float leaderX = float.MinValue;
        foreach (Entry e in entries)
        {
            if (e.cart == null || e.respawning || !e.cart.enabled) continue;
            leaderX = Mathf.Max(leaderX, e.cart.transform.position.x);
        }

        foreach (Entry e in entries)
        {
            if (e.cart == null || e.respawning) continue;

            if (!e.cart.enabled)
            {
                e.offTimer = 0f;
                SetIconActive(e, false);
                continue;
            }

            Tick(e, leaderX);
        }
    }

    private void Tick(Entry e, float leaderX)
    {
        Vector3 pos = e.cart.transform.position;
        Vector3 vp = cam.WorldToViewportPoint(pos);
        float gap = leaderX - pos.x;
        bool isLeader = gap <= 0.01f;
        bool offLeft = vp.z > 0f && vp.x < 0f;

        if (e.immunity > 0f) e.immunity -= Time.deltaTime;

        if (dropBehindRacersFromCamera)
        {
            if (e.inGroup && !isLeader && (gap > maxLeaderGap || offLeft))
            {
                targets.SetTracked(e.cart, false);
                e.inGroup = false;
            }
            else if (!e.inGroup && vp.x >= 0f && gap < maxLeaderGap * rejoinGapFraction)
            {
                targets.SetTracked(e.cart, true);
                e.inGroup = true;
            }
        }

        if (!offLeft || e.immunity > 0f)
        {
            e.offTimer = 0f;
            SetIconActive(e, false);
            return;
        }

        e.offTimer += Time.deltaTime;

        float distanceDanger = Mathf.InverseLerp(0f, despawnDistance, -vp.x);
        float timeDanger = Mathf.InverseLerp(iconDelay, despawnAfterSeconds, e.offTimer);
        float danger = Mathf.Max(distanceDanger, timeDanger);

        if (danger >= 1f)
        {
            StartCoroutine(RespawnRoutine(e));
            return;
        }

        if (e.offTimer >= iconDelay || distanceDanger > 0.5f) ShowIcon(e, pos, danger);
        else SetIconActive(e, false);
    }

    private void ShowIcon(Entry e, Vector3 worldPos, float danger)
    {
        if (e.icon == null) return;

        float scale = overlayCanvas.scaleFactor;
        float sizePx = iconSize * scale;
        Rect view = cam.pixelRect;

        Vector3 sp = cam.WorldToScreenPoint(worldPos);
        float pad = edgePadding * scale + sizePx * 0.5f;
        float x = view.xMin + pad;
        float y = Mathf.Clamp(sp.y, view.yMin + pad, view.yMax - pad);

        e.icon.rectTransform.anchoredPosition = new Vector2(x, y) / scale;

        float hz = Mathf.Lerp(minBlinkHz, maxBlinkHz, danger);
        e.phase += hz * Time.deltaTime;
        bool on = e.phase % 1f < 0.5f;

        Color c = e.icon.color;
        c.a = on ? 1f : blinkOffAlpha;
        e.icon.color = c;

        SetIconActive(e, true);
    }

    private void SetIconActive(Entry e, bool active)
    {
        if (e.icon == null) return;
        if (e.icon.gameObject.activeSelf != active) e.icon.gameObject.SetActive(active);
        if (!active) e.phase = 0f;
    }

    private IEnumerator RespawnRoutine(Entry e)
    {
        e.respawning = true;
        SetIconActive(e, false);

        targets.SetTracked(e.cart, false);
        e.inGroup = false;
        e.cart.Eliminate();

        yield return new WaitForSeconds(respawnDelay);
        if (e.cart == null) yield break;

        e.cart.RespawnAbove(WorldXAtViewport(respawnViewportX, e.cart.LaneZ), respawnHeight, boostSpeed, boostSeconds);

        e.cart.Status.SetInvulnerable(respawnImmunity);

        targets.SetTracked(e.cart, true);
        e.inGroup = true;
        e.offTimer = 0f;
        e.immunity = respawnImmunity;
        e.respawning = false;
    }

    private float WorldXAtViewport(float viewportX, float z)
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(viewportX, 0.5f, 0f));
        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, z));
        return plane.Raycast(ray, out float d) ? ray.GetPoint(d).x : cam.transform.position.x;
    }
}