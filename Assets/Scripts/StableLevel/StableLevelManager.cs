using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Central manager for the Stable Level. Controls all 6 phases and coordinates
/// phase transitions with fade effects and UnityEvents for designer hookups.
/// </summary>
public class StableLevelManager : MonoBehaviour
{
    /// <summary>
    /// Enum representing the 6 phases of the Stable Level.
    /// </summary>
    public enum StablePhase
    {
        EnterStable = 0,
        SearchStable = 1,
        FightWaves = 2,
        ChaseRider = 3,
        QuickDraw = 4,
        Reward = 5
    }

    [Header("Phase References")]
    [Tooltip("Root GameObjects for each phase. Index matches StablePhase enum value.")]
    [SerializeField] private GameObject[] phaseRoots = new GameObject[6];

    [Header("Phase 2 - Search")]
    [Tooltip("The lasso pickup GameObject.")]
    [SerializeField] private LassoPickup lassoPickup;

    [Header("Phase 3 - Fight Waves")]
    [Tooltip("The wave spawner that manages enemy waves.")]
    [SerializeField] private WaveSpawner waveSpawner;

    [Header("Phase 4 - Chase Rider")]
    [Tooltip("The phantom rider to chase.")]
    [SerializeField] private PhantomRider phantomRider;

    [Tooltip("The lasso throw component on the player.")]
    [SerializeField] private LassoThrow lassoThrow;

    [Tooltip("Reference to the player's health for damage-based reset.")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Phase 5 - Quick Draw")]
    [Tooltip("The duel controller for the quick draw phase.")]
    [SerializeField] private StableDuel stableDuel;

    [Header("Phase 6 - Reward")]
    [Tooltip("The reward controller for the final phase.")]
    [SerializeField] private StableReward stableReward;

    [Header("Phase Transition Events")]
    [Tooltip("Fired when entering Phase 1 - Enter Stable.")]
    public UnityEvent OnEnterStable;

    [Tooltip("Fired when entering Phase 2 - Search Stable.")]
    public UnityEvent OnSearchStable;

    [Tooltip("Fired when entering Phase 3 - Fight Waves.")]
    public UnityEvent OnFightWaves;

    [Tooltip("Fired when entering Phase 4 - Chase Rider.")]
    public UnityEvent OnChaseRider;

    [Tooltip("Fired when entering Phase 5 - Quick Draw.")]
    public UnityEvent OnQuickDraw;

    [Tooltip("Fired when entering Phase 6 - Reward.")]
    public UnityEvent OnReward;

    [Header("Settings")]
    [Tooltip("Delay between phase transitions in seconds.")]
    [SerializeField] private float phaseTransitionDelay = 0.5f;

    private StablePhase _currentPhase = StablePhase.EnterStable;
    private bool _transitioning;
    private int _playerHealthAtChaseStart;

    /// <summary>
    /// The current active phase of the Stable Level.
    /// </summary>
    public StablePhase CurrentPhase => _currentPhase;

    private void Start()
    {
        SetupPhase(StablePhase.EnterStable);
    }

    /// <summary>
    /// Advances to the next phase in the sequence with a fade transition.
    /// Call this from phase completion callbacks.
    /// </summary>
    public void AdvancePhase()
    {
        if (_transitioning) return;

        int nextIndex = (int)_currentPhase + 1;
        if (nextIndex > (int)StablePhase.Reward)
        {
            Debug.Log("[StableLevelManager] All phases complete!");
            return;
        }

        StartCoroutine(TransitionToPhase((StablePhase)nextIndex));
    }

    /// <summary>
    /// Forces the level to a specific phase. Useful for testing.
    /// </summary>
    /// <param name="phase">The phase to jump to.</param>
    public void SetPhase(StablePhase phase)
    {
        StartCoroutine(TransitionToPhase(phase));
    }

    private IEnumerator TransitionToPhase(StablePhase newPhase)
    {
        _transitioning = true;

        // Fade out
        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOut(phaseTransitionDelay);
        else
            yield return new WaitForSeconds(phaseTransitionDelay);

        _currentPhase = newPhase;
        SetupPhase(newPhase);

        // Fade in
        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeIn(phaseTransitionDelay);
        else
            yield return new WaitForSeconds(phaseTransitionDelay);

        _transitioning = false;
    }

    /// <summary>
    /// Configures the scene for the specified phase by enabling/disabling
    /// relevant GameObjects and invoking phase-specific setup logic.
    /// </summary>
    /// <param name="phase">The phase to set up.</param>
    private void SetupPhase(StablePhase phase)
    {
        // Enable/disable phase root objects
        for (int i = 0; i < phaseRoots.Length; i++)
        {
            if (phaseRoots[i] != null)
                phaseRoots[i].SetActive(i == (int)phase);
        }

        switch (phase)
        {
            case StablePhase.EnterStable:
                SetupEnterStable();
                break;
            case StablePhase.SearchStable:
                SetupSearchStable();
                break;
            case StablePhase.FightWaves:
                SetupFightWaves();
                break;
            case StablePhase.ChaseRider:
                SetupChaseRider();
                break;
            case StablePhase.QuickDraw:
                SetupQuickDraw();
                break;
            case StablePhase.Reward:
                SetupReward();
                break;
        }

        Debug.Log($"[StableLevelManager] Phase set to: {phase}");
    }

    private void SetupEnterStable()
    {
        // Phase 1: Player walks into the stable area
        // Lasso throw should be disabled
        if (lassoThrow != null)
            lassoThrow.SetActive(false);

        OnEnterStable?.Invoke();
    }

    private void SetupSearchStable()
    {
        // Phase 2: Player searches for lasso
        if (lassoThrow != null)
            lassoThrow.SetActive(false);

        OnSearchStable?.Invoke();
    }

    private void SetupFightWaves()
    {
        // Phase 3: Enemy wave fights begin
        if (lassoThrow != null)
            lassoThrow.SetActive(false);

        if (waveSpawner != null)
            waveSpawner.StartWaves();

        OnFightWaves?.Invoke();
    }

    private void SetupChaseRider()
    {
        // Phase 4: Chase the phantom rider with lasso
        if (lassoThrow != null)
            lassoThrow.SetActive(true);

        if (phantomRider != null)
            phantomRider.Activate();

        // Track player health for damage-based reset
        if (playerHealth != null)
            _playerHealthAtChaseStart = playerHealth.currentHealth;

        OnChaseRider?.Invoke();
    }

    private void Update()
    {
        // During Phase 4, check if player took damage to reset rider progress
        if (_currentPhase == StablePhase.ChaseRider && playerHealth != null && phantomRider != null)
        {
            if (playerHealth.currentHealth < _playerHealthAtChaseStart)
            {
                _playerHealthAtChaseStart = playerHealth.currentHealth;
                phantomRider.ResetProgress();
            }
        }
    }

    private void SetupQuickDraw()
    {
        // Phase 5: Quick draw duel
        if (lassoThrow != null)
            lassoThrow.SetActive(false);

        if (phantomRider != null)
            phantomRider.Deactivate();

        if (stableDuel != null)
            stableDuel.BeginDuel();

        OnQuickDraw?.Invoke();
    }

    private void SetupReward()
    {
        // Phase 6: Collect reward
        if (lassoThrow != null)
            lassoThrow.SetActive(false);

        if (stableReward != null)
            stableReward.SpawnRelic();

        OnReward?.Invoke();
    }
}
