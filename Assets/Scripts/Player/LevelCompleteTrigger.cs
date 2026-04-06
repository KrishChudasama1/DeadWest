using UnityEngine;

public class LevelCompleteTrigger : MonoBehaviour
{
    [Header("Level Completion")]
    [Tooltip("What should the player's progress update to when they touch this? (e.g., set to 1 to unlock Church)")]
    public int newProgressLevel = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            int currentProgress = PlayerPrefs.GetInt("GameProgress", 0);

            
            if (newProgressLevel > currentProgress)
            {
                PlayerPrefs.SetInt("GameProgress", newProgressLevel);
                PlayerPrefs.Save(); 
                
                Debug.Log($"LEVEL COMPLETE! Game Progress is now: {newProgressLevel}");
            }
            
            
        }
    }
}