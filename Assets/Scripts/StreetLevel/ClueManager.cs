using UnityEngine;
using TMPro; // This is Unity's text system!

public class ClueManager : MonoBehaviour
{
    [Header("Clue Settings")]
    public int totalCluesRequired = 3;
    private int cluesCollected = 0;

    [Header("UI Settings")]
    [Tooltip("Drag the UI Image that has the ImagePopup script here")]
    public ImagePopup relicPopupScript;
    
    [Tooltip("Drag your new Objective Text object here")]
    public TextMeshProUGUI objectiveText;

    [Header("Reward Settings")]
    public int newProgressLevel = 1;

    void Start()
    {
        // Check the permanent save file
        int savedProgress = PlayerPrefs.GetInt("GameProgress", 0);

        // If progress is 1 or higher, the skulls were already found!
        if (savedProgress >= newProgressLevel)
        {
            cluesCollected = totalCluesRequired; // Set count to 3 so the math works
            
            // OPTIONAL: Find all skulls in the scene and hide them so they don't reappear
            ClueItem[] skullsInScene = FindObjectsOfType<ClueItem>();
            foreach (ClueItem skull in skullsInScene)
            {
                skull.gameObject.SetActive(false);
            }
        }

        // Now update the text (it will either show 0/3 or "Saloon Unlocked!")
        UpdateObjectiveText();
    }

    public void CollectClue()
    {
        cluesCollected++;
        Debug.Log("Clue collected! Total: " + cluesCollected + "/" + totalCluesRequired);

        // Update the screen text every time we grab a skull
        UpdateObjectiveText();

        if (cluesCollected >= totalCluesRequired)
        {
            UnlockRelic();
        }
    }

    private void UpdateObjectiveText()
    {
        // Safety check to make sure we actually linked the text object
        if (objectiveText != null)
        {
            if (cluesCollected < totalCluesRequired)
            {
                objectiveText.text = "Objective: Find 3 Skulls to unlock the Saloon (" + cluesCollected + "/" + totalCluesRequired + ")";
            }
            else
            {
                // What it says when they win!
                objectiveText.text = "Objective: Saloon Unlocked!";
            }
        }
    }

    private void UnlockRelic()
    {
        if (relicPopupScript != null)
        {
            relicPopupScript.ShowImage();
        }

        int currentProgress = PlayerPrefs.GetInt("GameProgress", 0);
        if (currentProgress < newProgressLevel)
        {
            PlayerPrefs.SetInt("GameProgress", newProgressLevel);
            PlayerPrefs.Save();
            Debug.Log("Game Progress updated to level: " + newProgressLevel);
        }
    }
}