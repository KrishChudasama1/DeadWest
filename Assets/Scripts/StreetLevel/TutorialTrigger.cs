using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [TextArea(2, 3)]
    public string tutorialPrompt; // Type the hint in the Inspector
    
    [Header("Trigger Settings")]
    public bool showOnStart = false; // Check this for the "Welcome" movement tutorial
    
    [Tooltip("Type a unique name (e.g., 'MoveTut'). If filled, it saves to the game file and NEVER shows again.")]
    public string saveKey = ""; 

    private bool hasTriggered = false;

    private void Start()
    {
        // If this is set to show immediately, trigger it as soon as the scene loads
        if (showOnStart)
        {
            TryShowMessage();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If it's the player, we haven't triggered it locally, and it's NOT a start-up message
        if (other.CompareTag("Player") && !hasTriggered && !showOnStart)
        {
            TryShowMessage();
        }
    }

    private void TryShowMessage()
    {
        // Check the Save File. If this key is exactly 1, the player has already learned this.
        if (saveKey != "" && PlayerPrefs.GetInt(saveKey, 0) == 1)
        {
            return; // Stop here, don't show the message
        }

        if (TutorialUIManager.Instance != null)
        {
            TutorialUIManager.Instance.ShowMessage(tutorialPrompt);
            hasTriggered = true; // Mark as seen locally
            
            // Save it to the hard drive so it never shows again
            if (saveKey != "")
            {
                PlayerPrefs.SetInt(saveKey, 1);
                PlayerPrefs.Save();
            }
        }
    }
}