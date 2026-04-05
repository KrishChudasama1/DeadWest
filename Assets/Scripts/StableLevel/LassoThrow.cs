using UnityEngine;

namespace StableLevel
{
    public class LassoThrow : MonoBehaviour
    {
        [Header("Lasso")]
        public GameObject lassoProjectilePrefab;

        [SerializeField] private float throwCooldown = 0.8f;
        [SerializeField] private float muzzleDistance = 0.5f;
        [SerializeField] private KeyCode throwKey = KeyCode.L;

        private float _lastThrowTime = -999f;
        private bool _hasLasso = false;
        private bool _isEquipped = false;
        private Camera _cam;

        /// <summary>True once the player has picked up the lasso.</summary>
        public bool HasLasso => _hasLasso;

        /// <summary>True when the lasso is the active weapon (equipped via inventory).</summary>
        public bool IsEquipped => _isEquipped;

        public static LassoThrow Instance { get; private set; }

        // ────────────────────────────────────────────────────────
        // Bootstrap: auto-attach to the Player if not already there
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Call this once (e.g. from a scene manager) to ensure LassoThrow
        /// exists on the Player. Safe to call multiple times.
        /// </summary>
        public static LassoThrow EnsureOnPlayer()
        {
            if (Instance != null) return Instance;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("[LassoThrow] No GameObject tagged 'Player' found.");
                return null;
            }

            LassoThrow lt = player.GetComponent<LassoThrow>();
            if (lt == null)
                lt = player.AddComponent<LassoThrow>();

            return lt;
        }

        // ────────────────────────────────────────────────────────

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _cam = Camera.main;
            LassoPickup.OnLassoPickedUp += OnLassoAcquired;
        }

        private void OnDestroy()
        {
            LassoPickup.OnLassoPickedUp -= OnLassoAcquired;
            if (Instance == this) Instance = null;
        }

        private void OnLassoAcquired()
        {
            _hasLasso = true;
            Debug.Log("LassoThrow: lasso acquired — equip it from inventory, then press L to throw.");
        }

        public void Equip()
        {
            _hasLasso = true;   // force — the inventory already confirmed pickup
            _isEquipped = true;
            Debug.Log($"LassoThrow: lasso equipped. hasLasso={_hasLasso}, prefab={(lassoProjectilePrefab != null ? lassoProjectilePrefab.name : "NULL")}");
        }

        public void Unequip()
        {
            _isEquipped = false;
            Debug.Log("LassoThrow: lasso unequipped.");
        }

        private void Update()
        {
            if (!_hasLasso) return;
            if (!_isEquipped) return;
            if (lassoProjectilePrefab == null)
            {
                Debug.LogWarning("LassoThrow: lassoProjectilePrefab is NULL — cannot throw.");
                return;
            }

            if (Input.GetKeyDown(throwKey))
            {
                Debug.Log("LassoThrow: L key pressed — attempting throw...");
                if (Time.time - _lastThrowTime >= throwCooldown)
                {
                    ThrowLasso();
                }
                else
                {
                    Debug.Log("LassoThrow: on cooldown.");
                }
            }
        }

        private void ThrowLasso()
        {
            if (_cam == null) _cam = Camera.main;

            // ScreenToWorldPoint needs z = distance from camera to the game plane
            Vector3 mouseScreenPos = Input.mousePosition;
            mouseScreenPos.z = Mathf.Abs(_cam.transform.position.z);
            Vector3 mouseWorldPos = _cam.ScreenToWorldPoint(mouseScreenPos);
            mouseWorldPos.z = 0f;

            Vector2 playerPos = (Vector2)transform.position;
            Vector2 target = (Vector2)mouseWorldPos;
            Vector2 diff = target - playerPos;

            if (diff.sqrMagnitude < 0.01f)
                diff = Vector2.right;

            Vector2 direction = diff.normalized;
            float distance = diff.magnitude;

            Vector2 spawnPos = playerPos + direction * muzzleDistance;

            GameObject proj = Instantiate(lassoProjectilePrefab, spawnPos, Quaternion.identity);
            LassoProjectile lasso = proj.GetComponent<LassoProjectile>();
            if (lasso != null)
                lasso.Init(direction, transform, distance);

            _lastThrowTime = Time.time;
            Debug.Log($"LassoThrow: threw lasso toward cursor, distance={distance:F1}");
        }
    }
}
