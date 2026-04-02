using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Handles the Phase 6 reward sequence of the Stable Level.
/// Spawns a relic pickup, applies a permanent speed boost, saves progress,
/// and returns to the main hub scene.
/// </summary>
public class StableReward : MonoBehaviour
{
    [Header("Relic")]
    [Tooltip("The relic pickup GameObject to activate. Should have a collider set to IsTrigger.")]
    [SerializeField] private GameObject relicPickupObject;

    [Tooltip("Fixed position where the relic spawns.")]
    [SerializeField] private Vector3 relicSpawnPosition = Vector3.zero;

    [Header("Speed Boost")]
    [Tooltip("Multiplier applied to the player's base move speed.")]
    [SerializeField] private float speedMultiplier = 1.5f;

    [Header("UI")]
    [Tooltip("TMP text element to display the reward message.")]
    [SerializeField] private TMP_Text rewardMessageText;

    [Header("Scene")]
    [Tooltip("Name of the main hub scene to return to after collecting the reward.")]
    [SerializeField] private string mainHubSceneName = "MainScene";

    [Tooltip("Duration to show the reward message before transitioning.")]
    [SerializeField] private float messageDisplayTime = 3f;

    private bool _collected;

    /// <summary>
    /// Activates the relic pickup at the configured spawn position.
    /// Call this when Phase 6 begins.
    /// </summary>
    public void SpawnRelic()
    {
        if (relicPickupObject != null)
        {
            relicPickupObject.transform.position = relicSpawnPosition;
            relicPickupObject.SetActive(true);
        }

        if (rewardMessageText != null)
            rewardMessageText.gameObject.SetActive(false);

        Debug.Log("[StableReward] Relic spawned at " + relicSpawnPosition);
    }

    /// <summary>
    /// Called when the player picks up the relic via trigger collision.
    /// Attach this to the relic pickup object's trigger.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_collected) return;
        if (!other.CompareTag("Player")) return;

        CollectRelic(other.gameObject);
    }

    /// <summary>
    /// Collects the relic, applies the speed boost, saves progress, and transitions to hub.
    /// </summary>
    /// <param name="playerObj">The player GameObject.</param>
    private void CollectRelic(GameObject playerObj)
    {
        _collected = true;

        // Apply speed boost
        PlayerMovement movement = playerObj.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.moveSpeed *= speedMultiplier;
            Debug.Log($"[StableReward] Speed boosted to {movement.moveSpeed}");
        }

        // Save relic collection
        PlayerPrefs.SetInt("StableRelicCollected", 1);
        PlayerPrefs.Save();

        // Hide the relic object
        if (relicPickupObject != null)
            relicPickupObject.SetActive(false);

        // Show reward message and transition
        StartCoroutine(ShowRewardAndTransition());
    }

    private IEnumerator ShowRewardAndTransition()
    {
        // Show message
        if (rewardMessageText != null)
        {
            rewardMessageText.gameObject.SetActive(true);
            rewardMessageText.text = "Speed Boots Acquired! Movement speed increased!";
        }

        yield return new WaitForSeconds(messageDisplayTime);

        // Fade and load hub scene
        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOut(0.5f);

        SceneManager.LoadScene(mainHubSceneName);
    }
}
