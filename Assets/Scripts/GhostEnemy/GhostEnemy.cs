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
    public int maxHealth = 50;
    private int currentHealth;

    private Transform player;
    private Animator animator;
    private bool isAttacking = false;
    private bool canAttack = true;
    private float distanceToPlayer;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (player == null) return;

        distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && canAttack)
        {
            StartCoroutine(Attack());
        }
        else if (distanceToPlayer <= chaseRange && !isAttacking)
        {
            ChasePlayer();
        }
        else if (!isAttacking)
        {
            animator.SetBool("IsMoving", false);
        }
    }

    void ChasePlayer()
    {
        animator.SetBool("IsMoving", true);
        animator.SetBool("IsAttacking", false);

        Vector2 direction = (player.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            moveSpeed * Time.deltaTime
        );

        // Flip sprite to face player
        if (direction.x != 0)
            GetComponent<SpriteRenderer>().flipX = direction.x > 0;
    }

    IEnumerator Attack()
    {
        isAttacking = true;
        canAttack = false;

        animator.SetBool("IsMoving", false);
        animator.SetBool("IsAttacking", true);

        // Wait for attack animation to play
        yield return new WaitForSeconds(0.3f);

        // Deal damage if still in range
        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(0.3f);
        animator.SetBool("IsAttacking", false);

        // Cooldown before next attack
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
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        sr.color = Color.white;
    }

    void Die()
    {
        // Add death animation or effects here later
        Destroy(gameObject);
    }

    // Visualize ranges in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
    }
}