using UnityEngine;
using System.Collections;

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

    public static bool IsInteracting = false;

    private IWeapon _weapon;
    private Camera  _cam;
    private float   _currentCrosshairSize;
    private bool    _isStunned = false;

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

        Cursor.visible = InventoryManager.IsInventoryOpen || IsInteracting;

        if (InventoryManager.IsInventoryOpen) return;
        if (_isStunned) return;
        if (IsInteracting) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 shootDir = GetShootDirection();
            _weapon.Shoot(shootDir);
            StartCoroutine(PulseCrosshair());
        }
    }

    public void Stun(float duration)
    {
        StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        _isStunned = true;
        yield return new WaitForSeconds(duration);
        _isStunned = false;
    }

    private IEnumerator PulseCrosshair()
    {
        float elapsed = 0f;
        while (elapsed < pulseDuration)
        {
            elapsed += Time.deltaTime;
            _currentCrosshairSize = Mathf.Lerp(crosshairBaseSize, crosshairPulseSize, elapsed / pulseDuration);
            yield return null;
        }

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
        if (InventoryManager.IsInventoryOpen) return;
        if (IsInteracting) return;

        Vector2 mousePos = Event.current.mousePosition;
        float   half     = _currentCrosshairSize / 2f;

        GUI.DrawTexture(
            new Rect(mousePos.x - half, mousePos.y - half, _currentCrosshairSize, _currentCrosshairSize),
            crosshairTexture
        );
    }

    private Vector2 GetShootDirection()
    {
        // Ensure the cached camera is valid (it can be destroyed during scene loads)
        if (_cam == null)
        {
            _cam = Camera.main;
        }

        if (_cam == null)
        {
            // No camera available (editor or scene transition); return a default direction to avoid exceptions
            Debug.LogWarning("PlayerShooting: no Camera available when calculating shoot direction.");
            return Vector2.right;
        }

        Vector3 mouseScreen  = Input.mousePosition;
        // Use camera's Z so ScreenToWorldPoint maps correctly
        mouseScreen.z        = Mathf.Abs(_cam.transform.position.z);
        Vector2 mouseWorld   = _cam.ScreenToWorldPoint(mouseScreen);
        Vector2 playerPos    = transform.position;
        return (mouseWorld - playerPos).normalized;
    }
}
