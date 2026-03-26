using UnityEngine;
using System.Collections;

public class Revolver : MonoBehaviour, IWeapon
{
    public string WeaponName   => _data != null ? _data.weaponName : "None";
    public bool   IsOnCooldown => _isOnCooldown || _isReloading || _isDrawing;

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

    public int          CurrentAmmo    => _currentAmmo;
    public int          ChamberSize    => _data != null ? _data.chamberSize : 0;
    public bool         IsReloading    => _isReloading;
    public float        ReloadProgress => _reloadProgress;
    public RevolverData CurrentData    => _data;
    public GameObject   CurrentBulletPrefab => _runtimeBulletPrefab;

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

    private static readonly int HashDraw       = Animator.StringToHash("Draw");
    private static readonly int HashDrawStill  = Animator.StringToHash("DrawStill");
    private static readonly int HashHolster    = Animator.StringToHash("Holster");
    private static readonly int HashShootUp    = Animator.StringToHash("ShootUp");
    private static readonly int HashShootDown  = Animator.StringToHash("ShootDown");
    private static readonly int HashShootLeft  = Animator.StringToHash("ShootLeft");
    private static readonly int HashShootRight = Animator.StringToHash("ShootRight");

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

    public void SetBulletPrefab(GameObject bulletPrefab)
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("[Revolver] Ignored null bullet prefab.");
            return;
        }

        _runtimeBulletPrefab = bulletPrefab;
    }

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

    private IEnumerator ShootAndUnlock(Vector2 shootDirection)
    {
        _isOnCooldown = true;
        _playerMovement?.SetMovementLocked(true);

        FireShot(shootDirection);

        yield return new WaitForSeconds(_data.fireRate);

        _isOnCooldown = false;
        _playerMovement?.SetMovementLocked(false);
    }

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

        _currentAmmo  = _data.chamberSize;
        _isDrawn      = true;
        _lastShotTime = Time.time;

        _playerMovement?.SetGunDrawn(true);
        _playerMovement?.SetMovementLocked(false);

        ClearReloadState();

        yield return null;
        SetAnimTrigger(HashDrawStill);
    }

    private void FireShot(Vector2 shootDirection)
    {
        _currentAmmo--;
        _lastShotTime = Time.time;

        SetAnimTrigger(GetShootTrigger(shootDirection));
        SpawnBullet(shootDirection);
    }

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

    private void OnPlayerStartedMoving()
    {
        if (_isDrawn || _isReloading)
            Holster();
    }

    private void ClearReloadState()
    {
        _isReloading    = false;
        _reloadProgress = 0f;
    }

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

    private int GetShootTrigger(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle >= -45f  && angle < 45f)  return HashShootRight;
        if (angle >= 45f   && angle < 135f) return HashShootUp;
        if (angle >= -135f && angle < -45f) return HashShootDown;
        return HashShootLeft;
    }

    private Vector2 GetMuzzlePosition(Vector2 direction)
    {
        Vector2 center = (Vector2)transform.position + muzzleBaseOffset;
        return center + direction.normalized * muzzleDistance;
    }

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
