using System;
using System.Collections;
using UnityEngine;

namespace StableLevel
{
    public class PhantomRider : MonoBehaviour
    {
        [Header("Waypoints")]
        [Tooltip("Empty GameObjects forming an oval loop. The rider cycles through them continuously.")]
        public Transform[] waypoints;

        [Header("Movement")]
        public float moveSpeed = 6f;

        [Header("Health")]
        public int maxHits = 3;

        [Header("Popup On Death")]
        [SerializeField] private ImagePopup deathImagePopup;

        public event Action OnRiderDefeated;

      
        private int currentHits = 0;
        private bool isDefeated = false;
        private bool isInvincible = false;
        private bool isCharging = false;       // true when charging at the player after timeout
        private int currentWaypointIndex = 0;
        private Rigidbody2D rb;
        private SpriteRenderer sr;
        private Transform _chargeTarget;       // cached player transform for charge
        
    [Header("Audio")]
    [Tooltip("Short neigh sound played when the rider starts running or when defeated.")]
    public AudioClip neighClip;

    [Tooltip("Looping gallop audio played while the rider is moving.")]
    public AudioClip gallopLoopClip;

    [Range(0f,1f)]
    public float gallopVolume = 0.8f;

    private AudioSource _sfxSource; // neigh
    private AudioSource _loopSource; // for looping gallop

       

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();

            
            gameObject.SetActive(false);
            
            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.playOnAwake = false;

            _loopSource = gameObject.AddComponent<AudioSource>();
            _loopSource.playOnAwake = false;
            _loopSource.loop = true;
            _loopSource.volume = gallopVolume;
        }

        
        public void Activate()
        {
            gameObject.SetActive(true);
            currentWaypointIndex = 0;
            Debug.Log("PhantomRider: Activated.");
            // Play neigh and start gallop loop when rider begins running
            if (neighClip != null && _sfxSource != null)
                _sfxSource.PlayOneShot(neighClip);

            if (gallopLoopClip != null && _loopSource != null && !_loopSource.isPlaying)
            {
                _loopSource.clip = gallopLoopClip;
                _loopSource.volume = gallopVolume;
                _loopSource.Play();
            }
        }

        private void FixedUpdate()
        {
            if (isDefeated) return;

    
            if (isCharging)
            {
                if (_chargeTarget == null) return;

                Vector2 targetPos = _chargeTarget.position;
                float chargeSpeed = moveSpeed * 3f; // much faster than normal patrol
                Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, chargeSpeed * Time.fixedDeltaTime);

                float dirX = targetPos.x - rb.position.x;
                if (Mathf.Abs(dirX) > 0.01f)
                    sr.flipX = dirX < 0f;

                rb.MovePosition(newPos);

                // If close enough to the player, deal lethal damage directly
                if (Vector2.Distance(rb.position, targetPos) < 0.5f)
                {
                    DealLethalDamage();
                }
                return;
            }

           
            if (waypoints == null || waypoints.Length == 0) return;

            Transform target = waypoints[currentWaypointIndex];
            if (target == null) return;

            Vector2 wpPos = target.position;
            Vector2 wpNewPos = Vector2.MoveTowards(rb.position, wpPos, moveSpeed * Time.fixedDeltaTime);

            float wpDirX = wpPos.x - rb.position.x;
            if (Mathf.Abs(wpDirX) > 0.01f)
                sr.flipX = wpDirX < 0f;

            rb.MovePosition(wpNewPos);

            if (Vector2.Distance(rb.position, wpPos) < 0.15f)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
        }

       
        public void ChargeAndKillPlayer()
        {
            if (isDefeated) return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("PhantomRider: ChargeAndKillPlayer — no Player found.");
                return;
            }

            _chargeTarget = player.transform;
            isCharging = true;
            isInvincible = true; // can't be lassoed while charging

            // Visual feedback — tint red to signal danger
            if (sr != null)
                sr.color = Color.red;

            Debug.Log("PhantomRider: CHARGING at the player!");
        }

        private void DealLethalDamage()
        {
            if (_chargeTarget == null) return;

            PlayerHealth ph = _chargeTarget.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                // Deal damage equal to full max health to guarantee a kill
                ph.TakeDamage(ph.maxHealth);
                Debug.Log("PhantomRider: dealt lethal damage to the player.");
            }
            else
            {
                Debug.LogWarning("PhantomRider: PlayerHealth not found on target.");
            }

            // Stop charging
            isCharging = false;
        }

      

       
        public void TakeLassoHit()
        {
            if (isDefeated || isInvincible) return;

            currentHits++;
            Debug.Log($"PhantomRider: lasso hit {currentHits}/{maxHits}");

            StartCoroutine(FlashWhite());
            StartCoroutine(InvincibilityWindow(0.75f));

            // Speed up after each hit
            moveSpeed *= 1.25f;

            if (currentHits >= maxHits)
            {
                isDefeated = true;
                Debug.Log("PhantomRider: defeated!");
                deathImagePopup?.ShowImage();
                OnRiderDefeated?.Invoke();
                // Play death neigh and stop gallop
                if (neighClip != null && _sfxSource != null)
                    _sfxSource.PlayOneShot(neighClip);
                if (_loopSource != null && _loopSource.isPlaying)
                    _loopSource.Stop();

                StartCoroutine(DefeatSequence());
            }
        }

       

        private IEnumerator FlashWhite()
        {
            if (sr == null) yield break;

            Color original = sr.color;
            int flashes = 3;
            float flashDuration = 0.08f;

            for (int i = 0; i < flashes; i++)
            {
                sr.color = Color.red;
                yield return new WaitForSeconds(flashDuration);
                sr.color = original;
                yield return new WaitForSeconds(flashDuration);
            }

            sr.color = original; // ensure reset
        }

        private IEnumerator InvincibilityWindow(float duration)
        {
            isInvincible = true;
            yield return new WaitForSeconds(duration);
            isInvincible = false;
        }

        private IEnumerator DefeatSequence()
        {
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }

       

        private void OnDrawGizmosSelected()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                Gizmos.DrawWireSphere(waypoints[i].position, 0.25f);
            }

            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                int next = (i + 1) % waypoints.Length;
                if (waypoints[next] == null) continue;
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
            }
        }
    }
}
