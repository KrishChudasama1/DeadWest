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

        // ── Events ──────────────────────────────────────────────
        /// <summary>Fired when the rider takes the final lasso hit.</summary>
        public event Action OnRiderDefeated;

        // ── Private state ───────────────────────────────────────
        private int currentHits = 0;
        private bool isDefeated = false;
        private bool isInvincible = false;
        private int currentWaypointIndex = 0;
        private Rigidbody2D rb;
        private SpriteRenderer sr;

        // ────────────────────────────────────────────────────────
        // Unity callbacks
        // ────────────────────────────────────────────────────────

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();

            // Start inactive — caller uses Activate()
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Enables the GameObject and starts movement along the waypoint loop.
        /// </summary>
        public void Activate()
        {
            gameObject.SetActive(true);
            currentWaypointIndex = 0;
            Debug.Log("PhantomRider: Activated.");
        }

        private void FixedUpdate()
        {
            if (isDefeated) return;
            if (waypoints == null || waypoints.Length == 0) return;

            Transform target = waypoints[currentWaypointIndex];
            if (target == null) return;

            Vector2 targetPos = target.position;
            Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, moveSpeed * Time.fixedDeltaTime);

            // Flip sprite based on horizontal movement direction
            float dirX = targetPos.x - rb.position.x;
            if (Mathf.Abs(dirX) > 0.01f)
                sr.flipX = dirX < 0f;

            rb.MovePosition(newPos);

            // Advance to next waypoint when close enough
            if (Vector2.Distance(rb.position, targetPos) < 0.15f)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
        }

        // ────────────────────────────────────────────────────────
        // Lasso interaction
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Called by LassoProjectile when it hits this rider.
        /// NOT compatible with Bullet.cs (no TakeDamage(int)).
        /// </summary>
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
                OnRiderDefeated?.Invoke();
                StartCoroutine(DefeatSequence());
            }
        }

        // ────────────────────────────────────────────────────────
        // Coroutines
        // ────────────────────────────────────────────────────────

        private IEnumerator FlashWhite()
        {
            if (sr == null) yield break;

            sr.color = Color.white;
            yield return new WaitForSeconds(0.15f);
            sr.color = Color.white; // reset to default tint
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

        // ────────────────────────────────────────────────────────
        // Gizmos
        // ────────────────────────────────────────────────────────

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
