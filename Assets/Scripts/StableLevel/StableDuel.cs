using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Concrete implementation of QuickDrawDuel for the Stable Level.
/// Shows "DRAW!" after a random delay, and the player must click within a 0.5s reaction window.
/// Shooting early or too late results in a loss.
/// </summary>
public class StableDuel : QuickDrawDuel
{
    [Header("Stable Duel Settings")]
    [Tooltip("Minimum delay before DRAW! appears (seconds).")]
    [SerializeField] private float minDrawDelay = 1f;

    [Tooltip("Maximum delay before DRAW! appears (seconds).")]
    [SerializeField] private float maxDrawDelay = 3f;

    [Tooltip("Time window after DRAW! in which the player must shoot (seconds).")]
    [SerializeField] private float reactionWindow = 0.5f;

    [Tooltip("Reference to the StableLevelManager to advance phases.")]
    [SerializeField] private StableLevelManager levelManager;

    private bool _waitingForDraw;
    private bool _drawShown;
    private bool _playerShot;
    private bool _duelResolved;

    /// <summary>
    /// Begins the stable duel sequence. Call this to start the minigame.
    /// </summary>
    public void BeginDuel()
    {
        StartDuel();
    }

    /// <summary>
    /// Called when the duel starts. Initiates the draw delay countdown.
    /// </summary>
    protected override void OnDuelStart()
    {
        _waitingForDraw = true;
        _drawShown = false;
        _playerShot = false;
        _duelResolved = false;

        if (duelStatusText != null)
            duelStatusText.text = "Get ready...";

        StartCoroutine(DuelSequence());
    }

    /// <summary>
    /// Called when the player fires. Evaluates whether the shot was in time.
    /// </summary>
    protected override void OnPlayerShoot()
    {
        PlayGunshot();

        if (!_drawShown)
        {
            // Player shot too early
            ResolveDuel(false, "TOO EARLY!");
        }
        // If draw was shown, the DuelSequence coroutine handles the timing check
    }

    /// <summary>
    /// Called when the enemy fires. In the stable duel, this happens when the player is too slow.
    /// </summary>
    protected override void OnEnemyShoot()
    {
        PlayGunshot();
    }

    /// <summary>
    /// Called when the duel ends. Displays the result and advances the phase if won.
    /// </summary>
    /// <param name="playerWon">True if the player won.</param>
    protected override void OnDuelEnd(bool playerWon)
    {
        if (playerWon)
        {
            Debug.Log("[StableDuel] Player won the duel!");
            if (levelManager != null)
                levelManager.AdvancePhase();
        }
        else
        {
            Debug.Log("[StableDuel] Player lost the duel.");
        }

        StartCoroutine(HideOverlayAfterDelay(2f));
    }

    private void Update()
    {
        if (!duelActive || _duelResolved) return;

        // Detect player shooting during the duel
        if (Input.GetMouseButtonDown(0))
        {
            _playerShot = true;

            if (!_drawShown)
            {
                // Shot before DRAW! appeared
                OnPlayerShoot();
            }
        }
    }

    private IEnumerator DuelSequence()
    {
        // Random delay before showing DRAW!
        float delay = Random.Range(minDrawDelay, maxDrawDelay);
        float elapsed = 0f;

        while (elapsed < delay)
        {
            elapsed += Time.deltaTime;

            // Check if player shot early during the wait
            if (_duelResolved) yield break;

            yield return null;
        }

        if (_duelResolved) yield break;

        // Show DRAW!
        _drawShown = true;
        _playerShot = false; // Reset so we check only post-draw clicks

        if (duelStatusText != null)
        {
            duelStatusText.text = "DRAW!";
            duelStatusText.fontSize = 72;
        }

        // Wait for reaction window
        float reactionElapsed = 0f;
        while (reactionElapsed < reactionWindow)
        {
            reactionElapsed += Time.deltaTime;

            if (Input.GetMouseButtonDown(0) && !_duelResolved)
            {
                // Player shot in time!
                OnPlayerShoot();
                ResolveDuel(true, "YOU WIN!");
                yield break;
            }

            yield return null;
        }

        // Player was too slow
        if (!_duelResolved)
        {
            OnEnemyShoot();
            ResolveDuel(false, "TOO SLOW!");
        }
    }

    private void ResolveDuel(bool playerWon, string message)
    {
        if (_duelResolved) return;
        _duelResolved = true;

        if (duelStatusText != null)
        {
            duelStatusText.text = message;
            duelStatusText.fontSize = 64;
        }

        EndDuel(playerWon);
    }
}
