using UnityEngine;

/// <summary>
/// Projectile launched by the lasso throw system.
/// Moves in a straight line and detects hits on the PhantomRider.
/// Must be on the "Lasso" layer which only collides with the "Enemy" layer.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class LassoProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 14f;
    [SerializeField] private float maxRange = 12f;

    private Vector2 _direction;
    private Vector2 _spawnPosition;
    private Rigidbody2D _rb;
    private bool _initialized;

    /// <summary>
    /// Event fired when the lasso hits a valid target collider.
    /// </summary>
    public event System.Action<Collider2D> OnLassoHit;

    /// <summary>
    /// Initializes the lasso projectile with a direction of travel.
    /// </summary>
    /// <param name="direction">Normalized direction vector.</param>
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
        _rb.bodyType = RigidbodyType2D.Kinematic;

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
    }

    private void FixedUpdate()
    {
        if (!_initialized) return;

        _rb.MovePosition(_rb.position + _direction * speed * Time.fixedDeltaTime);

        if (Vector2.Distance(_spawnPosition, _rb.position) >= maxRange)
            Destroy(gameObject);
    }

    /// <summary>
    /// Detects collision with the PhantomRider or other enemy-layer objects.
    /// </summary>
    /// <param name="other">The collider that was hit.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;

        OnLassoHit?.Invoke(other);

        // Check for PhantomRider component
        PhantomRider rider = other.GetComponent<PhantomRider>();
        if (rider != null)
        {
            rider.RegisterLassoHit();
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}
