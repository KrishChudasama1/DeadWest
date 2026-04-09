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

    [Header("Drops")]
    public GameObject coinPrefab;
    public int coinDropAmount = 1;
    public float coinDropSpread = 0.25f;

    [Header("Death Goop")]
    [Tooltip("Prefab for the slow-goop zone spawned when this enemy dies.")]
    public GameObject slowZonePrefab;
    [Tooltip("Multiplier applied to player speed while inside the goop (0.7 = 30% slow).")]
    public float slowMultiplier = 0.7f;
    [Header("Audio")]
    [Tooltip("One-shot sound played when this ranch hand dies.")]
    public AudioClip deathClip;
    [Range(0f,1f)] public float deathVolume = 1f;

    private Transform player;
    private Animator animator;
    private SpriteRenderer sr;
    private bool isAttacking = false;
    private bool canAttack = true;
    private bool prevSrEnabled = true;
    private bool _isDying = false;

    void Start()
    {
        // Let all ranch hands on the same layer pass through each other
        Physics2D.IgnoreLayerCollision(gameObject.layer, gameObject.layer, true);

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
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
        DropCoins();
        DropOrGrantXP();
        SpawnSlowZone();

        if (deathClip != null)
        {
            AudioSource.PlayClipAtPoint(deathClip, transform.position, deathVolume);
        }

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

    void SpawnSlowZone()
    {
        if (slowZonePrefab == null) return;

        GameObject goop = Instantiate(slowZonePrefab, transform.position, Quaternion.identity);
        SlowZone sz = goop.GetComponent<SlowZone>();
        if (sz != null)
        {
            sz.playerSpeedMultiplier = slowMultiplier;
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
                Debug.LogWarning("xpPickupPrefab is missing an XPPickup component.");

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

    private void LateUpdate()
    {
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


    private void OnBecameInvisible()
    {
        Debug.Log($"RanchHandEnemy '{name}' OnBecameInvisible at {transform.position}");

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
