using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [TextArea(2, 3)]
    public string tutorialPrompt; // Type the hint in the Inspector (e.g., "Use WASD to Move")
    
    private bool hasTriggered = false; // Ensures the player doesn't get spammed

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if it's the player and if we haven't shown this exact prompt yet
        if (other.CompareTag("Player") && !hasTriggered)
        {
            if (TutorialUIManager.Instance != null)
            {
                TutorialUIManager.Instance.ShowMessage(tutorialPrompt);
                hasTriggered = true; // Mark as seen
                
                // Optional: Destroy the trigger object to keep the scene clean
                // Destroy(gameObject, 1f); 
            }
        }
    }
}