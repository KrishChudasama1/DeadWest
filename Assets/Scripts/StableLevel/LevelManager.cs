using UnityEngine;
using UnityEngine.SceneManagement;

namespace StableLevel
{
  
    public class LevelManager : MonoBehaviour
    {
        [Tooltip("Optional: assign the WaveSpawner in the scene. If left empty the manager will find the first WaveSpawner.")]
        public WaveSpawner waveSpawner;

        [Tooltip("Optional: assign the RaceTrackGate in the scene. If left empty the manager will find one.")]
        public RaceTrackGate raceTrackGate;

        [Tooltip("Scene name that triggers spawning (exact match).")]
        public string targetSceneName = "horseStable";

        private bool hasStarted = false;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        // Subscribe to lasso pickup so we can start spawning after the player picks it up
        LassoPickup.OnLassoPickedUp += OnLassoPickedUp;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        LassoPickup.OnLassoPickedUp -= OnLassoPickedUp;
        }

        private void Start()
        {
            // In case this object exists already in the target scene
                if (string.Equals(SceneManager.GetActiveScene().name, targetSceneName, System.StringComparison.OrdinalIgnoreCase))
                    // Don't auto-start here; wait for lasso pickup. Keep TryStartSpawner as a fallback.
                    Debug.Log("LevelManager: loaded target scene; waiting for lasso pickup to start waves.");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!string.Equals(scene.name, targetSceneName, System.StringComparison.OrdinalIgnoreCase)) return;
                // Don't immediately start waves when the scene loads; wait for lasso pickup event.
                Debug.Log("LevelManager: target scene loaded; waiting for lasso pickup to start waves.");
        }

            private void OnLassoPickedUp()
            {
                // Only respond if we're in the target scene (case-insensitive)
                if (!string.Equals(SceneManager.GetActiveScene().name, targetSceneName, System.StringComparison.OrdinalIgnoreCase)) return;
                if (hasStarted) return;

                Debug.Log("LevelManager: Lasso picked up — starting waves in 10 seconds...");
                StartCoroutine(StartSpawnerAfterDelay(10f));
            }

            private System.Collections.IEnumerator StartSpawnerAfterDelay(float delay)
            {
                yield return new WaitForSeconds(delay);
                TryStartSpawner();
            }

        private void TryStartSpawner()
        {
            if (hasStarted) return;

            if (waveSpawner == null)
            {
                waveSpawner = FindObjectOfType<WaveSpawner>();
            }

            if (waveSpawner == null)
            {
                Debug.LogWarning($"LevelManager: no WaveSpawner found in scene '{targetSceneName}'. Add one or assign it in the inspector.");
                return;
            }

            waveSpawner.OnAllWavesCleared += HandleAllWavesCleared;
            waveSpawner.StartSpawning();
            hasStarted = true;
            Debug.Log($"LevelManager: started spawning waves for scene '{targetSceneName}'.");
        }

        private void HandleAllWavesCleared()
        {
            Debug.Log("LevelManager: All waves cleared — unlocking race track gate.");
            UnlockGate();
        }

        private void UnlockGate()
        {
            if (raceTrackGate == null)
                raceTrackGate = FindObjectOfType<RaceTrackGate>(true);

            if (raceTrackGate == null)
            {
                Debug.LogWarning("LevelManager: no RaceTrackGate found in scene. Cannot open gate.");
                return;
            }

            raceTrackGate.Unlock();
            Debug.Log("LevelManager: Race track gate unlocked — player can walk through to fight the Phantom Rider.");
        }
    }
}
