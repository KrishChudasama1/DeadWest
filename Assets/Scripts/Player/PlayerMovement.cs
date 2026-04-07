using System.Collections;
using System.Collections.Generic;
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
    // Active speed modifiers (multiplicative). 1 = default speed.
    private List<float> _speedModifiers = new List<float>();
    private float _speedMultiplier = 1f;
    // Track whether player currently has any slow multipliers applied
    private bool _isSlowed = false;

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

        float baseSpeed = Input.GetKey(KeyCode.LeftShift)
            ? moveSpeed * sprintMultiplier
            : moveSpeed;

        float speed = baseSpeed * _speedMultiplier;

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

    // Add a multiplicative speed modifier for a duration (e.g. 0.7f for -30%).
    public void AddTemporarySpeedMultiplier(float multiplier, float duration)
    {
        if (multiplier <= 0f) return;

        _speedModifiers.Add(multiplier);
        RecalculateSpeedMultiplier();

        StartCoroutine(SpeedModifierRoutine(multiplier, duration));
    }

    private IEnumerator SpeedModifierRoutine(float multiplier, float duration)
    {
        yield return new WaitForSeconds(duration);
        // Remove one instance of this multiplier
        _speedModifiers.Remove(multiplier);
        RecalculateSpeedMultiplier();
    }

    private void RecalculateSpeedMultiplier()
    {
        float prod = 1f;
        for (int i = 0; i < _speedModifiers.Count; i++)
            prod *= _speedModifiers[i];

        _speedMultiplier = prod;
        bool nowSlowed = _speedMultiplier < 0.999f;
        if (nowSlowed != _isSlowed)
        {
            _isSlowed = nowSlowed;
            RefreshTint();
        }
    }

    // Update sprite tint based on slowed state (green when slowed, white otherwise)
    public void RefreshTint()
    {
        if (_spriteRenderer == null) return;

        if (_isSlowed)
            _spriteRenderer.color = Color.green;
        else
            _spriteRenderer.color = Color.white;
    }

    // Persistent multiplier: adds immediately and stays until explicitly removed.
    public void AddSpeedMultiplier(float multiplier)
    {
        if (multiplier <= 0f) return;
        _speedModifiers.Add(multiplier);
        RecalculateSpeedMultiplier();
    }

    // Removes one instance of a previously added multiplier.
    public void RemoveSpeedMultiplier(float multiplier)
    {
        if (multiplier <= 0f) return;
        _speedModifiers.Remove(multiplier);
        RecalculateSpeedMultiplier();
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
