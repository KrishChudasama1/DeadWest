using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance;

    [System.Serializable]
    public class SpawnEntry
    {
        public GameObject prefab;
        public int        count;
    }

    [System.Serializable]
    public class Wave
    {
        public List<SpawnEntry> enemies;
        public float            delayBeforeWave = 3f; // seconds before this wave spawns
    }

    [Header("Music")]
    public AudioClip normalMusic;
    public AudioClip waveMusic;
    public float fadeDuration = 1.5f; // how long the crossfade takes

    private AudioSource musicSource;
    private Coroutine fadeCoroutine;

    
    [Header("Waves")]
    public List<Wave> waves = new List<Wave>();

    [Header("Spawn Points")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Spawn Spread")]
    public float spawnJitterRadius = 0.5f;
    
    
    [Header("Settings")]
    public float checkInterval = 1f; // how often to check if wave is cleared

    [Header("Auto Start")]
    public bool autoStartOnLevelLoad = false;
    public float autoStartDelay = 2f;

    private int          currentWave      = -1;
    private bool         wavesStarted     = false;
    private bool         waveInProgress   = false;
    private List<GameObject> activeEnemies = new List<GameObject>();
    
    private void Awake()
    {
        instance = this;

        // Grab the AudioSource from the Main Camera
        musicSource = Camera.main.GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (autoStartOnLevelLoad)
            StartCoroutine(AutoStartWaves());
    }

    private IEnumerator AutoStartWaves()
    {
        if (autoStartDelay > 0f)
            yield return new WaitForSeconds(autoStartDelay);

        StartWaves();
    }
    

        public void StartWaves()
    {
        if (wavesStarted) return;
        wavesStarted = true;
        StartCoroutine(WaveSequence());
    }

    
    private IEnumerator WaveSequence()
    {
    
        for (int i = 0; i < waves.Count; i++)
        {
            yield return new WaitForSeconds(waves[i].delayBeforeWave);

            // ← switch to wave music when wave starts
            PlayMusic(waveMusic);

            SpawnWave(waves[i]);

            yield return StartCoroutine(WaitForWaveClear());

            Debug.Log($"Wave {i + 1} cleared!");
        }

        //switch back to normal music when all waves done
        PlayMusic(normalMusic);

        Debug.Log("All waves complete!");
        OnAllWavesComplete();
    
    }

    private void SpawnWave(Wave wave)
    {
        activeEnemies.Clear();

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning("WaveManager has no spawn points assigned.");
            return;
        }

        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);
        ShuffleSpawnPoints(availableSpawnPoints);
        int spawnPointIndex = 0;

        foreach (SpawnEntry entry in wave.enemies)
        {
            for (int i = 0; i < entry.count; i++)
            {
                if (spawnPointIndex >= availableSpawnPoints.Count)
                {
                    ShuffleSpawnPoints(availableSpawnPoints);
                    spawnPointIndex = 0;
                }

                Transform spawnPoint = availableSpawnPoints[spawnPointIndex++];
                Vector2 spawnPosition = spawnPoint.position;

                if (spawnJitterRadius > 0f)
                    spawnPosition += Random.insideUnitCircle * spawnJitterRadius;

                GameObject enemy = Instantiate(entry.prefab, spawnPosition, Quaternion.identity);
                activeEnemies.Add(enemy);
            }
        }

        Debug.Log($"Wave {currentWave + 1} spawned with {activeEnemies.Count} enemies.");
    }

    private void ShuffleSpawnPoints(List<Transform> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            int swapIndex = Random.Range(i, points.Count);
            (points[i], points[swapIndex]) = (points[swapIndex], points[i]);
        }
    }

    private IEnumerator WaitForWaveClear()
    {
        waveInProgress = true;

        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            // Remove any destroyed enemies from the list
            activeEnemies.RemoveAll(e => e == null);

            if (activeEnemies.Count == 0)
                break;
        }

        waveInProgress = false;
    }
    

    private void PlayMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;
        if (musicSource.clip == clip) return;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(CrossFade(clip));
    }

    private IEnumerator CrossFade(AudioClip newClip)
    {
        float startVolume = musicSource.volume;

        // Fade out current track
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.clip   = newClip;
        musicSource.loop   = true;
        musicSource.Play();

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, startVolume, elapsed / fadeDuration);
            yield return null;
        }

        musicSource.volume = startVolume;
        fadeCoroutine = null;
    }

    private void OnAllWavesComplete()
    {
        // Hook in whatever happens when all waves are done
        // e.g. unlock a door, spawn a reward, trigger dialogue
        Debug.Log("All waves defeated — trigger your end event here.");
    }

    public int  CurrentWave     => currentWave + 1;
    public bool WavesStarted    => wavesStarted;
    public bool WaveInProgress  => waveInProgress;
    public int  EnemiesRemaining => activeEnemies.FindAll(e => e != null).Count;
}
