using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed    = 18f;
    [SerializeField] private float maxRange = 15f;

    private Vector2    _direction;
    private Vector2    _spawnPosition;
    private Rigidbody2D _rb;
    private int        _damage;
    private bool       _initialized;
    private bool       _hitPlayer;
    private bool       _hitEnemies;
    private Transform  _owner;

    public void Init(Vector2 direction, int damage, bool hitPlayer = false, bool hitEnemies = true, Transform owner = null)
    {
        _direction     = direction.normalized;
        _spawnPosition = transform.position;
        _damage        = damage;
        _hitPlayer     = hitPlayer;
        _hitEnemies    = hitEnemies;
        _owner         = owner;
        _initialized   = true;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void Awake()
    {
        _rb              = GetComponent<Rigidbody2D>();
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
        if (_owner != null && (other.transform == _owner || other.transform.IsChildOf(_owner)))
            return;

        if (_hitPlayer)
        {
            PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(_damage);
                Destroy(gameObject);
                return;
            }
        }

        if (!_hitEnemies)
        {
            Destroy(gameObject);
            return;
        }

        GhostEnemy ghost = other.GetComponent<GhostEnemy>();
        if (ghost != null) { ghost.TakeDamage(_damage); Destroy(gameObject); return; }

        CursedBrawler brawler = other.GetComponent<CursedBrawler>();
        if (brawler != null) { brawler.TakeDamage(_damage); Destroy(gameObject); return; }

        // ← new
        BreakableObject breakable = other.GetComponent<BreakableObject>();
        if (breakable != null) { breakable.TakeDamage(_damage); Destroy(gameObject); return; }

        Destroy(gameObject);
    }
}
