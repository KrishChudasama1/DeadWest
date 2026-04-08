using UnityEngine;

public class ChurchMusicManager : MonoBehaviour
{
    [Header("Music Tracks")]
    public AudioClip explorationMusic;
    public AudioClip bossMusic;
    public AudioClip victoryMusic;

    [Header("Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    public float fadeDuration = 2.5f;

    private AudioSource audioSource;
    private static ChurchMusicManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = musicVolume;
        PlayExplorationMusic();
    }

    public void PlayExplorationMusic()
    {
        if (explorationMusic == null) return;
        StartCoroutine(FadeToNewTrack(explorationMusic));
    }

    public void PlayBossMusic()
    {
        if (bossMusic == null) return;
        StartCoroutine(FadeToNewTrack(bossMusic));
    }

    public void PlayVictoryMusic()
    {
        if (victoryMusic == null) return;
        StartCoroutine(FadeToNewTrack(victoryMusic));
    }

    private System.Collections.IEnumerator FadeToNewTrack(AudioClip newClip)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();

        while (audioSource.volume < musicVolume)
        {
            audioSource.volume += musicVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        audioSource.volume = musicVolume;
    }

    public static void SwitchToBossMusic()
    {
        if (instance != null) instance.PlayBossMusic();
    }

    public static void SwitchToVictoryMusic()
    {
        if (instance != null) instance.PlayVictoryMusic();
    }

    public static void SwitchToExplorationMusic()
    {
        if (instance != null) instance.PlayExplorationMusic();
    }
}