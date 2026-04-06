using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialUIManager : MonoBehaviour
{
    public static TutorialUIManager Instance;
    
    [Header("UI References")]
    public TextMeshProUGUI tutorialText; 
    public float displayTime = 4f; 

    void Awake() 
    { 
        if (Instance == null) Instance = this; 
    }

    void Start()
    {
        
        if (tutorialText != null) tutorialText.gameObject.SetActive(false);
    }

    public void ShowMessage(string message)
    {
        if (tutorialText == null) return;
        
        StopAllCoroutines(); 
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