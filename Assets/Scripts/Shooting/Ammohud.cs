using UnityEngine;

public class AmmoHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Revolver  revolver;
    [SerializeField] private Transform playerTransform;

    [Header("Ammo Counter")]
    [SerializeField] private float screenMargin    = 24f;
    [SerializeField] private int   counterFontSize = 20;
    [SerializeField] private Font  counterFont;

    [Header("Reload Bar")]
    [SerializeField] private float barWidth   = 64f;
    [SerializeField] private float barHeight  = 8f;
    [SerializeField] private float barOffsetY = 48f;

    private GUIStyle _counterStyle;
    private GUIStyle _hintStyle;
    private GUIStyle _reloadLabelStyle;
    private GUIStyle _barBgStyle;
    private GUIStyle _barFillStyle;
    private bool     _ready;

    private void Awake()
    {
        if (revolver == null && playerTransform != null)
            revolver = playerTransform.GetComponent<Revolver>();

        if (revolver == null)
            Debug.LogError("[AmmoHUD] No Revolver found.");
    }

    private void OnGUI()
    {
        if (revolver == null) return;

        EnsureStyles();
        DrawAmmoCounter();

        if (revolver.IsReloading)
            DrawReloadBar();
    }

    private void DrawAmmoCounter()
    {
        int  current = revolver.CurrentAmmo;
        int  total   = revolver.ChamberSize;
        bool empty   = current == 0 && !revolver.IsReloading;

        _counterStyle.normal.textColor = empty ? new Color(1f, 0.3f, 0.3f) : Color.white;

        string counterText = $"{current} / {total}";
        Vector2 textSize   = _counterStyle.CalcSize(new GUIContent(counterText));

        float symbolWidth = 22f;
        float totalWidth  = symbolWidth + 6f + textSize.x;
        float rowHeight   = Mathf.Max(textSize.y, 20f);

        float baseX = Screen.width  - screenMargin - totalWidth;
        float baseY = Screen.height - screenMargin - rowHeight;

        DrawAmmoSymbol(baseX, baseY + (rowHeight - 20f) / 2f, 14f, 20f);

        GUI.Label(new Rect(baseX + symbolWidth + 6f, baseY, textSize.x + 4f, rowHeight),
                  counterText, _counterStyle);

        if (empty)
        {
            Vector2 hintSize = _hintStyle.CalcSize(new GUIContent("RELOAD  [R]"));
            GUI.Label(new Rect(
                Screen.width  - screenMargin - hintSize.x,
                Screen.height - screenMargin - rowHeight - hintSize.y - 6f,
                hintSize.x, hintSize.y
            ), "RELOAD  [R]", _hintStyle);
        }
    }

    private void DrawAmmoSymbol(float x, float y, float w, float h)
    {
        float bodyH = h * 0.70f;
        float bodyY = y + h - bodyH;
        GUI.Box(new Rect(x + w * 0.15f, bodyY, w * 0.70f, bodyH), GUIContent.none, _barFillStyle);

        float tipW = w * 0.46f;
        float tipH = h * 0.35f;
        GUI.Box(new Rect(x + (w - tipW) / 2f, y, tipW, tipH), GUIContent.none, _barFillStyle);
    }

    private void DrawReloadBar()
    {
        if (playerTransform == null) return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(playerTransform.position);
        float   guiX      = screenPos.x - barWidth / 2f;
        float   guiY      = Screen.height - screenPos.y - barOffsetY;

        GUI.Box(new Rect(guiX, guiY, barWidth, barHeight), GUIContent.none, _barBgStyle);

        float t = revolver.ReloadProgress;
        _barFillStyle.normal.background = MakeTex(Color.Lerp(
            new Color(0.2f, 0.85f, 0.3f),
            new Color(1f,   0.85f, 0.1f),
            t
        ));
        GUI.Box(new Rect(guiX, guiY, barWidth * t, barHeight), GUIContent.none, _barFillStyle);

        GUI.Label(new Rect(guiX - 8f, guiY - 18f, barWidth + 16f, 16f),
                  "Reloading...", _reloadLabelStyle);
    }

    private void EnsureStyles()
    {
        if (_ready) return;

        _counterStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = counterFontSize,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        _counterStyle.font = counterFont;
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
