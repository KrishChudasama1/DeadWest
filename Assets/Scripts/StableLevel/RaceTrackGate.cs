using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StableLevel
{
    
    public class RaceTrackGate : MonoBehaviour
    {
        [Header("Target Scene")]
        [Tooltip("Exact name of the race-track scene (must be in Build Settings).")]
        [SerializeField] private string raceTrackSceneName = "RaceTrack";

        [Header("Fade")]
        [SerializeField] private float fadeDuration = 1f;

        [Header("Visual Hint (optional)")]
        [Tooltip("If assigned, this prompt text object is shown when the gate unlocks.")]
        [SerializeField] private GameObject promptUI;

       
        private Collider2D gateTrigger;
        private SpriteRenderer sr;
        private bool isUnlocked = false;
        private bool isLoading = false;

       
        public static event System.Action OnGateEntered;

        
        private void Awake()
        {
            gateTrigger = GetComponent<Collider2D>();
            sr = GetComponent<SpriteRenderer>();

            Lock();
        }

        
        public void Unlock()
        {
            isUnlocked = true;

            if (gateTrigger != null)
                gateTrigger.enabled = true;

            if (sr != null)
            {
                sr.enabled = true;
                
            }

            if (promptUI != null)
                promptUI.SetActive(true);

            Debug.Log("RaceTrackGate: Gate unlocked — walk into it to enter the race track!");
        }

        
        public void Lock()
        {
            isUnlocked = false;

            if (gateTrigger != null)
                gateTrigger.enabled = false;

            if (sr != null)
                sr.enabled = false;

            if (promptUI != null)
                promptUI.SetActive(false);
        }



        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isUnlocked || isLoading) return;
            if (!other.CompareTag("Player")) return;

            isLoading = true;

            // Lock player movement during transition
            PlayerMovement pm = other.GetComponent<PlayerMovement>();
            if (pm != null)
                pm.SetMovementLocked(true);

            OnGateEntered?.Invoke();
            StartCoroutine(TransitionToRaceTrack());
        }

        private IEnumerator TransitionToRaceTrack()
        {
            // Fade to black using the persistent ScreenFader
            if (ScreenFader.Instance != null)
                yield return StartCoroutine(ScreenFader.Instance.FadeOut(fadeDuration));

            Debug.Log($"RaceTrackGate: Loading scene '{raceTrackSceneName}'...");
            SceneManager.LoadSceneAsync(raceTrackSceneName);
        }
    }
}
