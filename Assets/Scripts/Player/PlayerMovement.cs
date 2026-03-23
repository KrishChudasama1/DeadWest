using UnityEngine;

/// <summary>
/// PlayerMovement.cs
///
/// Handles player movement, animation parameters, and sprite flipping.
///
/// Animator parameters set here:
///   Speed  — 0 (idle) or 1 (walking), drives the walk/idle blend tree
///   MoveX  — horizontal movement direction, drives walk blend tree
///   MoveY  — vertical movement direction, drives walk blend tree
///   GunX   — horizontal gun facing direction, drives gun blend trees
///   GunY   — vertical gun facing direction, drives gun blend trees
///
/// Public API called by Revolver:
///   SetGunDrawn(bool)                    — pauses movement-based sprite flipping
///   SetFacingDirection(float)            — manually flips sprite (gun mode)
///   SetMovementLocked(bool)              — freezes movement during shoot/reload
///   SetGunDirection(Vector2)             — updates GunX/GunY animator params
///   SetMovementStartedCallback(Action)   — fires when player moves while gun drawn
/// </summary>
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

    // ─── Unity lifecycle ──────────────────────────────────────────────────────

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

    // ─── Public API (called by Revolver) ──────────────────────────────────────

    /// Prevents movement input from overriding the gun facing direction.
    public void SetGunDrawn(bool drawn)
    {
        _gunDrawn = drawn;
    }

    /// Flips the sprite to face a direction.
    /// xDirection > 0 = face right (flips left-facing sprites).
    /// xDirection < 0 = face left (no flip).
    public void SetFacingDirection(float xDirection)
    {
        _spriteRenderer.flipX = xDirection > 0;
    }

    /// Completely freezes player movement and zeroes velocity.
    public void SetMovementLocked(bool locked)
    {
        _movementLocked = locked;
        if (locked)
            _rb.linearVelocity = Vector2.zero;
    }

    /// Updates GunX/GunY so gun blend trees show the correct
    /// directional sprite independent of movement direction.
    public void SetGunDirection(Vector2 direction)
    {
        _animator.SetFloat("GunX", direction.x);
        _animator.SetFloat("GunY", direction.y);
    }

    /// Registers a callback that fires when the player presses a movement
    /// key while the gun is drawn. Used by Revolver to trigger holstering.
    public void SetMovementStartedCallback(System.Action callback)
    {
        _onMovementStarted = callback;
    }
}