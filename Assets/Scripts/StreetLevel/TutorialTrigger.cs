using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [TextArea(2, 3)]
    public string tutorialPrompt; 
    
    [Header("Trigger Settings")]
    public bool showOnStart = false; 
    
    [Tooltip("Type a unique name (e.g., 'MoveTut'). If filled, it saves to the game file and NEVER shows again.")]
    public string saveKey = ""; 

    private bool hasTriggered = false;

    private void Start()
    {
        
        if (showOnStart)
        {
            TryShowMessage();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
            
        if (other.CompareTag("Player") && !hasTriggered && !showOnStart)
        {
            TryShowMessage();
        }
    }

    private void TryShowMessage()
    {
        
        if (saveKey != "" && PlayerPrefs.GetInt(saveKey, 0) == 1)
        {
            return; 
        }

        if (TutorialUIManager.Instance != null)
        {
            TutorialUIManager.Instance.ShowMessage(tutorialPrompt);
            hasTriggered = true; 
            
            
            if (saveKey != "")
            {
                PlayerPrefs.SetInt(saveKey, 1);
                PlayerPrefs.Save();
            }
        }
    }
}