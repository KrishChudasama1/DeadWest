using UnityEngine;

public class LassoPickup : MonoBehaviour
{
    [Header("Visual Feedback")]
    [Tooltip("Optional prompt shown near the pickup (leave null to skip).")]
    [SerializeField] private GameObject interactPrompt;

    private bool _pickedUp = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_pickedUp) return;
        if (!other.CompareTag("Player")) return;

        PlayerLasso lasso = other.GetComponent<PlayerLasso>();
        if (lasso == null)
        {
            Debug.LogWarning("[LassoPickup] Player is missing a PlayerLasso component.");
            return;
        }

        _pickedUp = true;
        lasso.UnlockLasso();
        Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (interactPrompt != null && other.CompareTag("Player"))
            interactPrompt.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (interactPrompt != null && other.CompareTag("Player"))
            interactPrompt.SetActive(false);
    }
}