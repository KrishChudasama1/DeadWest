using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.6f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 movement;
    private Vector2 lastMoveDir;

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

        animator.SetFloat("Speed", movement.magnitude);
        animator.SetFloat("MoveX", movement.x);
        animator.SetFloat("MoveY", movement.y);

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