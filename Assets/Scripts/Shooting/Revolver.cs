using UnityEngine;
using System.Collections;

/// <summary>
/// Revolver.cs
///
/// Animation flow:
///
///   First shot:
///     → SetTrigger(Draw)
///     → wait drawDuration
///     → SetTrigger(DrawStill)   [blend tree driven by MoveX/MoveY]
///     → FireShot()
///     → SetTrigger(ShootUp/Down/Left/Right)
///     → Animator auto-returns to DrawStill
///
///   Subsequent shots (gun already drawn):
///     → FireShot() immediately
///     → SetTrigger(ShootUp/Down/Left/Right)
///     → Animator auto-returns to DrawStill
///
///   After holsterDelay seconds of inactivity:
///     → SetTrigger(Holster)
///     → _isDrawn = false
///     → Animator returns to walk/idle blend tree
///
/// The DrawStill state in the Animator is a 2D blend tree using
/// MoveX and MoveY so the correct directional sprite shows while walking.
/// </summary>
public class Revolver : MonoBehaviour, IWeapon
{
    // ─── IWeapon ──────────────────────────────────────────────────────────────

    public string WeaponName   => _data != null ? _data.weaponName : "None";
    public bool   IsOnCooldown => _isOnCooldown || _isReloading || _isDrawing;

    // ─── Inspector ────────────────────────────────────────────────────────────

    [Header("References")]
    [SerializeField] private Animator  playerAnimator;
    [SerializeField] private Transform muzzlePoint;

    [Header("Starting Gun")]
    [SerializeField] private RevolverData startingData;

    [Header("Draw Settings")]
    [Tooltip("Match this to the length of your Draw animation clip in seconds.")]
    [SerializeField] private float drawDuration = 0.3f;

    [Tooltip("Seconds of inactivity before the gun holsters and Draw plays again on next shot.")]
    [SerializeField] private float holsterDelay = 3f;

    // ─── Public state (read by AmmoHUD) ───────────────────────────────────────

    public int          CurrentAmmo    => _currentAmmo;
    public int          ChamberSize    => _data != null ? _data.chamberSize : 0;
    public bool         IsReloading    => _isReloading;
    public float        ReloadProgress => _reloadProgress;
    public RevolverData CurrentData    => _data;

    // ─── Private state ────────────────────────────────────────────────────────

    private RevolverData _data;
    private int          _currentAmmo;
    private bool         _isOnCooldown;
    private bool         _isReloading;
    private float        _reloadProgress;
    private bool         _isDrawn;       // gun is currently out
    private bool         _isDrawing;     // draw animation is in progress
    private float        _lastShotTime;

    // ─── Animator parameter hashes ────────────────────────────────────────────

    // Triggers
    private static readonly int HashDraw       = Animator.StringToHash("Draw");
    private static readonly int HashDrawStill  = Animator.StringToHash("DrawStill");
    private static readonly int HashHolster    = Animator.StringToHash("Holster");
    private static readonly int HashShootUp    = Animator.StringToHash("ShootUp");
    private static readonly int HashShootDown  = Animator.StringToHash("ShootDown");
    private static readonly int HashShootLeft  = Animator.StringToHash("ShootLeft");
    private static readonly int HashShootRight = Animator.StringToHash("ShootRight");

    // Floats — same ones PlayerMovement already sets, used by DrawStill blend tree
    private static readonly int HashMoveX = Animator.StringToHash("MoveX");
    private static readonly int HashMoveY = Animator.StringToHash("MoveY");

    // ─── Unity lifecycle ──────────────────────────────────────────────────────

    private void Start()
    {
        if (startingData != null)
            Equip(startingData);
        else
            Debug.LogWarning("[Revolver] No starting data assigned.");
    }

    private void Update()
    {
        // Reload
        if (Input.GetKeyDown(KeyCode.R) && !_isReloading && _currentAmmo < ChamberSize)
            StartCoroutine(ReloadRoutine());

        // Auto-holster after inactivity
        if (_isDrawn && !_isDrawing && Time.time - _lastShotTime >= holsterDelay)
            Holster();
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    public void Equip(RevolverData data)
    {
        StopAllCoroutines();

        _data           = data;
        _currentAmmo    = data.chamberSize;
        _isOnCooldown   = false;
        _isReloading    = false;
        _isDrawing      = false;
        _isDrawn        = false;
        _reloadProgress = 0f;

        Debug.Log($"[Revolver] Equipped: {data.weaponName}");
    }

    // ─── IWeapon ──────────────────────────────────────────────────────────────

    public void Shoot(Vector2 shootDirection)
    {
        if (IsOnCooldown || _data == null) return;

        if (_currentAmmo <= 0)
        {
            Debug.Log("[Revolver] Chamber empty — press R to reload.");
            return;
        }

        if (!_isDrawn)
            StartCoroutine(DrawThenShoot(shootDirection));
        else
            FireShot(shootDirection);
    }

    // ─── Coroutines ───────────────────────────────────────────────────────────

    private IEnumerator DrawThenShoot(Vector2 shootDirection)
    {
        _isDrawing = true;

        if (playerAnimator != null)
            playerAnimator.SetTrigger(HashDraw);

        yield return new WaitForSeconds(drawDuration);

        _isDrawing = false;
        _isDrawn   = true;

        // Transition into the DrawStill blend tree
        // MoveX/MoveY are already being set by PlayerMovement every frame
        // so the blend tree will automatically show the correct direction
        if (playerAnimator != null)
            playerAnimator.SetTrigger(HashDrawStill);

        FireShot(shootDirection);
    }

    private IEnumerator FireRateCooldown()
    {
        _isOnCooldown = true;
        yield return new WaitForSeconds(_data.fireRate);
        _isOnCooldown = false;
    }

    private IEnumerator ReloadRoutine()
    {
        _isReloading    = true;
        _reloadProgress = 0f;

        float elapsed = 0f;
        while (elapsed < _data.reloadTime)
        {
            elapsed         += Time.deltaTime;
            _reloadProgress  = Mathf.Clamp01(elapsed / _data.reloadTime);
            yield return null;
        }

        _currentAmmo    = _data.chamberSize;
        _reloadProgress = 0f;
        _isReloading    = false;

        // Stay drawn after reload — no need to re-draw
        _isDrawn      = true;
        _lastShotTime = Time.time;

        if (playerAnimator != null)
            playerAnimator.SetTrigger(HashDrawStill);
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    private void FireShot(Vector2 shootDirection)
    {
        _currentAmmo--;
        _lastShotTime = Time.time;

        TriggerShootAnimation(shootDirection);
        SpawnBullet(shootDirection);
        StartCoroutine(FireRateCooldown());
    }

    private void Holster()
    {
        _isDrawn = false;

        if (playerAnimator != null)
            playerAnimator.SetTrigger(HashHolster);
    }

    private void TriggerShootAnimation(Vector2 direction)
    {
        if (playerAnimator == null) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if      (angle >= -45f  && angle <  45f)  playerAnimator.SetTrigger(HashShootRight);
        else if (angle >=  45f  && angle < 135f)  playerAnimator.SetTrigger(HashShootUp);
        else if (angle >= -135f && angle < -45f)  playerAnimator.SetTrigger(HashShootDown);
        else                                       playerAnimator.SetTrigger(HashShootLeft);
    }

    private void SpawnBullet(Vector2 direction)
    {
        if (_data.bulletPrefab == null)
        {
            Debug.LogWarning("[Revolver] No bullet prefab on this RevolverData.");
            return;
        }

        Transform  spawnPoint = muzzlePoint != null ? muzzlePoint : transform;
        GameObject bulletGO   = Instantiate(_data.bulletPrefab, spawnPoint.position, Quaternion.identity);
        Bullet     bullet     = bulletGO.GetComponent<Bullet>();

        if (bullet != null)
            bullet.Init(direction, _data.damage);
        else
            Debug.LogWarning("[Revolver] Bullet prefab missing a Bullet component.");
    }
}