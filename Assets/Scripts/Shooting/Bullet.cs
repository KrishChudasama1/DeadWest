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

    public void Init(Vector2 direction, int damage)
    {
        _direction     = direction.normalized;
        _spawnPosition = transform.position;
        _damage        = damage;
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
        if (other.CompareTag("Player")) return;

        GhostEnemy ghost = other.GetComponent<GhostEnemy>();
        if (ghost != null)
        {
            ghost.TakeDamage(_damage);
            Destroy(gameObject);
            return;
        }

        CursedBrawler brawler = other.GetComponent<CursedBrawler>();
        if (brawler != null)
        {
            brawler.TakeDamage(_damage);
            Destroy(gameObject);
            return;
        }

        RanchHandEnemy ranchHand = other.GetComponent<RanchHandEnemy>();
        if (ranchHand != null)
        {
            ranchHand.TakeDamage(_damage);
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}
