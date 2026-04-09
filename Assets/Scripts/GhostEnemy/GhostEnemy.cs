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

    [Header("Drops")]
    public GameObject coinPrefab;
    public int coinDropAmount = 1;
    public float coinDropSpread = 0.25f;

    private Transform player;
    private Animator animator;
    private SpriteRenderer sr;
    private bool isAttacking = false;
    private bool canAttack = true;

    void Start()
    {
        // Let all ghosts on the same layer pass through each other
        Physics2D.IgnoreLayerCollision(gameObject.layer, gameObject.layer, true);

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

        yield return new WaitForSeconds(0.3f);

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
        DropCoins();
        DropOrGrantXP();
        Destroy(gameObject);
    }

    void DropCoins()
    {
        if (coinPrefab == null || coinDropAmount <= 0)
            return;

        for (int i = 0; i < coinDropAmount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * coinDropSpread;
            Instantiate(coinPrefab, (Vector2)transform.position + offset, Quaternion.identity);
        }
    }

    void DropOrGrantXP()
    {
        if (xpOnDeath <= 0)
            return;

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

    XPManager.AddExperience(xpOnDeath);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}
