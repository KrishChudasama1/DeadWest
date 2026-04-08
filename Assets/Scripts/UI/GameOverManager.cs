using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    
    private static GameOverManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static GameOverManager GetInstance()
    {
        return instance;
    }

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public static void ShowGameOver()
    {
        if (instance != null && instance.gameOverPanel != null)
        {
            instance.gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void OnRespawnButtonClicked()
    {
        Time.timeScale = 1f;
        PlayerHealth.PrepareForRespawn();
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void OnQuitButtonClicked()
    {
        Time.timeScale = 1f;
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
