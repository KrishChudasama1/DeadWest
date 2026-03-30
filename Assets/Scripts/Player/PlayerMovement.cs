using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed        = 5f;
    public float sprintMultiplier = 1.6f;

    private Rigidbody2D    _rb;
    private Animator       _animator;
    private SpriteRenderer _spriteRenderer;
    private Vector2        _movement;

    private bool           _gunDrawn;
    private bool           _movementLocked;
    private System.Action  _onMovementStarted;

    

    private void Start()
    {
        _rb             = GetComponent<Rigidbody2D>();
        _animator       = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (_movementLocked)
        {
            _movement = Vector2.zero;
            _animator.SetFloat("Speed", 0f);
            return;
        }

        _movement.x = Input.GetAxisRaw("Horizontal");
        _movement.y = Input.GetAxisRaw("Vertical");

        // Notify Revolver the moment movement starts while gun is drawn
        if (_movement != Vector2.zero && _gunDrawn)
            _onMovementStarted?.Invoke();

        _animator.SetFloat("Speed", _movement.magnitude);

        if (_movement != Vector2.zero)
        {
            _animator.SetFloat("MoveX", _movement.x);
            _animator.SetFloat("MoveY", _movement.y);
        }

        // Only flip from movement when gun is not drawn
        if (!_gunDrawn && _movement.x != 0)
            _spriteRenderer.flipX = _movement.x > 0;
    }

    private void FixedUpdate()
    {
        if (_movementLocked)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        float speed = Input.GetKey(KeyCode.LeftShift)
            ? moveSpeed * sprintMultiplier
            : moveSpeed;

        _rb.MovePosition(_rb.position + _movement.normalized * speed * Time.fixedDeltaTime);
    }

    
    public void SetGunDrawn(bool drawn)
    {
        _gunDrawn = drawn;
    }

   
    public void SetFacingDirection(float xDirection)
    {
        _spriteRenderer.flipX = xDirection > 0;
    }

    
    public void SetMovementLocked(bool locked)
    {
        _movementLocked = locked;
        if (locked)
            _rb.linearVelocity = Vector2.zero;
    }

    
    public void SetGunDirection(Vector2 direction)
    {
        _animator.SetFloat("GunX", direction.x);
        _animator.SetFloat("GunY", direction.y);
    }

   
    public void SetMovementStartedCallback(System.Action callback)
    {
        _onMovementStarted = callback;
    }
}