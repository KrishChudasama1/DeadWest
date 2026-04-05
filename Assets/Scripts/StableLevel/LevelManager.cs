using UnityEngine;
using UnityEngine.SceneManagement;

namespace StableLevel
{
  
    public class LevelManager : MonoBehaviour
    {
        [Tooltip("Optional: assign the WaveSpawner in the scene. If left empty the manager will find the first WaveSpawner.")]
        public WaveSpawner waveSpawner;

        [Tooltip("Scene name that triggers spawning (exact match).")]
        public string targetSceneName = "horseStable";

        private bool hasStarted = false;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Start()
        {
            // In case this object exists already in the target scene
            if (SceneManager.GetActiveScene().name == targetSceneName)
                TryStartSpawner();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != targetSceneName) return;
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
            Debug.Log("LevelManager: All waves cleared.");
            // TODO: add level-complete behavior here 
        }
    }
}
