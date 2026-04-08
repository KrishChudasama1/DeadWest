using UnityEngine;

public class RestlessUndead : MonoBehaviour
{
    public float health = 50f;
    public float respawnHealth = 25f;
    public float respawnDelay = 2f;
    public GameObject respawnEffect;

    [Header("Drops")]
    public GameObject coinPrefab;
    public GameObject xpOrbPrefab;
    public int coinDropAmount = 3;
    public int xpDropAmount = 20;
    public float dropSpread = 0.5f;

    private bool hasRespawned = false;
    private bool isDying = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private Animator animator;
    private RestlessUndeadMovement movement;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        movement = GetComponent<RestlessUndeadMovement>();
    }

    public void TakeDamage(float damage)
    {
        if (isDying) return;

        health -= damage;

        if (health <= 0)
        {
            if (!hasRespawned)
                StartCoroutine(Respawn());
            else
                Die();
        }
    }

    void SpawnDrops()
    {
        for (int i = 0; i < coinDropAmount; i++)
        {
            if (coinPrefab != null)
            {
                Vector2 offset = Random.insideUnitCircle * dropSpread;
                Instantiate(coinPrefab, (Vector2)transform.position + offset, Quaternion.identity);
            }
        }

        if (xpOrbPrefab != null)
        {
            Vector2 offset = Random.insideUnitCircle * dropSpread;
            Instantiate(xpOrbPrefab, (Vector2)transform.position + offset, Quaternion.identity);
        }

        XPManager xpManager = FindObjectOfType<XPManager>();
        if (xpManager != null)
            xpManager.GainExperience(xpDropAmount);
    }

    System.Collections.IEnumerator Respawn()
    {
        hasRespawned = true;
        isDying = true;

        col.enabled = false;
        if (movement != null) movement.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        animator.SetBool("IsWalking", false);
        animator.SetBool("IsAttacking", false);
        animator.SetBool("IsReviving", false);
        animator.SetBool("IsDead", true);

        yield return new WaitForSeconds(respawnDelay);

        animator.SetBool("IsDead", false);
        animator.SetBool("IsReviving", true);
        isDying = false;

        yield return new WaitForSeconds(1.5f);

        animator.SetBool("IsReviving", false);
        health = respawnHealth;
        col.enabled = true;
        if (movement != null) movement.enabled = true;

        if (respawnEffect != null)
            Instantiate(respawnEffect, transform.position, Quaternion.identity);
    }

    void Die()
    {
        if (isDying) return;
        isDying = true;

        animator.SetBool("IsWalking", false);
        animator.SetBool("IsAttacking", false);
        animator.SetBool("IsReviving", false);
        animator.SetBool("IsDead", true);

        if (movement != null) movement.enabled = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        col.enabled = false;

        SpawnDrops();
        StartCoroutine(WaitForDeathAnimation());
    }

    System.Collections.IEnumerator WaitForDeathAnimation()
    {
        yield return null;
        yield return null;

        float timer = 0f;
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName("SkeletonDeath"))
        {
            timer += Time.deltaTime;
            if (timer > 1f) break;
            yield return null;
        }

        float deathLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(deathLength);

        Destroy(gameObject);
    }
}