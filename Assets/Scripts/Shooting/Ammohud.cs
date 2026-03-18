using UnityEngine;

/// <summary>
/// AmmoHUD.cs
///
/// Draws two things via OnGUI:
///
///   1. Bottom-right: ammo counter showing  3 / 6
///      A small ammo symbol (drawn with basic shapes) sits left of the numbers
///      so players know at a glance what the counter refers to.
///
///   2. Above the player (world-to-screen): reload progress bar + label,
///      visible only while reloading.
///
/// Attach to the Player GameObject (or any persistent object).
/// Assign the Revolver reference in the Inspector — drag the
/// BasicRevolver component into the slot.
/// </summary>
public class AmmoHUD : MonoBehaviour
{
    // ─── Inspector ────────────────────────────────────────────────────────────

    [Header("References")]
    [SerializeField] private Revolver  revolver;
    [SerializeField] private Transform playerTransform;

    [Header("Ammo Counter")]
    [SerializeField] private float screenMargin  = 24f;
    [SerializeField] private int   counterFontSize = 20;

    [Header("Reload Bar")]
    [SerializeField] private float barWidth   = 64f;
    [SerializeField] private float barHeight  = 8f;
    [SerializeField] private float barOffsetY = 48f;  // pixels above player

    // ─── Cached styles ────────────────────────────────────────────────────────

    private GUIStyle _counterStyle;
    private GUIStyle _hintStyle;
    private GUIStyle _reloadLabelStyle;
    private GUIStyle _barBgStyle;
    private GUIStyle _barFillStyle;
    private GUIStyle _symbolStyle;
    private bool     _ready;

    // ─── OnGUI ────────────────────────────────────────────────────────────────
    
    private void OnGUI()
    {
        if (revolver == null) return;

        EnsureStyles();
        DrawAmmoCounter();

        if (revolver.IsReloading)
            DrawReloadBar();
    }

    // ─── Ammo counter ─────────────────────────────────────────────────────────

    private void DrawAmmoCounter()
    {
        int current = revolver.CurrentAmmo;
        int total   = revolver.ChamberSize;
        bool empty  = current == 0 && !revolver.IsReloading;

        // Colour the number red when empty, white otherwise
        _counterStyle.normal.textColor = empty ? new Color(1f, 0.3f, 0.3f) : Color.white;

        string counterText = $"{current} / {total}";

        // Measure text width so we can anchor to bottom-right
        Vector2 textSize   = _counterStyle.CalcSize(new GUIContent(counterText));
        float symbolWidth  = 22f;   // space reserved for the drawn ammo symbol
        float totalWidth   = symbolWidth + 6f + textSize.x;
        float rowHeight    = Mathf.Max(textSize.y, 20f);

        float baseX = Screen.width  - screenMargin - totalWidth;
        float baseY = Screen.height - screenMargin - rowHeight;

        // ── Ammo symbol (hand-drawn with GL-style GUI rects) ──
        // A small bullet silhouette: rounded top cap + rectangular body
        DrawAmmoSymbol(baseX, baseY + (rowHeight - 20f) / 2f, 14f, 20f);

        // ── Counter text ──
        Rect textRect = new Rect(baseX + symbolWidth + 6f, baseY, textSize.x + 4f, rowHeight);
        GUI.Label(textRect, counterText, _counterStyle);

        // ── Empty hint ──
        if (empty)
        {
            Vector2 hintSize = _hintStyle.CalcSize(new GUIContent("RELOAD  [R]"));
            Rect hintRect = new Rect(
                Screen.width  - screenMargin - hintSize.x,
                Screen.height - screenMargin - rowHeight - hintSize.y - 6f,
                hintSize.x, hintSize.y
            );
            GUI.Label(hintRect, "RELOAD  [R]", _hintStyle);
        }
    }

    /// Draws a minimal bullet icon using plain GUI boxes.
    /// x,y = top-left of the icon area; w,h = icon bounds.
    private void DrawAmmoSymbol(float x, float y, float w, float h)
    {
        // Body (lower 70%)
        float bodyH = h * 0.70f;
        float bodyY = y + h - bodyH;
        GUI.Box(new Rect(x + w * 0.15f, bodyY, w * 0.70f, bodyH), GUIContent.none, _barFillStyle);

        // Tip cap (upper 35%, narrower)
        float tipW = w * 0.46f;
        float tipH = h * 0.35f;
        GUI.Box(new Rect(x + (w - tipW) / 2f, y, tipW, tipH), GUIContent.none, _barFillStyle);
    }

    // ─── Reload bar ───────────────────────────────────────────────────────────

    private void DrawReloadBar()
    {
        if (playerTransform == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(playerTransform.position);
        float   guiX      = screenPos.x - barWidth / 2f;
        float   guiY      = Screen.height - screenPos.y - barOffsetY;

        // Background
        GUI.Box(new Rect(guiX, guiY, barWidth, barHeight), GUIContent.none, _barBgStyle);

        // Fill — colour shifts from green → yellow as it completes
        float t = revolver.ReloadProgress;
        _barFillStyle.normal.background = MakeTex(Color.Lerp(
            new Color(0.2f, 0.85f, 0.3f),
            new Color(1f,   0.85f, 0.1f),
            t
        ));
        GUI.Box(new Rect(guiX, guiY, barWidth * t, barHeight), GUIContent.none, _barFillStyle);

        // Label
        Rect labelRect = new Rect(guiX - 8f, guiY - 18f, barWidth + 16f, 16f);
        GUI.Label(labelRect, "Reloading...", _reloadLabelStyle);
    }

    // ─── Style helpers ────────────────────────────────────────────────────────

    private void EnsureStyles()
    {
        if (_ready) return;

        _counterStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = counterFontSize,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        _counterStyle.normal.textColor = Color.white;

        _hintStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 12,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleRight
        };
        _hintStyle.normal.textColor = new Color(1f, 0.4f, 0.4f);

        _reloadLabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 11,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _reloadLabelStyle.normal.textColor = Color.white;

        _barBgStyle = new GUIStyle(GUI.skin.box);
        _barBgStyle.normal.background = MakeTex(new Color(0.1f, 0.1f, 0.1f, 0.75f));

        _barFillStyle = new GUIStyle(GUI.skin.box);
        _barFillStyle.normal.background = MakeTex(new Color(0.2f, 0.85f, 0.3f));

        _symbolStyle = new GUIStyle(GUI.skin.box);
        _symbolStyle.normal.background = MakeTex(new Color(1f, 0.85f, 0.2f));

        _ready = true;
    }

    private Texture2D MakeTex(Color c)
    {
        var t = new Texture2D(1, 1);
        t.SetPixel(0, 0, c);
        t.Apply();
        return t;
    }
}