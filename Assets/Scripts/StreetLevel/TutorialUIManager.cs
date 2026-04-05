using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialUIManager : MonoBehaviour
{
    public static TutorialUIManager Instance;
    
    [Header("UI References")]
    public TextMeshProUGUI tutorialText; // Drag your on-screen text here
    public float displayTime = 4f; // How long the hint stays on screen

    void Awake() 
    { 
        if (Instance == null) Instance = this; 
    }

    void Start()
    {
        // Ensure it starts hidden
        if (tutorialText != null) tutorialText.gameObject.SetActive(false);
    }

    public void ShowMessage(string message)
    {
        if (tutorialText == null) return;
        
        StopAllCoroutines(); // Stops any currently fading text
        StartCoroutine(DisplayRoutine(message));
    }

    private IEnumerator DisplayRoutine(string message)
    {
        tutorialText.text = message;
        tutorialText.gameObject.SetActive(true);
        
        yield return new WaitForSeconds(displayTime);
        
        tutorialText.gameObject.SetActive(false);
    }
}