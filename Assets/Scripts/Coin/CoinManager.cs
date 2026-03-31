using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager instance;

    public int coins = 0;
    public TextMeshProUGUI coinText;

    private void Awake()
    {
        instance = this;
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateUI();
    }

    public bool SpendCoins(int amount)
    {
        if (amount < 0)
            return false;

        if (coins < amount)
            return false;

        coins -= amount;
        UpdateUI();
        return true;
    }

    public bool HasCoins(int amount)
    {
        if (amount < 0)
            return false;

        return coins >= amount;
    }

    void UpdateUI()
    {
        if (coinText != null)
        {
            coinText.text = "Coins: " + coins;
        }
    }
}