using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]

public class CorruptedPriest : MonoBehaviour
{
    public enum PriestState
    {
        Idle,
        Moving,
        Casting,
        Stunned
    }

    [Header("State")]
    public PriestState currentState = PriestState.Idle;

    [Header("Health")]
    public int maxHealth = 80;
    private int currentHealth;

    [Header("XP")]
    public int xpOnDeath = 3;
    public GameObject xpPickupPrefab;

    [Header("Detection")]
    public float chaseRange = 10f;
    public float castRange = 6f;

    [Header("Movement")]
    public float moveSpeed = 1.8f;

    [Header("Projectile Attack")]
    public GameObject corruptionProjectilePrefab;
    public int projectileDamage = 18;
    public float castCooldown = 2.5f;
    public float castWindup = 0.4f;
    public float castRecovery = 0.5f;
    public float projectileSpeed = 6f;
    public Transform projectileSpawnPoint;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float waitTimeAtPoint = 2f;

    [Header("Audio")]
    public AudioClip castSound;
    public AudioClip deathSound;
    public AudioClip hurtSound;

    [Header("Audio Volume")]
    [Range(0f, 1f)] public float deathVolume = 0.7f;
    [Range(0f, 1f)] public float castVolume = 1f;

    private AudioSource audioSource;

    private Transform player;
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator animator;
    private SpriteRenderer sr;

    private Vector2 moveDirection;

    private float castCooldownTimer = 0f;
    private float patrolWaitTimer = 0f;
    private int patrolIndex = 0;

    private bool isBusy = false;
    private bool isDead = false;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.linearVelocity = Vector2.zero;

        currentHealth = maxHealth;
        castCooldownTimer = castCooldown;

        ChangeState(PriestState.Moving);
    }

    private void Update()
    {
        if (player == null || isDead) return;

        if (castCooldownTimer > 0f)
            castCooldownTimer -= Time.deltaTime;

        if (patrolWaitTimer > 0f)
            patrolWaitTimer -= Time.deltaTime;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        if (currentState == PriestState.Stunned ||
            currentState == PriestState.Casting)
            return;

        if (distToPlayer > chaseRange)
        {
            if (currentState != PriestState.Idle)
                ChangeState(PriestState.Idle);
            HandlePatrol();
            return;
        }

        if (currentState == PriestState.Idle)
            ChangeState(PriestState.Moving);

        if (currentState == PriestState.Moving)
            HandleMoving(distToPlayer);
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        if (currentState == PriestState.Moving || currentState == PriestState.Idle)
            HandleMovingPhysics();
        else
            rb.linearVelocity = Vector2.zero;
    }

    private void HandleMoving(float distToPlayer)
    {
        if (player == null) return;

        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        UpdateFacing(direction);

        if (distToPlayer > castRange)
            moveDirection = direction;
        else
            moveDirection = Vector2.zero;

        if (isBusy) return;

        if (CanCast(distToPlayer))
        {
            StartCoroutine(DoCastAttack());
        }
    }

    private void HandlePatrol()
    {
        if (patrolPoints.Length == 0)
        {
            moveDirection = Vector2.zero;
            return;
        }

        if (patrolWaitTimer > 0f)
        {
            moveDirection = Vector2.zero;
            return;
        }

        Transform target = patrolPoints[patrolIndex];
        Vector2 direction = ((Vector2)target.position - rb.position).normalized;
        moveDirection = direction;
        UpdateFacing(direction);

        if (Vector2.Distance(transform.position, target.position) < 0.2f)
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            patrolWaitTimer = waitTimeAtPoint;
            moveDirection = Vector2.zero;
        }
    }

    private void HandleMovingPhysics()
    {
        if (moveDirection == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
    }

    private bool CanCast(float distToPlayer)
    {
        return castCooldownTimer <= 0f && distToPlayer <= castRange;
    }

    private IEnumerator DoCastAttack()
    {
        if (isBusy || player == null) yield break;

        isBusy = true;
        castCooldownTimer = castCooldown;

        ChangeState(PriestState.Casting);
        rb.linearVelocity = Vector2.zero;

        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        UpdateFacing(dir);

        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(castWindup);

        SpawnProjectile();
        PlaySound(castSound, castVolume);

        yield return new WaitForSeconds(castRecovery);

        ChangeState(PriestState.Moving);
        isBusy = false;
    }

    public void SpawnProjectile()
    {
        if (corruptionProjectilePrefab == null || player == null) return;

        Vector2 dir = ((Vector2)player.position - rb.position).normalized;

        Vector3 spawnPos = projectileSpawnPoint != null
            ? projectileSpawnPoint.position
            : transform.position + new Vector3(dir.x * 0.5f, 0.3f, 0);

        GameObject proj = Instantiate(corruptionProjectilePrefab, spawnPos, Quaternion.identity);

        CorruptedProjectile cp = proj.GetComponent<CorruptedProjectile>();
        if (cp != null)
            cp.Init(dir, projectileDamage, projectileSpeed);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log("CorruptedPriest took damage! HP: " + currentHealth);

        PlaySound(hurtSound);
        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
            Die();
    }

    private IEnumerator FlashRed()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        sr.color = Color.white;
    }

    private void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        col.enabled = false;
        PlaySound(deathSound, deathVolume);
        sr.enabled = false;
        DropOrGrantXP();
        ChurchMusicManager.SwitchToVictoryMusic();
        Debug.Log("CorruptedPriest died!");
        Destroy(gameObject, deathSound != null ? deathSound.length + 0.1f : 0.5f);
    }

    private void DropOrGrantXP()
    {
        if (xpOnDeath <= 0) return;

        if (xpPickupPrefab != null)
        {
            GameObject drop = Instantiate(xpPickupPrefab, transform.position, Quaternion.identity);
            XPPickup pickup = drop.GetComponent<XPPickup>();
            if (pickup != null)
                pickup.xpValue = xpOnDeath;
            else
                Debug.LogWarning("xpPickupPrefab is missing an XPPickup component.");
            return;
        }

        XPManager xpManager = null;
        if (player != null)
            xpManager = player.GetComponent<XPManager>();

        if (xpManager == null)
            xpManager = FindObjectOfType<XPManager>();

        if (xpManager != null)
            xpManager.GainExperience(xpOnDeath);
        else
            Debug.LogWarning("No XPManager found. Priest XP could not be awarded.");
    }

    private void ChangeState(PriestState newState)
    {
        currentState = newState;
        animator.SetBool("IsMoving", newState == PriestState.Moving);
        animator.SetBool("IsStunned", newState == PriestState.Stunned);
    }

    private void UpdateFacing(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > 0.01f)
            sr.flipX = direction.x < 0f;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip);
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip, volume);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, castRange);
    }
}