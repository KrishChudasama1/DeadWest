using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PriestAltar : MonoBehaviour
{
    [Header("Priest Spawning")]
    public GameObject corruptedPriestPrefab;
    public Transform priestSpawnPoint;

    [Header("Interaction")]
    public float interactRange = 2f;
    public TextMeshProUGUI interactPrompt;
    public TextMeshProUGUI altarText;

    [Header("Victory Screen")]
    public GameObject victoryScreen;
    public TextMeshProUGUI victoryText;

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

        // Hide victory screen at start
        if (victoryScreen != null)
            victoryScreen.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);
        bool inRange = dist <= interactRange;

        // Check if the priest has been killed
        if (priestSpawned && spawnedPriest == null && !priestDead)
        {
            priestDead = true;
            if (altarText != null)
                altarText.text = "The curse has been weakened...";
        }

        // Show correct prompt depending on state
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

        // Handle Y press
        if (inRange && Input.GetKeyDown(KeyCode.Y))
        {
            if (!priestSpawned)
                SpawnPriest();
            else if (priestDead)
                ShowVictoryScreen();
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

        Debug.Log("Corrupted Priest spawned!");
    }

    void ShowVictoryScreen()
    {
        if (victoryScreen != null)
            victoryScreen.SetActive(true);

        if (victoryText != null)
            victoryText.text = "Congratulations, you have lifted the curse";

        if (altarText != null)
            altarText.gameObject.SetActive(false);

        if (interactPrompt != null)
            interactPrompt.gameObject.SetActive(false);

        // Freeze the game
        Time.timeScale = 0f;

        Debug.Log("Victory!");
    }
}