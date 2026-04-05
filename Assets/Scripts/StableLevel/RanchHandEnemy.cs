using System.Collections;
using UnityEngine;

public class RanchHandEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 1.0f;
    public float attackRange = 1.0f;
    public float chaseRange = 8f;

    [Header("Attack")]
    public int attackDamage = 14;
    public float attackCooldown = 1.2f;
    [Tooltip("Invert the horizontal flip used during the attack animation if the sprite art is facing the opposite direction.")]
    public bool invertAttackFlip = false;

    [Header("Health")]
    public int maxHealth = 6;
    private int currentHealth;

    [Header("XP")]
    public int xpOnDeath = 3;
    public GameObject xpPickupPrefab;

    private Transform player;
    private Animator animator;
    private SpriteRenderer sr;
    private bool isAttacking = false;
    private bool canAttack = true;
    private bool prevSrEnabled = true;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    // Prevent URP / dynamic occlusion from culling this enemy unexpectedly
    #if UNITY_2018_1_OR_NEWER
    sr.allowOcclusionWhenDynamic = false;
    #endif
        currentHealth = maxHealth;
    prevSrEnabled = sr != null ? sr.enabled : true;
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

        if (player != null && sr != null)
        {
            float dirX = player.position.x - transform.position.x;
            bool shouldFlip = dirX > 0f;
            if (invertAttackFlip) shouldFlip = !shouldFlip;
            sr.flipX = shouldFlip;
        }

        animator.SetBool("IsAttacking", true);

        yield return new WaitForSeconds(0.25f);

        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }

        yield return new WaitForSeconds(0.4f);
        animator.SetBool("IsAttacking", false);

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        canAttack = true;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
            Die();
    }

    IEnumerator FlashRed()
    {
        if (sr != null)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.12f);
            sr.color = Color.white;
        }
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

        if (xpPickupPrefab != null)
        {
            GameObject drop = Instantiate(xpPickupPrefab, transform.position, Quaternion.identity);
            XPPickup pickup = drop.GetComponent<XPPickup>();
            if (pickup != null)
            {
                pickup.xpValue = xpOnDeath;
            }
            else
                Debug.LogWarning("xpPickupPrefab is missing an XPPickup component.");

            return;
        }

        XPManager xpManager = null;
        if (player != null)
            xpManager = player.GetComponent<XPManager>();

        if (xpManager == null)
            xpManager = FindObjectOfType<XPManager>();

        if (xpManager != null)
            xpManager.GainExperience(xpOnDeath);
        else
            Debug.LogWarning("No XPManager found. RanchHand XP could not be awarded.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }

    private void LateUpdate()
    {
        // Keep the sprite's sorting order updated so Y-sorting matches the player
        if (sr != null)
        {
            float feetY = 0f;
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
                feetY = col.bounds.min.y;
            else
                feetY = transform.position.y - sr.bounds.extents.y;

            sr.sortingOrder = Mathf.RoundToInt(-feetY * 100);
        }
    }

    // ...existing code...

    private void OnBecameInvisible()
    {
        Debug.Log($"RanchHandEnemy '{name}' OnBecameInvisible at {transform.position}");

        // If the sprite renderer was turned off accidentally while the enemy is alive, re-enable it.
        if (currentHealth > 0 && sr != null && !sr.enabled)
        {
            Debug.Log($"RanchHandEnemy '{name}': re-enabling SpriteRenderer because enemy is alive.");
            sr.enabled = true;
        }
    }

    private void OnBecameVisible()
    {
        Debug.Log($"RanchHandEnemy '{name}' OnBecameVisible at {transform.position}");
    }

    private void OnDisable()
    {
        Debug.Log($"RanchHandEnemy '{name}' OnDisable (alive? {currentHealth > 0})");
        if (currentHealth > 0)
        {
            Debug.LogWarning($"RanchHandEnemy '{name}': object was disabled while alive. Stack trace:");
            Debug.Log(new System.Diagnostics.StackTrace(true).ToString());
        }
    }

    private void OnEnable()
    {
        Debug.Log($"RanchHandEnemy '{name}' OnEnable (alive? {currentHealth > 0})");
    }
}
