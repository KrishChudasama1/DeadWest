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
    private bool _ignoreWalls = true;
    private float _ignoreWallsTimer = 0.1f;

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

        // Stop ignoring walls after the timer expires
        if (_ignoreWalls)
        {
            _ignoreWallsTimer -= Time.fixedDeltaTime;
            if (_ignoreWallsTimer <= 0f)
                _ignoreWalls = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_owner != null && (other.transform == _owner || other.transform.IsChildOf(_owner)))
            return;

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (!_hitPlayer && playerHealth != null)
            return;

        if (_hitPlayer)
        {
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
        
        if (other.CompareTag("Player")) return;

        // Ignore walls briefly after spawn so corner bullets don't die instantly
        if (_ignoreWalls && other.CompareTag("Wall")) return;

        GhostEnemy ghost = other.GetComponentInParent<GhostEnemy>();
        if (ghost != null) { ghost.TakeDamage(_damage); Destroy(gameObject); return; }

        CursedBrawler brawler = other.GetComponent<CursedBrawler>();
        if (brawler != null) { brawler.TakeDamage(_damage); Destroy(gameObject); return; }

        CorruptedPriest priest = other.GetComponent<CorruptedPriest>();
        if (priest != null) { priest.TakeDamage(_damage); Destroy(gameObject); return; }

        RanchHandEnemy ranchHand = other.GetComponent<RanchHandEnemy>();
        if (ranchHand != null) { ranchHand.TakeDamage(_damage); Destroy(gameObject); return; }

        EnemyChase sheriff = other.GetComponentInParent<EnemyChase>();
        if (sheriff != null) { sheriff.TakeDamage(_damage); Destroy(gameObject); return; }

        BreakableObject breakable = other.GetComponentInParent<BreakableObject>();
        if (breakable != null) { breakable.TakeDamage(_damage); Destroy(gameObject); return; }

        RestlessUndead undead = other.GetComponent<RestlessUndead>();
        if (undead != null) { undead.TakeDamage(_damage); Destroy(gameObject); return; }

        Destroy(gameObject);
    }
}

