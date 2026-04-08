using UnityEngine;
using TMPro;
using UnityEngine.Video;

public class PriestAltar : MonoBehaviour
{
    [Header("Priest Spawning")]
    public GameObject corruptedPriestPrefab;
    public Transform priestSpawnPoint;

    [Header("Interaction")]
    public float interactRange = 2f;
    public TextMeshProUGUI interactPrompt;
    public TextMeshProUGUI altarText;

    [Header("Video")]
    public GameObject videoScreen;
    public VideoPlayer videoPlayer;

    private Transform player;
    private bool priestSpawned = false;
    private bool priestDead = false;
    private GameObject spawnedPriest = null;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (interactPrompt != null)
            interactPrompt.gameObject.SetActive(false);

        if (altarText != null)
        {
            altarText.gameObject.SetActive(true);
            altarText.text = "Place down the relics";
        }

        if (videoScreen != null)
            videoScreen.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        bool inRange = dist <= interactRange;

        if (priestSpawned && spawnedPriest == null && !priestDead)
        {
            priestDead = true;
            if (altarText != null)
                altarText.text = "The curse has been weakened...";
        }

        if (interactPrompt != null)
        {
            interactPrompt.gameObject.SetActive(inRange);

            if (inRange)
            {
                if (!priestSpawned)
                    interactPrompt.text = "Press Y to place the relics";
                else if (priestDead)
                    interactPrompt.text = "Press Y to lift the curse";
                else
                    interactPrompt.text = "Defeat the Corrupted Priest first!";
            }
        }

        if (inRange && Input.GetKeyDown(KeyCode.Y))
        {
            if (!priestSpawned)
                SpawnPriest();
            else if (priestDead)
                ShowVictoryVideo();
        }
    }

    void SpawnPriest()
    {
        if (corruptedPriestPrefab == null || priestSpawnPoint == null)
        {
            Debug.LogWarning("Assign the priest prefab and spawn point!");
            return;
        }

        spawnedPriest = Instantiate(
            corruptedPriestPrefab,
            priestSpawnPoint.position,
            Quaternion.identity
        );

        priestSpawned = true;

        if (altarText != null)
            altarText.text = "Defeat the Corrupted Priest!";

        if (interactPrompt != null)
            interactPrompt.gameObject.SetActive(false);

        ChurchMusicManager.SwitchToBossMusic();

        Debug.Log("Corrupted Priest spawned!");
    }

    void ShowVictoryVideo()
    {
        if (altarText != null)
            altarText.gameObject.SetActive(false);

        if (interactPrompt != null)
            interactPrompt.gameObject.SetActive(false);

        GameObject playerCanvas = GameObject.Find("PlayerCanvas");
        if (playerCanvas != null)
            playerCanvas.SetActive(false);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            MonoBehaviour[] scripts = playerObj.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                if (script.GetType().Name == "Ammohud" ||
                    script.GetType().Name == "AmmoHud" ||
                    script.GetType().Name == "AmmoHUD")
                {
                    script.enabled = false;
                    break;
                }
            }

            PlayerMovement pm = playerObj.GetComponent<PlayerMovement>();
            if (pm != null) pm.enabled = false;

            PlayerShooting ps = playerObj.GetComponent<PlayerShooting>();
            if (ps != null) ps.enabled = false;
        }

        if (videoScreen != null)
            videoScreen.SetActive(true);

        if (videoPlayer != null)
        {
            videoPlayer.targetTexture.Release();
            videoPlayer.time = 0;
            videoPlayer.timeReference = VideoTimeReference.InternalTime;
            videoPlayer.isLooping = false;
            videoPlayer.Play();
            videoPlayer.loopPointReached += OnVideoFinished;
        }

        Time.timeScale = 1f;
        Debug.Log("Playing victory video!");
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        vp.Pause();
        vp.time = vp.length - 0.05;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}