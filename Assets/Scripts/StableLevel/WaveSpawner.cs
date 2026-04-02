using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Spawns configurable waves of enemies one after another.
/// Tracks enemy deaths and opens a gate when all waves are cleared.
/// </summary>
public class WaveSpawner : MonoBehaviour
{
    /// <summary>
    /// Represents a single wave of enemies with prefabs and spawn positions.
    /// </summary>
    [Serializable]
    public class Wave
    {
        [Tooltip("Enemy prefabs to spawn in this wave.")]
        public List<GameObject> enemyPrefabs = new List<GameObject>();

        [Tooltip("Spawn positions for each enemy. Must match enemyPrefabs count.")]
        public List<Transform> spawnPositions = new List<Transform>();
    }

    [Header("Wave Configuration")]
    [Tooltip("List of waves. Each wave spawns sequentially after the previous is cleared.")]
    [SerializeField] private List<Wave> waves = new List<Wave>();

    [Header("Gate")]
    [Tooltip("GameObject with a collider that blocks progress. Deactivated when all waves are cleared.")]
    [SerializeField] private GameObject gateObject;

    [Header("Events")]
    [Tooltip("Fired when all waves have been cleared.")]
    public UnityEvent OnAllWavesCleared;

    [Tooltip("Reference to the StableLevelManager to advance phases.")]
    [SerializeField] private StableLevelManager levelManager;

    private int _currentWaveIndex;
    private int _enemiesAliveInWave;
    private bool _spawning;

    /// <summary>
    /// Begins the wave spawning sequence. Call this when Phase 3 starts.
    /// </summary>
    public void StartWaves()
    {
        if (_spawning) return;
        _spawning = true;
        _currentWaveIndex = 0;
        StartCoroutine(SpawnWaveRoutine());
    }

    private IEnumerator SpawnWaveRoutine()
    {
        while (_currentWaveIndex < waves.Count)
        {
            Wave wave = waves[_currentWaveIndex];
            _enemiesAliveInWave = wave.enemyPrefabs.Count;

            Debug.Log($"[WaveSpawner] Starting wave {_currentWaveIndex + 1}/{waves.Count} with {_enemiesAliveInWave} enemies.");

            for (int i = 0; i < wave.enemyPrefabs.Count; i++)
            {
                Vector3 spawnPos = (i < wave.spawnPositions.Count && wave.spawnPositions[i] != null)
                    ? wave.spawnPositions[i].position
                    : transform.position + UnityEngine.Random.insideUnitSphere * 3f;
                spawnPos.z = 0f;

                GameObject enemy = Instantiate(wave.enemyPrefabs[i], spawnPos, Quaternion.identity);

                // Subscribe to death events from supported enemy types
                SubscribeToDeathEvent(enemy);
            }

            // Wait until all enemies in this wave are dead
            while (_enemiesAliveInWave > 0)
                yield return null;

            Debug.Log($"[WaveSpawner] Wave {_currentWaveIndex + 1} cleared!");
            _currentWaveIndex++;

            // Short delay between waves
            if (_currentWaveIndex < waves.Count)
                yield return new WaitForSeconds(1f);
        }

        OnWavesComplete();
    }

    /// <summary>
    /// Subscribes to the death event of any known enemy type on the spawned GameObject.
    /// </summary>
    /// <param name="enemy">The spawned enemy GameObject.</param>
    private void SubscribeToDeathEvent(GameObject enemy)
    {
        RanchHandEnemy ranchHand = enemy.GetComponent<RanchHandEnemy>();
        if (ranchHand != null)
        {
            ranchHand.OnDeath += OnEnemyDied;
            return;
        }

        // Fallback: use a DeathNotifier helper that fires when the GameObject is destroyed
        DeathNotifier notifier = enemy.GetComponent<DeathNotifier>();
        if (notifier == null)
            notifier = enemy.AddComponent<DeathNotifier>();
        notifier.OnDestroyed += OnEnemyDied;
    }

    private void OnEnemyDied()
    {
        _enemiesAliveInWave--;
        if (_enemiesAliveInWave < 0) _enemiesAliveInWave = 0;
    }

    private void OnWavesComplete()
    {
        _spawning = false;

        // Open the gate
        if (gateObject != null)
        {
            gateObject.SetActive(false);
            Debug.Log("[WaveSpawner] Gate opened!");
        }

        OnAllWavesCleared?.Invoke();

        // Advance to Phase 4
        if (levelManager != null)
            levelManager.AdvancePhase();
    }
}

/// <summary>
/// Helper component that fires an event when the GameObject is destroyed.
/// Used as a fallback for enemies without a built-in OnDeath event (e.g., GhostEnemy).
/// </summary>
public class DeathNotifier : MonoBehaviour
{
    /// <summary>
    /// Invoked when this GameObject is destroyed.
    /// </summary>
    public event Action OnDestroyed;

    private void OnDestroy()
    {
        OnDestroyed?.Invoke();
    }
}
