using UnityEngine;
using UnityEngine.UI;

///
/// Helper script to automatically configure Game Over UI buttons.
/// Attach this script to your GameOverPanel for easier setup.
///
public class GameOverUISetup : MonoBehaviour
{
    [SerializeField] private Button respawnButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        // Find buttons by name if not assigned in inspector
        if (respawnButton == null)
            respawnButton = transform.Find("RespawnButton")?.GetComponent<Button>();
        
        if (quitButton == null)
            quitButton = transform.Find("QuitButton")?.GetComponent<Button>();

        // Connect buttons to GameOverManager methods
        if (respawnButton != null)
            respawnButton.onClick.AddListener(() => GameOverManager.GetInstance().OnRespawnButtonClicked());
        
        if (quitButton != null)
            quitButton.onClick.AddListener(() => GameOverManager.GetInstance().OnQuitButtonClicked());
    }
}
