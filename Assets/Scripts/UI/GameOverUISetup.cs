using UnityEngine;
using UnityEngine.UI;

public class GameOverUISetup : MonoBehaviour
{
    [SerializeField] private Button respawnButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        if (respawnButton == null)
            respawnButton = transform.Find("RespawnButton")?.GetComponent<Button>();
        
        if (quitButton == null)
            quitButton = transform.Find("QuitButton")?.GetComponent<Button>();

        if (respawnButton != null)
            respawnButton.onClick.AddListener(() => GameOverManager.GetInstance().OnRespawnButtonClicked());
        
        if (quitButton != null)
            quitButton.onClick.AddListener(() => GameOverManager.GetInstance().OnQuitButtonClicked());
    }
}
