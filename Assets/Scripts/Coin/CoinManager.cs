using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager instance;

    public int coins = 0;
    public TextMeshProUGUI coinText;

    private const string COIN_KEY = "PlayerCoins";

    private void Awake()
    {
        // Singleton: survive scene loads, destroy duplicates
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Load saved coins
        coins = PlayerPrefs.GetInt(COIN_KEY, 0);

        // Re-bind UI whenever a new scene loads
        SceneManager.sceneLoaded += OnSceneLoaded;

        UpdateUI();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// After every scene load, find the CoinText UI in the new scene and refresh it.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Try to find a CoinText in the new scene's UI
        GameObject coinTextObj = GameObject.Find("CoinText");
        if (coinTextObj != null)
        {
            coinText = coinTextObj.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            coinText = null;
        }

        UpdateUI();
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        SaveCoins();
        UpdateUI();
    }

    public bool SpendCoins(int amount)
    {
        if (amount < 0)
            return false;

        if (coins < amount)
            return false;

        coins -= amount;
        SaveCoins();
        UpdateUI();
        return true;
    }

    public bool HasCoins(int amount)
    {
        if (amount < 0)
            return false;

        return coins >= amount;
    }

    /// <summary>
    /// Wipe all coins (called on player death).
    /// </summary>
    public void ResetCoins()
    {
        coins = 0;
        SaveCoins();
        UpdateUI();
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(COIN_KEY, coins);
        PlayerPrefs.Save();
    }

    void UpdateUI()
    {
        if (coinText != null)
        {
            coinText.text = "Coins: " + coins;
        }
    }
}