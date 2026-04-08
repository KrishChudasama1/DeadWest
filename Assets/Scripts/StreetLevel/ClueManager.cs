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
        int savedProgress = PlayerPrefs.GetInt("GameProgress", 0);

        // If progress is 1 or higher, the skulls were already found in a past session.
        if (savedProgress >= newProgressLevel)
        {
            cluesCollected = totalCluesRequired;

            // 1. Hide the physical skulls so they aren't in the dirt.
            ClueItem[] skullsInScene = FindObjectsOfType<ClueItem>();
            foreach (ClueItem skull in skullsInScene)
            {
                skull.gameObject.SetActive(false);
            }

            // 2. SILENCE THE UI: If the task was already done before we loaded this scene,
            // we hide the text object immediately and STOP the script.
            if (objectiveText != null)
            {
                objectiveText.gameObject.SetActive(false);
            }
            
            return; // This "return" is the magic part—it prevents UpdateObjectiveText from ever running.
        }

        // Only run this if the player actually still needs to find the skulls.
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
        if (objectiveText == null) return;

        if (cluesCollected < totalCluesRequired)
        {
            objectiveText.text = "Objective: Find 3 Skulls to unlock the Saloon (" + cluesCollected + "/" + totalCluesRequired + ")";
            objectiveText.gameObject.SetActive(true);
        }
        else
        {
            objectiveText.text = "Objective: Saloon Unlocked!";
            
            // FIX: This tells Unity to wait 5 seconds and then run the Hide function
            Invoke("HideObjectiveText", 5f);
        }
    }

	// Helper function to turn off the text
    private void HideObjectiveText()
    {
        if (objectiveText != null)
        {
            objectiveText.gameObject.SetActive(false);
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