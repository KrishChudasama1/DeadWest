using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.6f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Pass speed to animator (0 = idle, 1 = walking)
        animator.SetFloat("Speed", movement.magnitude);

        // Pass direction so animator knows which walk clip to play
        if (movement != Vector2.zero)
        {
            animator.SetFloat("MoveX", movement.x);
            animator.SetFloat("MoveY", movement.y);
        }

        // Flip sprite left/right instead of needing separate left sprite
        if (movement.x != 0)
            spriteRenderer.flipX = movement.x > 0;
    }

    void FixedUpdate()
    {
        float speed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
            speed *= sprintMultiplier;

        rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
    }
}