using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]

public class CursedBrawler : MonoBehaviour
{
    public enum BrawlerState
    {
        Idle,
        Moving,
        Locking,
        Charging,
        Stunned,
        Attacking
    }

    [Header("State")]
    public BrawlerState currentState = BrawlerState.Idle;

    [Header("Health")]
    public int maxHealth = 10;
    private int currentHealth;

    [Header("XP")]
    public int xpOnDeath = 2;
    public GameObject xpPickupPrefab;

    [Header("Drops")]
    public GameObject coinPrefab;
    public int coinDropAmount = 1;
    public float coinDropSpread = 0.25f;

    [Header("Detection")]
    public float chaseRange = 10f;
    public float meleeRange = 1.3f;

    [Header("Movement")]
    public float moveSpeed = 1.5f;

    [Header("Regular Melee")]
    public int meleeDamage = 12;
    public float meleeCooldown = 1.2f;
    public float meleeWindup = 0.2f;
    public float meleeRecovery = 0.35f;
    public float meleeStopDistanceBuffer = 0.1f;

    [Header("Charge Ability")]
    public float chargeCooldown = 5f;
    public float minChargeDistance = 2f;
    public float lockDuration = 0.5f;
    public float chargeSpeed = 8f;
    public float maxChargeDistance = 4.5f;
    public Vector2 boxCastSizeMultiplier = new Vector2(0.9f, 0.9f);
    public float collisionCheckPadding = 0.05f;

    [Header("Charge Hit")]
    public int chargeImpactDamage = 20;
    public float playerStunDuration = 0.75f;
    public float chargeImpactPause = 0.15f;

    [Header("Recovery")]
    public float stunnedDuration = 1.5f;

    [Header("Collision")]
    public LayerMask obstacleMask;

    [Header("Audio")]
    public AudioClip lockOnSound;
    public AudioClip chargeLoopSound;
    public AudioClip chargeImpactSound;
    public AudioClip meleeThudA;
    public AudioClip meleeThudB;
    public AudioClip groundSlamSound;
    public AudioClip deathSound;

    [Header("Audio Volume")]
    [Range(0f, 1f)] public float deathVolume = 0.7f;
    [Range(0f, 1f)] public float chargeImpactVolume = 1f;
    
    private AudioSource audioSource;
    private AudioSource chargeAudioSource;

    private Transform player;
    private Rigidbody2D rb;
    private Collider2D col;
    private Animator animator;
    private SpriteRenderer sr;

    private Vector2 lockedDirection;
    private Vector2 moveDirection;
    private Vector2 chargeStartPosition;

    private float meleeCooldownTimer = 0f;
    private float chargeCooldownTimer = 0f;

    private bool isBusy = false;
    private bool isChargingActive = false;
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

        chargeAudioSource = gameObject.AddComponent<AudioSource>();
        chargeAudioSource.loop = true;
        chargeAudioSource.playOnAwake = false;

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.linearVelocity = Vector2.zero;

        currentHealth = maxHealth;
        chargeCooldownTimer = chargeCooldown;
        ChangeState(BrawlerState.Moving);
    }

    private void Update()
    {
        if (player == null || isDead) return;

        if (meleeCooldownTimer > 0f)
            meleeCooldownTimer -= Time.deltaTime;

        if (chargeCooldownTimer > 0f)
            chargeCooldownTimer -= Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (currentState == BrawlerState.Stunned ||
            currentState == BrawlerState.Attacking ||
            currentState == BrawlerState.Locking ||
            currentState == BrawlerState.Charging)
        {
            return;
        }

        if (distanceToPlayer > chaseRange)
        {
            ChangeState(BrawlerState.Idle);
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (currentState == BrawlerState.Idle)
            ChangeState(BrawlerState.Moving);

        if (currentState == BrawlerState.Moving)
            HandleMoving(distanceToPlayer);
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        if (currentState == BrawlerState.Charging)
            HandleCharging();
        else if (currentState == BrawlerState.Moving)
            HandleMovingPhysics();
        else if (currentState != BrawlerState.Locking)
            rb.linearVelocity = Vector2.zero;
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

    private void StartChargeLoop()
    {
        if (chargeLoopSound == null) return;
        chargeAudioSource.clip = chargeLoopSound;
        chargeAudioSource.Play();
    }

    private void StopChargeLoop()
    {
        if (chargeAudioSource.isPlaying)
            chargeAudioSource.Stop();
    }

    private void PlayRandomMeleeThud()
    {
        if (meleeThudA == null && meleeThudB == null) return;
        if (meleeThudA == null) { PlaySound(meleeThudB); return; }
        if (meleeThudB == null) { PlaySound(meleeThudA); return; }
        PlaySound(Random.value > 0.5f ? meleeThudA : meleeThudB);
    }

   
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log("CursedBrawler took damage! HP remaining: " + currentHealth);

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
        StopChargeLoop();
        PlaySound(deathSound, deathVolume);
        sr.enabled = false; 
        DropCoins();
        DropOrGrantXP();
        Debug.Log("CursedBrawler died!");
        Destroy(gameObject, deathSound != null ? deathSound.length + 0.1f : 0.5f);
    }

    private void DropCoins()
    {
        if (coinPrefab == null || coinDropAmount <= 0)
            return;

        for (int i = 0; i < coinDropAmount; i++)
        {
            Vector2 offset = Random.insideUnitCircle * coinDropSpread;
            Instantiate(coinPrefab, (Vector2)transform.position + offset, Quaternion.identity);
        }
    }

    private void DropOrGrantXP()
    {
        if (xpOnDeath <= 0)
            return;

        if (xpPickupPrefab != null)
        {
            GameObject drop = Instantiate(xpPickupPrefab, transform.position, Quaternion.identity);
            XPPickup pickup = drop.GetComponent<XPPickup>();
            if (pickup != null)
            {
                pickup.xpValue = xpOnDeath;
            }
            else
            {
                Debug.LogWarning("xpPickupPrefab is missing an XPPickup component.");
            }
            return;
        }

    XPManager.AddExperience(xpOnDeath);
    }

   
    private void HandleMoving(float distanceToPlayer)
    {
        if (player == null) return;

        Vector2 direction = ((Vector2)player.position - rb.position).normalized;
        bool inMeleeSpace = distanceToPlayer <= meleeRange + meleeStopDistanceBuffer;
        moveDirection = inMeleeSpace ? Vector2.zero : direction;

        UpdateFacing(direction);

        if (isBusy) return;

        if (CanUseMelee(distanceToPlayer))
        {
            StartCoroutine(DoMeleeAttack());
            return;
        }

        if (CanUseCharge(distanceToPlayer))
        {
            StartCoroutine(LockAndCharge());
            return;
        }

        if (distanceToPlayer <= meleeRange + meleeStopDistanceBuffer)
            moveDirection = direction * 0.3f;
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

    
    private IEnumerator DoMeleeAttack()
    {
        if (isBusy || player == null)
            yield break;

        isBusy = true;
        meleeCooldownTimer = meleeCooldown;

        ChangeState(BrawlerState.Attacking);
        rb.linearVelocity = Vector2.zero;

        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(meleeWindup);

        PlaySound(groundSlamSound);

        PlayRandomMeleeThud();

        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= meleeRange + 0.15f)
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(meleeDamage);
            }
        }

        yield return new WaitForSeconds(meleeRecovery);

        ChangeState(BrawlerState.Moving);
        isBusy = false;
    }

    
    private IEnumerator LockAndCharge()
    {
        if (isBusy || player == null)
            yield break;

        isBusy = true;
        chargeCooldownTimer = chargeCooldown;

        ChangeState(BrawlerState.Locking);
        rb.linearVelocity = Vector2.zero;

        lockedDirection = ((Vector2)player.position - rb.position).normalized;
        if (lockedDirection == Vector2.zero)
            lockedDirection = Vector2.right;

        UpdateFacing(lockedDirection);

        PlaySound(lockOnSound);
        animator.SetTrigger("StartCharge");

        yield return new WaitForSeconds(lockDuration);

        Physics2D.IgnoreLayerCollision(gameObject.layer, gameObject.layer, true);

        chargeStartPosition = rb.position;
        isChargingActive = true;

        StartChargeLoop();

        ChangeState(BrawlerState.Charging);
    }

    private void HandleCharging()
    {
        if (!isChargingActive) return;

        Vector2 currentPos = rb.position;
        float moveStep = chargeSpeed * Time.fixedDeltaTime;
        Vector2 castSize = GetCastSize();

        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            currentPos,
            castSize,
            0f,
            lockedDirection,
            moveStep + collisionCheckPadding
        );

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider == null || hitCollider.isTrigger || IsSelfCollider(hitCollider))
                continue;

            if (hitCollider.CompareTag("Enemy"))
                continue;

            if (IsPlayerCollider(hitCollider))
            {
                isChargingActive = false;
                StopChargeLoop();
                PlaySound(chargeImpactSound);
                rb.linearVelocity = Vector2.zero;
                rb.MovePosition(hits[i].point - lockedDirection * 0.1f);
                StartCoroutine(HandleChargeHit(GetPlayerRoot(hitCollider.transform)));
                return;
            }

            if (IsObstacleCollider(hitCollider))
            {
                isChargingActive = false;
                StopChargeLoop();
                PlaySound(chargeImpactSound, chargeImpactVolume);

                BreakableObject breakable = hitCollider.GetComponent<BreakableObject>();
                if (breakable != null)
                    breakable.TakeDamage(chargeImpactDamage);

                StartCoroutine(DoStunned());
                return;
            }
        }

        rb.MovePosition(currentPos + lockedDirection * moveStep);

        float traveled = Vector2.Distance(chargeStartPosition, rb.position);
        if (traveled >= maxChargeDistance)
        {
            isChargingActive = false;
            StopChargeLoop();
            EndChargeAndResume();
        }
    }

    private IEnumerator HandleChargeHit(Transform playerTarget)
    {
        isChargingActive = false;
        rb.linearVelocity = Vector2.zero;

        if (playerTarget != null)
        {
            PlayerHealth playerHealth = playerTarget.GetComponentInParent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(chargeImpactDamage);

            PlayerMovement playerMovement = playerTarget.GetComponentInParent<PlayerMovement>();
            if (playerMovement != null)
                playerMovement.Stun(playerStunDuration);

            PlayerShooting playerShooting = playerTarget.GetComponentInParent<PlayerShooting>();
            if (playerShooting != null)
                playerShooting.Stun(playerStunDuration);
        }

        yield return new WaitForSeconds(chargeImpactPause);

        EndChargeAndResume();
    }

    
    private IEnumerator DoStunned()
    {
        ChangeState(BrawlerState.Stunned);
        rb.linearVelocity = Vector2.zero;
        Physics2D.IgnoreLayerCollision(gameObject.layer, gameObject.layer, false);

        yield return new WaitForSeconds(stunnedDuration);

        isBusy = false;
        ChangeState(BrawlerState.Moving);
    }

    private void EndChargeAndResume()
    {
        isChargingActive = false;
        StopChargeLoop();
        Physics2D.IgnoreLayerCollision(gameObject.layer, gameObject.layer, false);
        ChangeState(BrawlerState.Moving);
        isBusy = false;
    }

    
    private void ChangeState(BrawlerState newState)
    {
        currentState = newState;

        animator.SetBool("IsMoving",   newState == BrawlerState.Moving);
        animator.SetBool("IsStunned",  newState == BrawlerState.Stunned);
        animator.SetBool("IsCharging", newState == BrawlerState.Charging);
    }

    private void UpdateFacing(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > 0.01f)
            sr.flipX = direction.x < 0f;
    }

    private Vector2 GetCastSize()
    {
        Vector3 size = col.bounds.size;
        return new Vector2(
            Mathf.Max(0.1f, size.x * boxCastSizeMultiplier.x),
            Mathf.Max(0.1f, size.y * boxCastSizeMultiplier.y)
        );
    }

    private bool IsSelfCollider(Collider2D hitCollider)
    {
        return hitCollider == col || hitCollider.transform.IsChildOf(transform);
    }

    private bool CanUseMelee(float distanceToPlayer)
    {
        return distanceToPlayer <= meleeRange + meleeStopDistanceBuffer
               && meleeCooldownTimer <= 0f;
    }

    private bool CanUseCharge(float distanceToPlayer)
    {
        return chargeCooldownTimer <= 0f &&
               distanceToPlayer >= minChargeDistance &&
               distanceToPlayer > meleeRange;
    }

    private bool IsPlayerCollider(Collider2D hitCollider)
    {
        return hitCollider.CompareTag("Player") ||
               hitCollider.GetComponentInParent<PlayerHealth>() != null;
    }

    private bool IsObstacleCollider(Collider2D hitCollider)
    {
        if (((1 << hitCollider.gameObject.layer) & obstacleMask) != 0)
            return true;

        Transform current = hitCollider.transform.parent;
        while (current != null)
        {
            if (((1 << current.gameObject.layer) & obstacleMask) != 0)
                return true;
            current = current.parent;
        }

        return !IsPlayerCollider(hitCollider);
    }

    private Transform GetPlayerRoot(Transform hitTransform)
    {
        PlayerHealth playerHealth = hitTransform.GetComponentInParent<PlayerHealth>();
        return playerHealth != null ? playerHealth.transform : hitTransform;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position,
                (Vector2)transform.position + lockedDirection * maxChargeDistance);

            Gizmos.color = Color.magenta;
            if (col != null)
            {
                Vector2 size = GetCastSize();
                Gizmos.DrawWireCube(
                    (Vector2)transform.position + lockedDirection, size);
            }
        }
    }
}
