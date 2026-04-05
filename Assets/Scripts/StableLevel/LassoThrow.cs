using UnityEngine;

namespace StableLevel
{
    [RequireComponent(typeof(PlayerMovement))]
    public class LassoThrow : MonoBehaviour
    {
        [Header("Lasso")]
        public GameObject lassoProjectilePrefab;

        [SerializeField] private float throwCooldown = 0.8f;
        [SerializeField] private float muzzleDistance = 0.5f;

        private float _lastThrowTime = -999f;
        private bool _hasLasso = false;
        private Camera _cam;

        private void Start()
        {
            _cam = Camera.main;
            LassoPickup.OnLassoPickedUp += OnLassoAcquired;
        }

        private void OnDestroy()
        {
            LassoPickup.OnLassoPickedUp -= OnLassoAcquired;
        }

        private void OnLassoAcquired()
        {
            _hasLasso = true;
            Debug.Log("LassoThrow: lasso acquired — right-click to throw.");
        }

        private void Update()
        {
            if (!_hasLasso) return;
            if (lassoProjectilePrefab == null) return;

            if (Input.GetMouseButtonDown(1)) // right mouse button
            {
                if (Time.time - _lastThrowTime >= throwCooldown)
                {
                    ThrowLasso();
                }
            }
        }

        private void ThrowLasso()
        {
            Vector3 mouseWorldPos = _cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;

            Vector2 direction = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;
            Vector2 spawnPos = (Vector2)transform.position + direction * muzzleDistance;

            GameObject proj = Instantiate(lassoProjectilePrefab, spawnPos, Quaternion.identity);
            LassoProjectile lasso = proj.GetComponent<LassoProjectile>();
            if (lasso != null)
                lasso.Init(direction);

            _lastThrowTime = Time.time;
        }
    }
}
