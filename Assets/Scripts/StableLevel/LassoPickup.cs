using UnityEngine;

public class LassoPickup : MonoBehaviour
{
    public static System.Action OnLassoPickedUp;
    [Header("Visual Feedback")]
    [Tooltip("Optional prompt shown near the pickup (leave null to skip).")]
    [SerializeField] private GameObject interactPrompt;

    private bool _pickedUp = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_pickedUp) return;
        if (!other.CompareTag("Player")) return;

        Debug.Log("[LassoPickup] Player entered trigger — picking up lasso.");
        PickUp();
    }

    private void Update()
    {
        if (_pickedUp) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f);
        foreach (var hit in hits)
        {
            if (hit == null) continue;
            if (hit.CompareTag("Player"))
            {
                Debug.Log("[LassoPickup] Overlap fallback — picking up lasso.");
                PickUp();
                return;
            }
        }
    }

    private void PickUp()
    {
        _pickedUp = true;
        OnLassoPickedUp?.Invoke();
        Debug.Log("[LassoPickup] Lasso picked up, event fired.");
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