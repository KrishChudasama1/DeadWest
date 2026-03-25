using UnityEngine;
using System.Collections;

/// <summary>
/// Revolver.cs
///
/// Manages all revolver behaviour: drawing, shooting, reloading, holstering,
/// directional animation, and sprite flipping.
///
/// Animation flow:
///   First shot       → Draw → DrawStill → Shoot → DrawStill (loops)
///   Subsequent shots → Shoot → DrawStill (loops)
///   Reload           → ReloadRoutine → DrawStill (cancelled if player moves)
///   Move while drawn → Holster immediately → back to idle/walk
///   Move mid-reload  → Cancel reload, reset ammo bar, holster
///   Inactivity       → Holster after holsterDelay → back to idle/walk
///
/// Direction system:
///   Only Left/Up/Down clips exist. Right reuses Left with flipX.
///   GunX/GunY drive the blend trees independently of movement params.
///   Direction updates every shot — no re-draw needed when changing direction.
///
/// Bullet spawn:
///   Spawn position is offset from the player center based on shoot direction
///   so bullets always appear in front of the character regardless of facing.
/// </summary>
public class Revolver : MonoBehaviour, IWeapon
{
    // ─── IWeapon ──────────────────────────────────────────────────────────────

    public string WeaponName   => _data != null ? _data.weaponName : "None";
    public bool   IsOnCooldown => _isOnCooldown || _isReloading || _isDrawing;

    // ─── Inspector ────────────────────────────────────────────────────────────

    [Header("References")]
    [SerializeField] private Animator playerAnimator;

    [Header("Starting Gun")]
    [SerializeField] private RevolverData startingData;

    [Header("Draw Settings")]
    [Tooltip("Match this to the length of your Draw animation clip in seconds.")]
    [SerializeField] private float drawDuration = 0.3f;

    [Tooltip("Seconds of inactivity before the gun auto-holsters.")]
    [SerializeField] private float holsterDelay = 0.8f;

    [Header("Bullet Spawn")]
    [Tooltip("How far from the player center the bullet spawns in the shoot direction.")]
    [SerializeField] private float muzzleDistance = 0.4f;

    [Tooltip("Fixed offset applied on top of the directional offset. " +
             "Nudge to align spawn point with the gun barrel visually.")]
    [SerializeField] private Vector2 muzzleBaseOffset = new Vector2(0f, 0.1f);

    // ─── Public state (read by AmmoHUD) ───────────────────────────────────────

    public int          CurrentAmmo    => _currentAmmo;
    public int          ChamberSize    => _data != null ? _data.chamberSize : 0;
    public bool         IsReloading    => _isReloading;
    public float        ReloadProgress => _reloadProgress;
    public RevolverData CurrentData    => _data;
    public GameObject   CurrentBulletPrefab => _runtimeBulletPrefab;

    // ─── Private state ────────────────────────────────────────────────────────

    private RevolverData   _data;
    private PlayerMovement _playerMovement;
    private int            _currentAmmo;
    private bool           _isOnCooldown;
    private bool           _isReloading;
    private float          _reloadProgress;
    private bool           _isDrawn;
    private bool           _isDrawing;
    private float          _lastShotTime;
    private GameObject     _runtimeBulletPrefab;

    // ─── Animator parameter hashes ────────────────────────────────────────────

    private static readonly int HashDraw       = Animator.StringToHash("Draw");
    private static readonly int HashDrawStill  = Animator.StringToHash("DrawStill");
    private static readonly int HashHolster    = Animator.StringToHash("Holster");
    private static readonly int HashShootUp    = Animator.StringToHash("ShootUp");
    private static readonly int HashShootDown  = Animator.StringToHash("ShootDown");
    private static readonly int HashShootLeft  = Animator.StringToHash("ShootLeft");
    private static readonly int HashShootRight = Animator.StringToHash("ShootRight");

    // ─── Unity lifecycle ──────────────────────────────────────────────────────

    private void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _playerMovement?.SetMovementStartedCallback(OnPlayerStartedMoving);

        if (startingData != null)
            Equip(startingData);
        else
            Debug.LogWarning("[Revolver] No starting data assigned.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !_isReloading && _currentAmmo < ChamberSize)
            StartCoroutine(ReloadRoutine());

        if (_isDrawn && !_isDrawing && Time.time - _lastShotTime >= holsterDelay)
            Holster();
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Equip a new gun at runtime. Resets all state immediately.
    /// e.g. revolver.Equip(magnumData);
    /// </summary>
    public void Equip(RevolverData data)
    {
        StopAllCoroutines();
        ClearReloadState();

        _data         = data;
        _currentAmmo  = data.chamberSize;
        _isOnCooldown = false;
        _isDrawing    = false;
        _isDrawn      = false;
        _runtimeBulletPrefab = data.bulletPrefab;

        _playerMovement?.SetGunDrawn(false);
        _playerMovement?.SetMovementLocked(false);

        Debug.Log($"[Revolver] Equipped: {data.weaponName}");
    }

    /// <summary>
    /// Swaps the bullet prefab used by this revolver instance at runtime.
    /// Does not modify the underlying RevolverData asset.
    /// </summary>
    public void SetBulletPrefab(GameObject bulletPrefab)
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("[Revolver] Ignored null bullet prefab.");
            return;
        }

        _runtimeBulletPrefab = bulletPrefab;
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

        ApplyDirection(shootDirection);

        if (!_isDrawn)
            StartCoroutine(DrawThenShoot(shootDirection));
        else
            StartCoroutine(ShootAndUnlock(shootDirection));
    }

    // ─── Coroutines ───────────────────────────────────────────────────────────

    /// First shot — plays Draw animation then fires.
    private IEnumerator DrawThenShoot(Vector2 shootDirection)
    {
        _isDrawing    = true;
        _isOnCooldown = true;
        _playerMovement?.SetMovementLocked(true);

        SetAnimTrigger(HashDraw);

        yield return new WaitForSeconds(drawDuration);

        _isDrawing = false;
        _isDrawn   = true;
        _playerMovement?.SetGunDrawn(true);

        SetAnimTrigger(HashDrawStill);
        FireShot(shootDirection);

        yield return new WaitForSeconds(_data.fireRate);

        _isOnCooldown = false;
        _playerMovement?.SetMovementLocked(false);
    }

    /// Subsequent shots — fires immediately, no draw.
    private IEnumerator ShootAndUnlock(Vector2 shootDirection)
    {
        _isOnCooldown = true;
        _playerMovement?.SetMovementLocked(true);

        FireShot(shootDirection);

        yield return new WaitForSeconds(_data.fireRate);

        _isOnCooldown = false;
        _playerMovement?.SetMovementLocked(false);
    }

    /// Reload — locks movement for the full reload duration.
    /// Cancelled immediately if the player moves (OnPlayerStartedMoving).
    private IEnumerator ReloadRoutine()
    {
        _isReloading    = true;
        _reloadProgress = 0f;
        _playerMovement?.SetMovementLocked(true);

        float elapsed = 0f;
        while (elapsed < _data.reloadTime)
        {
            elapsed         += Time.deltaTime;
            _reloadProgress  = Mathf.Clamp01(elapsed / _data.reloadTime);
            yield return null;
        }

        // Reload completed fully
        _currentAmmo  = _data.chamberSize;
        _isDrawn      = true;
        _lastShotTime = Time.time;

        _playerMovement?.SetGunDrawn(true);
        _playerMovement?.SetMovementLocked(false);

        ClearReloadState();

        // One frame pause so the Animator settles before pushing DrawStill
        yield return null;
        SetAnimTrigger(HashDrawStill);
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    /// Fires one shot — decrements ammo, triggers animation, spawns bullet.
    private void FireShot(Vector2 shootDirection)
    {
        _currentAmmo--;
        _lastShotTime = Time.time;

        SetAnimTrigger(GetShootTrigger(shootDirection));
        SpawnBullet(shootDirection);
    }

    /// Holsters the gun and returns Animator to idle/walk immediately.
    /// Always safe to call — clears all coroutines and resets all state.
    private void Holster()
    {
        StopAllCoroutines();
        ClearReloadState();

        _isDrawn      = false;
        _isDrawing    = false;
        _isOnCooldown = false;

        _playerMovement?.SetGunDrawn(false);
        _playerMovement?.SetMovementLocked(false);

        SetAnimTrigger(HashHolster);
    }

    /// Called by PlayerMovement the moment WASD is pressed while gun is drawn.
    /// Cancels reload (resetting progress) and holsters immediately.
    private void OnPlayerStartedMoving()
    {
        // Cancel regardless of whether reloading or just drawn
        // Reload progress is discarded — player must reload again from scratch
        if (_isDrawn || _isReloading)
            Holster();
    }

    /// Forces reload state to a clean resting value.
    /// Called on holster, equip, and reload completion so the
    /// reload bar never gets stuck regardless of how the state ended.
    private void ClearReloadState()
    {
        _isReloading    = false;
        _reloadProgress = 0f;
    }

    /// Resets ALL pending Animator triggers before setting a new one.
    /// Prevents stale queued triggers from firing at the wrong time.
    private void SetAnimTrigger(int hash)
    {
        if (playerAnimator == null) return;

        playerAnimator.ResetTrigger(HashDraw);
        playerAnimator.ResetTrigger(HashDrawStill);
        playerAnimator.ResetTrigger(HashHolster);
        playerAnimator.ResetTrigger(HashShootUp);
        playerAnimator.ResetTrigger(HashShootDown);
        playerAnimator.ResetTrigger(HashShootLeft);
        playerAnimator.ResetTrigger(HashShootRight);

        playerAnimator.SetTrigger(hash);
    }

    /// Snaps direction to a cardinal, sets sprite flip, and updates
    /// GunX/GunY so blend trees always show the correct directional sprite.
    /// Right reuses the Left clip with flipX — no separate Right clip needed.
    private void ApplyDirection(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle >= -45f && angle < 45f)
        {
            _playerMovement?.SetFacingDirection(1f);
            _playerMovement?.SetGunDirection(new Vector2(-1f, 0f));
        }
        else if (angle >= 45f && angle < 135f)
        {
            _playerMovement?.SetFacingDirection(-1f);
            _playerMovement?.SetGunDirection(new Vector2(0f, 1f));
        }
        else if (angle >= -135f && angle < -45f)
        {
            _playerMovement?.SetFacingDirection(-1f);
            _playerMovement?.SetGunDirection(new Vector2(0f, -1f));
        }
        else
        {
            _playerMovement?.SetFacingDirection(-1f);
            _playerMovement?.SetGunDirection(new Vector2(-1f, 0f));
        }
    }

    /// Maps a shoot direction to the correct Animator trigger hash.
    private int GetShootTrigger(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle >= -45f  && angle < 45f)  return HashShootRight;
        if (angle >= 45f   && angle < 135f) return HashShootUp;
        if (angle >= -135f && angle < -45f) return HashShootDown;
        return HashShootLeft;
    }

    /// Calculates bullet spawn position offset from player center
    /// in the shoot direction so bullets always emerge from the muzzle.
    private Vector2 GetMuzzlePosition(Vector2 direction)
    {
        Vector2 center = (Vector2)transform.position + muzzleBaseOffset;
        return center + direction.normalized * muzzleDistance;
    }

    /// Instantiates a bullet at the directional muzzle position.
    private void SpawnBullet(Vector2 direction)
    {
        if (_runtimeBulletPrefab == null)
        {
            Debug.LogWarning("[Revolver] No bullet prefab assigned in RevolverData.");
            return;
        }

        Vector2    spawnPos = GetMuzzlePosition(direction);
        GameObject bulletGO = Instantiate(_runtimeBulletPrefab, spawnPos, Quaternion.identity);
        Bullet     bullet   = bulletGO.GetComponent<Bullet>();

        if (bullet != null)
            bullet.Init(direction, _data.damage);
        else
            Debug.LogWarning("[Revolver] Bullet prefab is missing a Bullet component.");
    }
}