using System.Collections;
using UnityEngine;

/// <summary>
/// A ranch hand enemy for the Stable Level wave fights.
/// Follows the same TakeDamage pattern as GhostEnemy so existing Bullet.cs works via GetComponent.
/// Uses the "Enemy" tag so bullets can detect it.
/// </summary>
public class RanchHandEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public float attackRange = 0.9f;
    public float chaseRange = 10f;

    [Header("Attack")]
    public int attackDamage = 12;
    public float attackCooldown = 1.2f;

    [Header("Health")]
    public int maxHealth = 3;

    [Header("XP")]
    public int xpOnDeath = 3;
    public GameObject xpPickupPrefab;

    /// <summary>
    /// Event fired when this enemy dies. Used by WaveSpawner to track remaining enemies.
    /// </summary>
    public event System.Action OnDeath;

    private int _currentHealth;
    private Transform _player;
    private Animator _animator;
    private SpriteRenderer _sr;
    private bool _isAttacking;
    private bool _canAttack = true;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        _animator = GetComponent<Animator>();
        _sr = GetComponent<SpriteRenderer>();
        _currentHealth = maxHealth;
        gameObject.tag = "Enemy";
    }

    private void Update()
    {
        if (_player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        if (distanceToPlayer <= attackRange)
        {
            if (_canAttack && !_isAttacking)
                StartCoroutine(Attack());
        }
        else if (distanceToPlayer <= chaseRange && !_isAttacking)
        {
            ChasePlayer();
        }
    }

    /// <summary>
    /// Moves toward the player position.
    /// </summary>
    private void ChasePlayer()
    {
        if (_animator != null)
            _animator.SetBool("IsAttacking", false);

        Vector2 direction = (_player.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(
            transform.position,
            _player.position,
            moveSpeed * Time.deltaTime
        );

        if (_sr != null && direction.x != 0)
            _sr.flipX = direction.x < 0;
    }

    private IEnumerator Attack()
    {
        _isAttacking = true;
        _canAttack = false;

        if (_animator != null)
            _animator.SetBool("IsAttacking", true);

        // Wind up
        yield return new WaitForSeconds(0.3f);

        // Deal damage if still in range
        if (_player != null && Vector2.Distance(transform.position, _player.position) <= attackRange)
        {
            PlayerHealth playerHealth = _player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(0.5f);

        if (_animator != null)
            _animator.SetBool("IsAttacking", false);

        yield return new WaitForSeconds(attackCooldown);
        _isAttacking = false;
        _canAttack = true;
    }

    /// <summary>
    /// Applies damage to this enemy. Called by Bullet.cs OnTriggerEnter2D.
    /// Compatible with the existing bullet damage system.
    /// </summary>
    /// <param name="amount">Amount of damage to apply.</param>
    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        Debug.Log($"[RanchHand] Took {amount} damage! Health: {_currentHealth}");
        StartCoroutine(FlashRed());

        if (_currentHealth <= 0)
            Die();
    }

    private IEnumerator FlashRed()
    {
        if (_sr != null)
        {
            _sr.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            _sr.color = Color.white;
        }
    }

    private void Die()
    {
        DropXP();
        OnDeath?.Invoke();
        Destroy(gameObject);
    }

    private void DropXP()
    {
        if (xpOnDeath <= 0) return;

        if (xpPickupPrefab != null)
        {
            GameObject drop = Instantiate(xpPickupPrefab, transform.position, Quaternion.identity);
            XPPickup pickup = drop.GetComponent<XPPickup>();
            if (pickup != null)
                pickup.xpValue = xpOnDeath;
            return;
        }

        XPManager xpManager = FindFirstObjectByType<XPManager>();
        if (xpManager != null)
            xpManager.GainExperience(xpOnDeath);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
