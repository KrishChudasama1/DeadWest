using UnityEngine;
using System.Collections;

/// <summary>
/// PlayerShooting.cs
///
/// Detects left mouse click, converts cursor position to a world-space
/// shoot direction, and delegates to whatever IWeapon is equipped.
/// Also handles the crosshair — hides the default cursor and draws
/// a custom one via OnGUI with a pulse animation on fire.
/// </summary>
public class PlayerShooting : MonoBehaviour
{
    [Header("Weapon")]
    [Tooltip("Drag the Revolver component here.")]
    [SerializeField] private MonoBehaviour weaponBehaviour;

    [Header("Crosshair")]
    [SerializeField] private Texture2D crosshairTexture;
    [SerializeField] private float crosshairBaseSize  = 32f;
    [SerializeField] private float crosshairPulseSize = 48f;
    [SerializeField] private float pulseDuration      = 0.1f;

    private IWeapon _weapon;
    private Camera  _cam;
    private float   _currentCrosshairSize;

    // ─── Unity lifecycle ──────────────────────────────────────────────────────

    private void Start()
    {
        _cam    = Camera.main;
        _weapon = weaponBehaviour as IWeapon;
        _currentCrosshairSize = crosshairBaseSize;

        Cursor.visible = false;

        if (_weapon == null)
            Debug.LogError("[PlayerShooting] Assigned weaponBehaviour does not implement IWeapon.");
    }

    private void Update()
    {
        if (_weapon == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 shootDir = GetShootDirection();
            _weapon.Shoot(shootDir);
            StartCoroutine(PulseCrosshair());
        }
    }

    // ─── Crosshair ────────────────────────────────────────────────────────────

    private IEnumerator PulseCrosshair()
    {
        // Expand
        float elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            _currentCrosshairSize = Mathf.Lerp(crosshairBaseSize, crosshairPulseSize, elapsed / pulseDuration);
            yield return null;
        }

        // Shrink back
        elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            _currentCrosshairSize = Mathf.Lerp(crosshairPulseSize, crosshairBaseSize, elapsed / pulseDuration);
            yield return null;
        }

        _currentCrosshairSize = crosshairBaseSize;
    }

    private void OnGUI()
    {
        if (crosshairTexture == null) return;

        Vector2 mousePos = Event.current.mousePosition;
        float   half     = _currentCrosshairSize / 2f;

        GUI.DrawTexture(
            new Rect(mousePos.x - half, mousePos.y - half, _currentCrosshairSize, _currentCrosshairSize),
            crosshairTexture
        );
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private Vector2 GetShootDirection()
    {
        Vector3 mouseScreen  = Input.mousePosition;
        mouseScreen.z        = Mathf.Abs(_cam.transform.position.z);
        Vector2 mouseWorld   = _cam.ScreenToWorldPoint(mouseScreen);
        Vector2 playerPos    = transform.position;
        return (mouseWorld - playerPos).normalized;
    }
}