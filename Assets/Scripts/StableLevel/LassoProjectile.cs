using UnityEngine;

namespace StableLevel
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class LassoProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 12f;
        [SerializeField] private float maxRange = 10f;

        private Vector2 _direction;
        private Vector2 _spawnPosition;
        private Rigidbody2D _rb;
        private bool _initialized;

       
        public void Init(Vector2 direction)
        {
            _direction = direction.normalized;
            _spawnPosition = transform.position;
            _initialized = true;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
        }

        private void FixedUpdate()
        {
            if (!_initialized) return;

            _rb.MovePosition(_rb.position + _direction * speed * Time.fixedDeltaTime);

            if (Vector2.Distance(_spawnPosition, _rb.position) >= maxRange)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Only interact with PhantomRider, pass through everything else
            PhantomRider rider = other.GetComponent<PhantomRider>();
            if (rider != null)
            {
                rider.TakeLassoHit();
                Destroy(gameObject);
            }
            // If not PhantomRider: do nothing, lasso passes through
        }
    }
}
