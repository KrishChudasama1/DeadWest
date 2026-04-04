using System.Collections;
using UnityEngine;

public class GhostEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.5f;
    public float attackRange = 0.8f;
    public float chaseRange = 8f;

    [Header("Attack")]
    public int attackDamage = 10;
    public float attackCooldown = 1.5f;

    [Header("Health")]
    public int maxHealth = 2;
    private int currentHealth;

    [Header("XP")]
    public int xpOnDeath = 2;
    public GameObject xpPickupPrefab;

    private Transform player;
    private Animator animator;
    private SpriteRenderer sr;
    private bool isAttacking = false;
    private bool canAttack = true;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            // Stop moving and attack
            if (canAttack && !isAttacking)
                StartCoroutine(Attack());
        }
        else if (distanceToPlayer <= chaseRange && !isAttacking)
        {
            ChasePlayer();
        }
    }

    void ChasePlayer()
    {
        animator.SetBool("IsAttacking", false);

        Vector2 direction = (player.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            moveSpeed * Time.deltaTime
        );

        if (direction.x != 0)
            sr.flipX = direction.x < 0;
    }

    IEnumerator Attack()
    {
        isAttacking = true;
        canAttack = false;

        animator.SetBool("IsAttacking", true);

        // Wait for wind up
        yield return new WaitForSeconds(0.3f);

        // Deal damage if still in range
        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log("Ghost dealt " + attackDamage + " damage!");
            }
            else
            {
                Debug.LogWarning("PlayerHealth component not found on player!");
            }
        }

        yield return new WaitForSeconds(0.5f);
        animator.SetBool("IsAttacking", false);

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        canAttack = true;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log("Ghost took damage! Health remaining: " + currentHealth);
        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator FlashRed()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        sr.color = Color.white;
    }

    void Die()
    {
        DropOrGrantXP();
        Destroy(gameObject);
    }

    void DropOrGrantXP()
    {
        if (xpOnDeath <= 0)
            return;

        // If a pickup prefab is assigned, spawn it and set the XP value on it.
        if (xpPickupPrefab != null)
        {
            GameObject drop = Instantiate(xpPickupPrefab, transform.position, Quaternion.identity);
            XPPickup pickup = drop.GetComponent<XPPickup>();
            if (pickup != null)
            {
                pickup.xpValue = xpOnDeath;
            }
            else
            {
                Debug.LogWarning("xpPickupPrefab is missing an XPPickup component.");
            }
            return;
        }

        // Fallback: grant XP directly if no pickup prefab is configured.
        XPManager xpManager = null;
        if (player != null)
            xpManager = player.GetComponent<XPManager>();

        if (xpManager == null)
            xpManager = FindObjectOfType<XPManager>();

        if (xpManager != null)
            xpManager.GainExperience(xpOnDeath);
        else
            Debug.LogWarning("No XPManager found. Ghost XP could not be awarded.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}