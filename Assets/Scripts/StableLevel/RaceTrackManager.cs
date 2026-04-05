using System.Collections;
using UnityEngine;

namespace StableLevel
{
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

      
        private bool riderDefeated = false;


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

       

        private void HandleRiderDefeated()
        {
            if (riderDefeated) return;
            riderDefeated = true;

            Debug.Log("RaceTrackManager: Phantom Rider defeated — advancing to duel phase.");
            // TODO: transition to duel phase or load next scene
            
        }
    }
}
