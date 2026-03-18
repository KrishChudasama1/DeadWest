using UnityEngine;

/// <summary>
/// Bullet.cs
///
/// Travels in a straight line until it hits a collider or exceeds maxRange.
/// Receives damage from the weapon that spawned it — ready to apply to
/// enemies once a Health component exists.
///
/// Prefab requirements:
///   - Rigidbody2D   (Gravity Scale = 0, Collision Detection = Continuous)
///   - CircleCollider2D  (Is Trigger = true)
///   - This script
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    // ─── Inspector ────────────────────────────────────────────────────────────

    [SerializeField] private float speed    = 18f;
    [SerializeField] private float maxRange = 15f;

    // ─── Private state ────────────────────────────────────────────────────────

    private Vector2    _direction;
    private Vector2    _spawnPosition;
    private Rigidbody2D _rb;
    private int        _damage;
    private bool       _initialized;

    // ─── Init (called by Revolver after spawn) ────────────────────────────────

    public void Init(Vector2 direction, int damage)
    {
        _direction     = direction.normalized;
        _spawnPosition = transform.position;
        _damage        = damage;
        _initialized   = true;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    // ─── Unity lifecycle ──────────────────────────────────────────────────────

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
        if (other.CompareTag("Player")) return;

        // TODO: other.GetComponent<Health>()?.TakeDamage(_damage);

        Destroy(gameObject);
    }
}