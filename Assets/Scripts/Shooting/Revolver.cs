using UnityEngine;
using System.Collections;

/// <summary>
/// Revolver.cs
///
/// The single weapon component that lives on the Player forever.
/// It reads all its stats from a RevolverData ScriptableObject asset.
///
/// To give the player a new gun at runtime, call:
///     revolver.Equip(newRevolverData);
///
/// That immediately applies the new stats and refills the chamber.
/// </summary>
public class Revolver : MonoBehaviour, IWeapon
{
    // ─── IWeapon ──────────────────────────────────────────────────────────────

    public string WeaponName   => _data != null ? _data.weaponName : "None";
    public bool   IsOnCooldown => _isOnCooldown || _isReloading;

    // ─── Inspector ────────────────────────────────────────────────────────────

    [Header("References")]
    [SerializeField] private Animator  playerAnimator;
    [SerializeField] private Transform muzzlePoint;

    [Header("Starting Gun")]
    [Tooltip("Drag a RevolverData asset here — this is the gun the player starts with.")]
    [SerializeField] private RevolverData startingData;

    // ─── Public state (read by AmmoHUD) ───────────────────────────────────────

    public int   CurrentAmmo    => _currentAmmo;
    public int   ChamberSize    => _data != null ? _data.chamberSize : 0;
    public bool  IsReloading    => _isReloading;
    public float ReloadProgress => _reloadProgress;
    public RevolverData CurrentData => _data;

    // ─── Private state ────────────────────────────────────────────────────────

    private RevolverData _data;
    private int          _currentAmmo;
    private bool         _isOnCooldown;
    private bool         _isReloading;
    private float        _reloadProgress;

    // ─── Animator hashes ──────────────────────────────────────────────────────

    private static readonly int HashShootUp    = Animator.StringToHash("ShootUp");
    private static readonly int HashShootDown  = Animator.StringToHash("ShootDown");
    private static readonly int HashShootLeft  = Animator.StringToHash("ShootLeft");
    private static readonly int HashShootRight = Animator.StringToHash("ShootRight");

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
        if (Input.GetKeyDown(KeyCode.R) && !_isReloading && _currentAmmo < ChamberSize)
            StartCoroutine(ReloadRoutine());
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Call this to give the player a new gun.
    /// Immediately swaps stats and refills the chamber.
    /// e.g.  revolver.Equip(magnumData);
    /// </summary>
    public void Equip(RevolverData data)
    {
        // Stop any in-progress reload from the old gun
        StopAllCoroutines();

        _data           = data;
        _currentAmmo    = data.chamberSize;
        _isOnCooldown   = false;
        _isReloading    = false;
        _reloadProgress = 0f;

        Debug.Log($"[Revolver] Equipped: {data.weaponName}");
    }

    // ─── IWeapon ──────────────────────────────────────────────────────────────

    public void Shoot(Vector2 shootDirection)
    {
        if (_data == null || IsOnCooldown) return;

        if (_currentAmmo <= 0)
        {
            Debug.Log("[Revolver] Chamber empty — press R to reload.");
            return;
        }

        _currentAmmo--;
        TriggerShootAnimation(shootDirection);
        SpawnBullet(shootDirection);
        StartCoroutine(FireRateCooldown());
    }

    // ─── Coroutines ───────────────────────────────────────────────────────────

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
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

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