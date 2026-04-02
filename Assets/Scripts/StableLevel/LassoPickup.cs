using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// A lasso tool pickup that the player can collect during Phase 2 of the Stable Level.
/// Shows a UI prompt when the player is within range and adds the lasso to their inventory on pickup.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class LassoPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [Tooltip("Distance within which the pickup prompt appears.")]
    [SerializeField] private float promptRange = 1.5f;

    [Tooltip("Key to press to pick up the lasso.")]
    [SerializeField] private KeyCode pickupKey = KeyCode.E;

    [Header("UI")]
    [Tooltip("TMP text element for the pickup prompt. Should display '[E] Pick up Lasso'.")]
    [SerializeField] private TMP_Text promptText;

    [Header("Events")]
    [Tooltip("Reference to the StableLevelManager to advance phases on pickup.")]
    [SerializeField] private StableLevelManager levelManager;

    private Transform _player;
    private bool _collected;

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        if (promptText != null)
            promptText.gameObject.SetActive(false);

        // Ensure the collider is set to trigger
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
    }

    private void Update()
    {
        if (_collected || _player == null) return;

        float distance = Vector2.Distance(transform.position, _player.position);
        bool inRange = distance <= promptRange;

        if (promptText != null)
            promptText.gameObject.SetActive(inRange);

        if (inRange && Input.GetKeyDown(pickupKey))
            Collect();
    }

    /// <summary>
    /// Collects the lasso, hides the prompt, and advances the level phase.
    /// </summary>
    private void Collect()
    {
        _collected = true;

        if (promptText != null)
            promptText.gameObject.SetActive(false);

        // Mark lasso as collected in PlayerPrefs for persistence
        PlayerPrefs.SetInt("HasLasso", 1);
        PlayerPrefs.Save();

        Debug.Log("[LassoPickup] Lasso collected!");

        // Advance to Phase 3
        if (levelManager != null)
            levelManager.AdvancePhase();

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, promptRange);
    }
}
