using System.Collections;
using UnityEngine;

namespace StableLevel
{
    /// <summary>
    /// Place this MonoBehaviour in the RaceTrack scene.
    /// It finds the PhantomRider in the scene, activates it after a short delay,
    /// and listens for the rider's defeat to advance to the next phase (duel).
    ///
    /// Setup:
    ///   1. In your RaceTrack scene, create an empty "RaceTrackManager" GameObject.
    ///   2. Attach this script.
    ///   3. Place the PhantomRider prefab in the scene (it deactivates itself in Awake).
    ///   4. Optionally assign playerSpawnPoint — the spot the player appears at.
    /// </summary>
    public class RaceTrackManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Drag the PhantomRider in this scene. If left empty, it will be found automatically.")]
        public PhantomRider phantomRider;

        [Tooltip("Where the player should spawn when this scene loads.")]
        public Transform playerSpawnPoint;

        [Header("Timing")]
        [Tooltip("Seconds after scene load before the rider activates.")]
        [SerializeField] private float riderActivateDelay = 3f;

        // ── Private ─────────────────────────────────────────────
        private bool riderDefeated = false;

        // ────────────────────────────────────────────────────────

        private void Start()
        {
            // Move the player to the spawn point (if assigned)
            PositionPlayer();

            // Find the rider if not assigned
            if (phantomRider == null)
                phantomRider = FindObjectOfType<PhantomRider>(true); // include inactive

            if (phantomRider == null)
            {
                Debug.LogWarning("RaceTrackManager: No PhantomRider found in the scene!");
                return;
            }

            phantomRider.OnRiderDefeated += HandleRiderDefeated;
            StartCoroutine(ActivateRiderAfterDelay());
        }

        private void OnDestroy()
        {
            if (phantomRider != null)
                phantomRider.OnRiderDefeated -= HandleRiderDefeated;
        }

        // ────────────────────────────────────────────────────────
        // Player positioning
        // ────────────────────────────────────────────────────────

        private void PositionPlayer()
        {
            if (playerSpawnPoint == null) return;

            // The player persists across scenes via DontDestroyOnLoad, so find them.
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("RaceTrackManager: No GameObject tagged 'Player' found.");
                return;
            }

            player.transform.position = playerSpawnPoint.position;
            Debug.Log($"RaceTrackManager: Player positioned at {playerSpawnPoint.position}");

            // Unlock movement in case it was locked during the gate transition
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null)
                pm.SetMovementLocked(false);
        }

        // ────────────────────────────────────────────────────────
        // Rider activation
        // ────────────────────────────────────────────────────────

        private IEnumerator ActivateRiderAfterDelay()
        {
            Debug.Log($"RaceTrackManager: Rider activating in {riderActivateDelay}s...");
            yield return new WaitForSeconds(riderActivateDelay);

            if (phantomRider != null)
            {
                phantomRider.Activate();
                Debug.Log("RaceTrackManager: Phantom Rider activated — fight!");
            }
        }

        // ────────────────────────────────────────────────────────
        // Rider defeated
        // ────────────────────────────────────────────────────────

        private void HandleRiderDefeated()
        {
            if (riderDefeated) return;
            riderDefeated = true;

            Debug.Log("RaceTrackManager: Phantom Rider defeated — advancing to duel phase.");
            // TODO: transition to duel phase or load next scene
            // e.g. StartCoroutine(LoadDuelScene());
        }
    }
}
