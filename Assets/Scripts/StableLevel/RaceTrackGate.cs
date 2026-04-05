using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StableLevel
{
    /// <summary>
    /// A gate that appears after all enemy waves are cleared.
    /// When the player walks into it, it fades to black and loads the race-track scene
    /// where the Phantom Rider boss fight takes place.
    /// 
    /// Setup:
    ///   1. Create an empty GameObject in HorseStable named "RaceTrackGate".
    ///   2. Add a BoxCollider2D (Is Trigger = true) sized like a doorway.
    ///   3. Optionally add a SpriteRenderer with a gate/portal sprite.
    ///   4. Attach this script.  Set raceTrackSceneName to your scene's exact name.
    ///   5. The gate starts locked (invisible + trigger disabled).
    ///      LevelManager calls Unlock() once all waves are cleared.
    /// </summary>
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

        // ── Private ─────────────────────────────────────────────
        private Collider2D gateTrigger;
        private SpriteRenderer sr;
        private bool isUnlocked = false;
        private bool isLoading = false;

        // ── Static event so LevelManager can listen ─────────────
        /// <summary>Fired right before the scene transition begins.</summary>
        public static event System.Action OnGateEntered;

        // ────────────────────────────────────────────────────────
        private void Awake()
        {
            gateTrigger = GetComponent<Collider2D>();
            sr = GetComponent<SpriteRenderer>();

            // Start hidden & non-interactive
            Lock();
        }

        // ────────────────────────────────────────────────────────
        // Public API
        // ────────────────────────────────────────────────────────

        /// <summary>
        /// Makes the gate visible and interactable.
        /// Called by LevelManager when all waves are cleared.
        /// </summary>
        public void Unlock()
        {
            isUnlocked = true;

            if (gateTrigger != null)
                gateTrigger.enabled = true;

            if (sr != null)
            {
                sr.enabled = true;
                // Optional: gentle pulse could be added here via coroutine
            }

            if (promptUI != null)
                promptUI.SetActive(true);

            Debug.Log("RaceTrackGate: Gate unlocked — walk into it to enter the race track!");
        }

        /// <summary>Hides and disables the gate.</summary>
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

        // ────────────────────────────────────────────────────────
        // Trigger
        // ────────────────────────────────────────────────────────

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
