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
        public float            delayBeforeWave = 3f;
    }

    [Header("Music")]
    public AudioClip normalMusic;
    public AudioClip waveMusic;
    public float fadeDuration = 1.5f;

    private AudioSource musicSource;
    private Coroutine fadeCoroutine;

    
    [Header("Waves")]
    public List<Wave> waves = new List<Wave>();

    [Header("Spawn Points")]
    public List<Transform> spawnPoints = new List<Transform>();
    
    
    [Header("Settings")]
    public float checkInterval = 1f;

    private int          currentWave      = -1;
    private bool         wavesStarted     = false;
    private bool         waveInProgress   = false;
    private List<GameObject> activeEnemies = new List<GameObject>();
    
    private void Awake()
    {
        instance = this;

        musicSource = Camera.main.GetComponent<AudioSource>();
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
            currentWave = i;

            yield return new WaitForSeconds(waves[i].delayBeforeWave);

            PlayMusic(waveMusic);

            SpawnWave(waves[i]);

            yield return StartCoroutine(WaitForWaveClear());

            Debug.Log($"Wave {i + 1} cleared!");
        }

        PlayMusic(normalMusic);

        Debug.Log("All waves complete!");
        OnAllWavesComplete();
    }

    private void SpawnWave(Wave wave)
    {
        activeEnemies.Clear();

        foreach (SpawnEntry entry in wave.enemies)
        {
            for (int i = 0; i < entry.count; i++)
            {
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

                GameObject enemy = Instantiate(entry.prefab, spawnPoint.position, Quaternion.identity);
                activeEnemies.Add(enemy);
            }
        }

        Debug.Log($"Wave {currentWave + 1} spawned with {activeEnemies.Count} enemies.");
    }

    private IEnumerator WaitForWaveClear()
    {
        waveInProgress = true;

        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

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
        Debug.Log("All waves defeated - trigger your end event here.");
    }

    public int  CurrentWave     => currentWave + 1;
    public bool WavesStarted    => wavesStarted;
    public bool WaveInProgress  => waveInProgress;
    public int  EnemiesRemaining => activeEnemies.FindAll(e => e != null).Count;
}
