using UnityEngine;

public class ClueItem : MonoBehaviour
{
    private ClueManager manager;

    void Start()
    {
        // Find the ClueManager in the scene automatically
        manager = FindFirstObjectByType<ClueManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Make sure it's the Sheriff touching it
        if (other.CompareTag("Player"))
        {
            if (manager != null)
            {
                manager.CollectClue();
            }
            // Destroy the clue so it can't be collected twice
            Destroy(gameObject);
        }
    }
}