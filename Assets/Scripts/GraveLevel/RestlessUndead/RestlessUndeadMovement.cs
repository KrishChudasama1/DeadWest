using UnityEngine;

public class RestlessUndeadMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float detectionRange = 8f;
    public float attackRange = 1.5f;
    public int attackDamage = 10;
    public AudioClip attackSound;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private AudioSource audioSource;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsAttacking", true);
            isAttacking = true;
        }
        else if (distanceToPlayer <= detectionRange)
        {
            isAttacking = false;
            animator.SetBool("IsAttacking", false);
            animator.SetBool("IsWalking", true);

            Vector2 direction = (player.position - transform.position).normalized;
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
            spriteRenderer.flipX = player.position.x < transform.position.x;
        }
        else
        {
            isAttacking = false;
            animator.SetBool("IsAttacking", false);
            animator.SetBool("IsWalking", false);
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void DealDamage()
    {
        if (!isAttacking) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            if (audioSource != null && attackSound != null)
                audioSource.PlayOneShot(attackSound);

            PlayerHealth playerHealth = player.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    void OnDisable()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}