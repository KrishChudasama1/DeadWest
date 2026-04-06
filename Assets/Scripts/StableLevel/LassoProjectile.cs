using UnityEngine;

namespace StableLevel
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(LineRenderer))]
    public class LassoProjectile : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float speed = 12f;
        [SerializeField] private float maxRangeClamp = 20f;   // absolute maximum distance cap
        [SerializeField] private float lingerTime = 0.15f;    // seconds the lasso stays at the target before destroying

        [Header("Rope Visuals")]
        [SerializeField] private float ropeWidth = 0.06f;
        [SerializeField] private Color ropeColor = new Color(0.72f, 0.53f, 0.26f); // tan / rope brown

        private Vector2 _direction;
        private Vector2 _spawnPosition;
        private Rigidbody2D _rb;
        private LineRenderer _lr;
        private Transform _origin;   // player (or muzzle) that the rope starts from
        private bool _initialized;
        private float _targetRange;   // dynamic range based on cursor distance
        private bool _arrived;        // true once the lasso reaches the target distance

        /// <summary>
        /// Initialise with a dynamic range (distance to cursor).
        /// </summary>
        public void Init(Vector2 direction, Transform origin, float range)
        {
            _direction = direction.normalized;
            _spawnPosition = transform.position;
            _origin = origin;
            _targetRange = Mathf.Min(range, maxRangeClamp);
            _initialized = true;
        }

        

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;

            // Set up the LineRenderer as a simple rope line
            _lr = GetComponent<LineRenderer>();
            _lr.positionCount = 2;
            _lr.startWidth = ropeWidth;
            _lr.endWidth = ropeWidth;
            _lr.useWorldSpace = true;
            _lr.sortingLayerName = "player";  // match your player sorting layer
            _lr.sortingOrder = 10;             // draw on top

            // Try URP unlit first, fall back to Sprites/Default
            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");
            if (shader != null)
                _lr.material = new Material(shader);

            _lr.startColor = ropeColor;
            _lr.endColor = ropeColor;

            Debug.Log($"LassoProjectile: spawned. Shader={(shader != null ? shader.name : "NULL")}");
        }

        private void FixedUpdate()
        {
            if (!_initialized) return;

            if (!_arrived)
            {
                _rb.MovePosition(_rb.position + _direction * speed * Time.fixedDeltaTime);

                if (Vector2.Distance(_spawnPosition, _rb.position) >= _targetRange)
                {
                    _arrived = true;
                    _rb.linearVelocity = Vector2.zero;
                    Destroy(gameObject, lingerTime);   // brief pause, then disappear
                }
            }
        }

        private void LateUpdate()
        {
            if (!_initialized || _lr == null) return;

            // Point 0 = player / origin, Point 1 = lasso tip (this object)
            Vector3 start = _origin != null ? _origin.position : (Vector3)_spawnPosition;
            start.z = 0f;

            Vector3 end = transform.position;
            end.z = 0f;

            _lr.SetPosition(0, start);
            _lr.SetPosition(1, end);
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
        }
    }
}
