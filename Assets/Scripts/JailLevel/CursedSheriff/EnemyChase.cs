using UnityEngine;
using System.Collections;

public class EnemyChase : MonoBehaviour
{
    [Header("Chase")]
    public Transform player;
    public float moveSpeed = 2f;
    public float chaseRange = 6f;
    public float stopDistance = 1.5f;

    [Header("Dual Revolver")]
    public GameObject bulletPrefab;
    public Transform leftMuzzle;
    public Transform rightMuzzle;
    public float shootRange = 4f;
    public float fireCooldown = 1f;
    public int bulletDamage = 8;

    [Header("Health")]
    public int maxHealth = 6;
    public float hitFlashDuration = 0.12f;
    public Color hitFlashColor = Color.red;

    [Header("Shooting Animation")]
    public float shootingAnimTime = 0.2f;
    public bool stopWhileShooting = true;
    [Tooltip("Safety window so Animator has enough time to enter a shoot state.")]
    public float minShootingStateTime = 0.35f;

    [Header("Music On Death")]
    public AudioSource levelMusicSource;
    public AudioClip sheriffDefeatedMusic;
    public bool loopDefeatedMusic = true;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 movement;
    private Vector2 facingDirection = Vector2.down;
    private float nextShotTime;
    private float shootingAnimEndTime;
    private int currentHealth;
    private bool isDead;
    private Color originalColor = Color.white;
    private Coroutine flashCoroutine;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        currentHealth = maxHealth;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
    }

    void Update()
    {
        if (isDead)
            return;

        if (player == null)
        {
            movement = Vector2.zero;
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsShooting", false);
            animator.SetFloat("MoveX", facingDirection.x);
            animator.SetFloat("MoveY", facingDirection.y);
            return;
        }

        Vector2 toPlayer = player.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;

        if (distanceToPlayer <= chaseRange && distanceToPlayer > stopDistance)
        {
            Vector2 direction = toPlayer.normalized;
            movement = direction;

            animator.SetBool("IsMoving", true);

            // Use dominant axis so directional states don't fight each other.
            if (Mathf.Abs(movement.y) >= Mathf.Abs(movement.x))
            {
                facingDirection = new Vector2(0f, Mathf.Sign(movement.y));
            }
            else
            {
                facingDirection = new Vector2(Mathf.Sign(movement.x), 0f);
            }

            animator.SetFloat("MoveX", facingDirection.x);
            animator.SetFloat("MoveY", facingDirection.y);
        }
        else
        {
            movement = Vector2.zero;

            if (distanceToPlayer <= chaseRange)
                UpdateFacingFromDirection(toPlayer.normalized);

            animator.SetBool("IsMoving", false);
            animator.SetFloat("MoveX", facingDirection.x);
            animator.SetFloat("MoveY", facingDirection.y);
        }

        if (distanceToPlayer <= shootRange)
            TryShoot(toPlayer.normalized);

        bool isShooting = Time.time < shootingAnimEndTime;
        animator.SetBool("IsShooting", isShooting);

        if (stopWhileShooting && isShooting)
        {
            movement = Vector2.zero;
            animator.SetBool("IsMoving", false);
        }
    }

    void FixedUpdate()
    {
        if (isDead)
            return;

        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }

    public void TakeDamage(int amount)
    {
        if (isDead)
            return;

        currentHealth -= amount;
        TriggerHitFlash();

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;
        movement = Vector2.zero;
        animator.SetBool("IsMoving", false);
        animator.SetBool("IsShooting", false);

        SwapMusicOnDeath();

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        Destroy(gameObject);
    }

    private void SwapMusicOnDeath()
    {
        if (levelMusicSource == null || sheriffDefeatedMusic == null)
            return;

        levelMusicSource.Stop();
        levelMusicSource.clip = sheriffDefeatedMusic;
        levelMusicSource.loop = loopDefeatedMusic;
        levelMusicSource.Play();
    }

    private void TriggerHitFlash()
    {
        if (spriteRenderer == null)
            return;

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
        flashCoroutine = null;
    }

    private void TryShoot(Vector2 shootDirection)
    {
        if (Time.time < nextShotTime || bulletPrefab == null)
            return;

        if (shootDirection.sqrMagnitude < 0.0001f)
            return;

        nextShotTime = Time.time + fireCooldown;
        shootingAnimEndTime = Time.time + Mathf.Max(shootingAnimTime, minShootingStateTime);
        UpdateFacingFromDirection(shootDirection);

        FireFromMuzzle(leftMuzzle, shootDirection);
        FireFromMuzzle(rightMuzzle, shootDirection);
    }

    private void FireFromMuzzle(Transform muzzle, Vector2 direction)
    {
        Vector3 spawnPos = muzzle != null
            ? muzzle.position
            : transform.position + (Vector3)(direction * 0.5f);

        GameObject bulletGO = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        Bullet bullet = bulletGO.GetComponent<Bullet>();

        if (bullet != null)
            bullet.Init(direction, bulletDamage, true, false, transform);
        else
            Destroy(bulletGO);
    }

    private void UpdateFacingFromDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.y) >= Mathf.Abs(direction.x))
            facingDirection = new Vector2(0f, Mathf.Sign(direction.y));
        else
            facingDirection = new Vector2(Mathf.Sign(direction.x), 0f);

        animator.SetFloat("MoveX", facingDirection.x);
        animator.SetFloat("MoveY", facingDirection.y);
    }
}