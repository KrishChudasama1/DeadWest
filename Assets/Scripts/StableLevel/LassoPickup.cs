using UnityEngine;

public class LassoPickup : MonoBehaviour
{
    public static System.Action OnLassoPickedUp;
    [Header("Visual Feedback")]
    [Tooltip("Optional prompt shown near the pickup (leave null to skip).")]
    [SerializeField] private GameObject interactPrompt;

    [Header("Debug / Diagnostics")]
    [Tooltip("Radius used for overlap checks to detect the player if triggers aren't firing.")]
    [SerializeField] private float debugOverlapRadius = 0.5f;
    [Tooltip("Layer mask to use for the overlap check (set to player layer for faster checks).")]
    [SerializeField] private LayerMask debugOverlapMask = ~0;

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
    OnLassoPickedUp?.Invoke();
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

    // Diagnostic overlap polling: useful when OnTriggerEnter2D isn't firing due to collider/layer setup.
    private void Update()
    {
        if (_pickedUp) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, debugOverlapRadius, debugOverlapMask);
        foreach (var hit in hits)
        {
            if (hit == null) continue;
            if (hit.CompareTag("Player") || hit.GetComponentInParent<PlayerLasso>() != null)
            {
                Debug.Log($"LassoPickup: Overlap detected with '{hit.name}' (tag='{hit.tag}'). This indicates the player is physically overlapping the pickup. Triggers may be misconfigured.");
                // we don't auto-pickup here; just diagnostic log
                return;
            }
        }
    }
}