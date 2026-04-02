using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Abstract base class for quick draw duel minigames.
/// Each team member can extend this class to implement their own duel variant.
/// Provides lifecycle methods (OnDuelStart, OnPlayerShoot, OnEnemyShoot, OnDuelEnd)
/// and utility methods (StartDuel, EndDuel) for freezing movement and showing UI.
/// </summary>
public abstract class QuickDrawDuel : MonoBehaviour
{
    [Header("Duel UI")]
    [Tooltip("Canvas group overlay shown during the duel.")]
    [SerializeField] protected CanvasGroup duelOverlay;

    [Tooltip("TMP text element for duel status messages (e.g., DRAW!, YOU WIN!).")]
    [SerializeField] protected TMP_Text duelStatusText;

    [Header("Audio")]
    [Tooltip("Gunshot audio clip played when shooting.")]
    [SerializeField] protected AudioClip gunshotClip;

    [Header("Events")]
    [Tooltip("Fired when the player wins the duel.")]
    public UnityEvent OnPlayerWon;

    [Tooltip("Fired when the player loses the duel.")]
    public UnityEvent OnPlayerLost;

    protected AudioSource audioSource;
    protected PlayerMovement playerMovement;
    protected bool duelActive;

    /// <summary>
    /// Called when the duel sequence begins. Override to set up duel-specific state.
    /// </summary>
    protected abstract void OnDuelStart();

    /// <summary>
    /// Called when the player fires their weapon during the duel.
    /// Override to implement reaction timing logic.
    /// </summary>
    protected abstract void OnPlayerShoot();

    /// <summary>
    /// Called when the enemy fires their weapon during the duel.
    /// Override to implement enemy shooting behavior.
    /// </summary>
    protected abstract void OnEnemyShoot();

    /// <summary>
    /// Called when the duel resolves with a winner.
    /// Override to implement result-specific effects.
    /// </summary>
    /// <param name="playerWon">True if the player won, false if the enemy won.</param>
    protected abstract void OnDuelEnd(bool playerWon);

    /// <summary>
    /// Initializes common duel systems. Call base.Awake() in derived classes if overriding.
    /// </summary>
    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    /// <summary>
    /// Freezes player movement, shows the duel UI overlay, and begins the duel sequence.
    /// Call this from derived classes to initiate a duel.
    /// </summary>
    protected void StartDuel()
    {
        duelActive = true;

        // Find and freeze player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerMovement = playerObj.GetComponent<PlayerMovement>();

        if (playerMovement != null)
            playerMovement.SetMovementLocked(true);

        // Show overlay
        if (duelOverlay != null)
        {
            duelOverlay.alpha = 1f;
            duelOverlay.blocksRaycasts = false;
        }

        if (duelStatusText != null)
            duelStatusText.text = "";

        OnDuelStart();
    }

    /// <summary>
    /// Unfreezes player movement, hides the overlay, and fires the appropriate result event.
    /// Call this from derived classes when the duel is resolved.
    /// </summary>
    /// <param name="playerWon">True if the player won, false if the enemy won.</param>
    protected void EndDuel(bool playerWon)
    {
        duelActive = false;

        // Unfreeze player
        if (playerMovement != null)
            playerMovement.SetMovementLocked(false);

        OnDuelEnd(playerWon);

        if (playerWon)
            OnPlayerWon?.Invoke();
        else
            OnPlayerLost?.Invoke();
    }

    /// <summary>
    /// Plays the gunshot sound effect.
    /// </summary>
    protected void PlayGunshot()
    {
        if (gunshotClip != null && audioSource != null)
            audioSource.PlayOneShot(gunshotClip);
    }

    /// <summary>
    /// Hides the duel overlay by fading out the CanvasGroup.
    /// </summary>
    protected IEnumerator HideOverlayAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (duelOverlay != null)
        {
            float elapsed = 0f;
            float duration = 0.5f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                duelOverlay.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                yield return null;
            }
            duelOverlay.alpha = 0f;
        }
    }
}
