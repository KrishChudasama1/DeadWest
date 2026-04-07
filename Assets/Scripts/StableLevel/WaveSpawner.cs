using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StableLevel
{
    public class WaveSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class Wave
        {
            public GameObject enemyPrefab;
            public int count = 3;
            public float timeBetweenSpawns = 0.5f;
            public float delayBeforeWave = 0f;
        }

        public Wave[] waves = new Wave[0];

        [Header("Spawn Points")]
        public Transform[] spawnPoints;

        // Tracks living enemies spawned by this spawner
        private List<GameObject> livingEnemies = new List<GameObject>();

        // Fired when the last enemy of the last wave dies
        public event Action OnAllWavesCleared;

    // Public flag other systems can query to know if spawning finished
    public bool AllWavesCleared { get; private set; } = false;

        private Coroutine spawnRoutine;

        
        public void StartSpawning()
        {
            if (spawnRoutine == null)
                spawnRoutine = StartCoroutine(SpawnRoutine());
        }

        
        public void StopSpawning()
        {
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }
        }

        private IEnumerator SpawnRoutine()
        {
            if (waves == null || waves.Length == 0)
            {
                Debug.LogWarning("WaveSpawner: No waves configured.");
                yield break;
            }

            if (spawnPoints == null || spawnPoints.Length == 0)
                Debug.LogWarning("WaveSpawner: No spawn points assigned; enemies will spawn at spawner position.");

            for (int w = 0; w < waves.Length; w++)
            {
                Wave wave = waves[w];
                if (wave == null || wave.enemyPrefab == null)
                {
                    Debug.LogWarning($"WaveSpawner: skipping invalid wave index {w} (null or missing prefab)");
                    continue;
                }

                if (wave.delayBeforeWave > 0f)
                    yield return new WaitForSeconds(wave.delayBeforeWave);

                // Spawn the wave
                for (int i = 0; i < Mathf.Max(0, wave.count); i++)
                {
                    Transform spawnPoint = (spawnPoints != null && spawnPoints.Length > 0)
                        ? spawnPoints[i % spawnPoints.Length]
                        : transform;

                    Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
                    GameObject enemy = Instantiate(wave.enemyPrefab, spawnPos, Quaternion.identity);
                    livingEnemies.Add(enemy);

                    if (wave.timeBetweenSpawns > 0f)
                        yield return new WaitForSeconds(wave.timeBetweenSpawns);
                    else
                        yield return null;
                }

                // Wait until all enemies from this wave are dead before starting the next wave
                while (true)
                {
                    livingEnemies.RemoveAll(e => e == null);
                    if (livingEnemies.Count == 0)
                        break;
                    yield return new WaitForSeconds(0.25f);
                }
            }

            // Wait until all spawned enemies are dead (clean up nulls periodically)
            while (true)
            {
                livingEnemies.RemoveAll(e => e == null);
                if (livingEnemies.Count == 0)
                    break;
                yield return new WaitForSeconds(0.25f);
            }

            // Fire event to notify listeners that all waves have been cleared
            AllWavesCleared = true;
            OnAllWavesCleared?.Invoke();
            spawnRoutine = null;
        }
    }
}
