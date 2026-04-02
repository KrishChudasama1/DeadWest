using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintMultiplier = 1.6f;

    private Rigidbody2D _rb;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _collider;
    private Vector2 _movement;

    private bool _gunDrawn;
    private bool _movementLocked;
    private System.Action _onMovementStarted;

    private Coroutine _stunCoroutine;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
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

    public void Stun(float duration)
    {
        if (_stunCoroutine != null)
            StopCoroutine(_stunCoroutine);

        _stunCoroutine = StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        SetMovementLocked(true);
        yield return new WaitForSeconds(duration);
        SetMovementLocked(false);
        _stunCoroutine = null;
    }
    
    private void LateUpdate()
    {
        if (_spriteRenderer != null)
        {
            float feetY = _collider != null
                ? _collider.bounds.min.y
                : transform.position.y - _spriteRenderer.bounds.extents.y;
            _spriteRenderer.sortingOrder = Mathf.RoundToInt(-feetY * 100);
        }
    }
}
