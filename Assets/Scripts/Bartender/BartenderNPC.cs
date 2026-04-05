using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BartenderNPC : MonoBehaviour
{
    [Header("Interaction")]
    public float interactDistance = 2f;
    public KeyCode interactKey = KeyCode.Y;

    [Header("UI - Cost Labels")]
    public TMP_Text healCostText;
    public TMP_Text levelUpCostText;

    [Header("UI - Prompt")]
    public TMP_Text promptText;

    [Header("UI - Dialogue Box")]
    public GameObject dialogueBox;
    public TMP_Text dialogueText;
    public float typingSpeed = 0.04f;

    [Header("UI - Choice Buttons")]
    public GameObject choicePanel;
    public Button choiceButtonA;
    public Button choiceButtonB;
    public TMP_Text choiceAText;
    public TMP_Text choiceBText;

    [Header("UI - Service Menu")]
    public GameObject servicePanel;
    public Button healButton;
    public Button levelUpButton;

    [Header("Costs")]
    public int healCost    = 2;
    public int levelUpCost = 6;

    [Header("Player")]
    public int healAmount = 50;

    [Header("Cooldown")]
    public float interactCooldown = 3f;
    private float cooldownTimer   = 0f;
    private bool onCooldown       = false;

    private Transform      player;
    private SpriteRenderer sr;
    private PlayerMovement playerMovement;
    private PlayerHealth   playerHealth;
    private XPManager      xpManager;

    private bool playerInRange   = false;
    private bool isInteracting   = false;
    private bool hasMetBartender = false;
    private bool hasBoughtDrink  = false;
    private bool isTyping        = false;
    private bool skipTyping      = false;
    private bool canSkip         = false;
    private bool justInteracted  = false;
    private Coroutine typingCoroutine;

    // ─────────────────────────────────────────
    //  DIALOGUE STRINGS
    // ─────────────────────────────────────────
    private string bartenderIntro =
        "Set your spurs down, traveler. You look like you've been chased by something that doesn't have a heartbeat. " +
        "What brings a living soul into a graveyard like this?";

    private string playerChoiceA = "Just let me get a drink.";
    private string playerChoiceB = "I'm here for the Relic. Where is it?";

    private string bartenderResponseA =
        "Straight to the point. I like that. In the Dead West, a man's last thirst is usually his most honest one. " +
        "But I suggest you drink fast—the air is getting heavy.";

    private string bartenderResponseB =
        "The Relic? Lower your voice, friend. That hunk of stone is the only reason the floorboards aren't screaming. " +
        "It's tucked away, but it's drawing eyes from the other side. You'd best steel your nerves before the 'regulars' show up to claim it.";

    private string drinkMenuLine =
        "I've got two special brews today. One to mend the flesh, and one to sharpen the spirit. " +
        "Pick your poison—you're going to need the edge for what's coming through those doors.";

    private string healResponseLine =
        "The Red Brew. Tastes like iron and copper, but it'll keep your heart beating when the lead starts flying. " +
        "Drink up... I hear them scratching at the walls already.";

    private string levelUpResponseLine =
        "The Gold Brew. A bit of liquid courage to help you grow stronger. " +
        "Keep your head on straight, traveler—the Relic won't protect itself.";

    private string notEnoughCoinsLine =
        "You're a little short, friend. Come back when your pockets aren't so empty.";

    // ─────────────────────────────────────────
    //  START
    // ─────────────────────────────────────────
    private void Start()
    {
        sr             = GetComponent<SpriteRenderer>();
        player         = GameObject.FindGameObjectWithTag("Player").transform;
        playerMovement = player.GetComponent<PlayerMovement>();
        playerHealth   = player.GetComponent<PlayerHealth>();
        xpManager      = FindObjectOfType<XPManager>();

        HideAll();

        choiceButtonA.onClick.AddListener(() => OnPlayerChoice(true));
        choiceButtonB.onClick.AddListener(() => OnPlayerChoice(false));
        healButton.onClick.AddListener(OnHeal);
        levelUpButton.onClick.AddListener(OnLevelUp);

        if (healCostText    != null) healCostText.text    = healCost    + " coins";
        if (levelUpCostText != null) levelUpCostText.text = levelUpCost + " coins";
    }

    // ─────────────────────────────────────────
    //  UPDATE
    // ─────────────────────────────────────────
    private void Update()
    {
        if (player == null) return;

        // Tick cooldown
        if (onCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                onCooldown = false;
                cooldownTimer = 0f;
            }
            else
            {
                if (promptText != null)
                    promptText.gameObject.SetActive(false);
                return;
            }
        }

        float distance = Vector2.Distance(transform.position, player.position);
        playerInRange  = distance <= interactDistance;

        if (promptText != null)
            promptText.gameObject.SetActive(playerInRange && !isInteracting);

        // Open interaction
        if (playerInRange && !isInteracting && Input.GetKeyDown(interactKey))
        {
            justInteracted = true;
            StartInteraction();
            return;
        }

        // Skip typing mid-line
        if (isInteracting && isTyping && canSkip && Input.GetKeyDown(interactKey))
        {
            skipTyping = true;
            return;
        }

        // Cancel service panel with Y — but not same frame as another action
        if (!justInteracted && !isTyping && servicePanel != null
            && servicePanel.activeSelf && Input.GetKeyDown(interactKey))
        {
            servicePanel.SetActive(false);
            CloseAll();
            return;
        }

        // Clear same-frame guard
        if (justInteracted)
            justInteracted = false;
    }

    // ─────────────────────────────────────────
    //  START INTERACTION
    // ─────────────────────────────────────────
    private void StartInteraction()
    {
        isInteracting = true;

        if (playerMovement != null) playerMovement.SetMovementLocked(true);
        PlayerShooting.IsInteracting = true;

        if (promptText != null)
            promptText.gameObject.SetActive(false);

        if (!hasMetBartender)
        {
            dialogueBox.SetActive(true);
            choicePanel.SetActive(false);
            servicePanel.SetActive(false);
            StartCoroutine(StartTypingNextFrame(bartenderIntro, ShowIntroChoices));
        }
        else
        {
            dialogueBox.SetActive(false);
            servicePanel.SetActive(true);
            ShowCursor();
        }
    }

    private IEnumerator StartTypingNextFrame(string line, System.Action onComplete)
    {
        yield return null;
        canSkip         = true;
        typingCoroutine = StartCoroutine(TypeLine(line, onComplete));
    }

    // ─────────────────────────────────────────
    //  INTRO FLOW
    // ─────────────────────────────────────────
    private void ShowIntroChoices()
    {
        canSkip          = false;
        choiceAText.text = playerChoiceA;
        choiceBText.text = playerChoiceB;
        choicePanel.SetActive(true);
        ShowCursor();
    }

    private void OnPlayerChoice(bool choseA)
    {
        choicePanel.SetActive(false);
        HideCursor();

        string response = choseA ? bartenderResponseA : bartenderResponseB;
        StartCoroutine(StartTypingNextFrame(response, FinishIntro));
    }

    private void FinishIntro()
    {
        hasMetBartender = true;
        StartCoroutine(WaitForYThenDrinkMenu());
    }

    private IEnumerator WaitForYThenDrinkMenu()
    {
        canSkip            = false;
        dialogueText.text += "\n\n<color=#aaaaaa>[Press Y to continue]</color>";

        // Wait two frames then listen for Y
        yield return null;
        yield return null;
        yield return new WaitUntil(() => Input.GetKeyDown(interactKey));

        StartCoroutine(StartTypingNextFrame(drinkMenuLine, ShowDrinkOptions));
    }

    private void ShowDrinkOptions()
    {
        canSkip            = false;
        dialogueText.text += "\n\n<color=#aaaaaa>[Press Y to cancel]</color>";
        servicePanel.SetActive(true);
        ShowCursor();
    }

    // ─────────────────────────────────────────
    //  SERVICE PANEL
    // ─────────────────────────────────────────
    private void OnHeal()
    {
        if (CoinManager.instance == null || !CoinManager.instance.HasCoins(healCost))
        {
            servicePanel.SetActive(false);
            HideCursor();
            dialogueBox.SetActive(true);
            StartCoroutine(StartTypingNextFrame(notEnoughCoinsLine,
                () => StartCoroutine(DelayThen(1.5f, () =>
                {
                    dialogueBox.SetActive(false);
                    servicePanel.SetActive(true);
                    ShowCursor();
                }))));
            return;
        }

        CoinManager.instance.SpendCoins(healCost);
        servicePanel.SetActive(false);
        HideCursor();

        if (playerHealth != null)
            playerHealth.Heal(healAmount);

        if (!hasBoughtDrink)
        {
            hasBoughtDrink = true;
            dialogueBox.SetActive(true);
            StartCoroutine(StartTypingNextFrame(healResponseLine,
                () => StartCoroutine(DelayThen(1.2f, CloseAll))));
        }
        else
        {
            CloseAll();
        }
    }

    private void OnLevelUp()
    {
        if (CoinManager.instance == null || !CoinManager.instance.HasCoins(levelUpCost))
        {
            servicePanel.SetActive(false);
            HideCursor();
            dialogueBox.SetActive(true);
            StartCoroutine(StartTypingNextFrame(notEnoughCoinsLine,
                () => StartCoroutine(DelayThen(1.5f, () =>
                {
                    dialogueBox.SetActive(false);
                    servicePanel.SetActive(true);
                    ShowCursor();
                }))));
            return;
        }

        CoinManager.instance.SpendCoins(levelUpCost);
        servicePanel.SetActive(false);
        HideCursor();

        if (xpManager != null)
            xpManager.GainExperience(xpManager.XPToLevel);

        if (!hasBoughtDrink)
        {
            hasBoughtDrink = true;
            dialogueBox.SetActive(true);
            StartCoroutine(StartTypingNextFrame(levelUpResponseLine,
                () => StartCoroutine(DelayThen(1.2f, CloseAll))));
        }
        else
        {
            CloseAll();
        }
    }

    // ─────────────────────────────────────────
    //  CLOSE
    // ─────────────────────────────────────────
    private void CloseAll()
    {
        isInteracting = false;
        canSkip       = false;

        // Start waves only after the very first interaction completes
        if (hasMetBartender && WaveManager.instance != null)
            WaveManager.instance.StartWaves();

        // Start cooldown only after full close
        onCooldown    = true;
        cooldownTimer = interactCooldown;

        if (playerMovement != null) playerMovement.SetMovementLocked(false);
        PlayerShooting.IsInteracting = false;

        HideAll();
        HideCursor();
    }

    private void HideAll()
    {
        if (promptText   != null) promptText.gameObject.SetActive(false);
        if (dialogueBox  != null) dialogueBox.SetActive(false);
        if (choicePanel  != null) choicePanel.SetActive(false);
        if (servicePanel != null) servicePanel.SetActive(false);
    }

    // ─────────────────────────────────────────
    //  CURSOR
    // ─────────────────────────────────────────
    private void ShowCursor()
    {
        PlayerShooting.IsInteracting = true;
        Cursor.visible   = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void HideCursor()
    {
        PlayerShooting.IsInteracting = false;
        Cursor.visible   = false;
        Cursor.lockState = CursorLockMode.None;
    }

    // ─────────────────────────────────────────
    //  TYPING
    // ─────────────────────────────────────────
    private IEnumerator TypeLine(string line, System.Action onComplete = null)
    {
        isTyping          = true;
        skipTyping        = false;
        dialogueText.text = "";

        foreach (char letter in line)
        {
            if (skipTyping)
            {
                dialogueText.text = line;
                break;
            }

            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping   = false;
        skipTyping = false;
        canSkip    = false;
        onComplete?.Invoke();
    }

    // ─────────────────────────────────────────
    //  UTILITY
    // ─────────────────────────────────────────
    private IEnumerator DelayThen(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactDistance);
    }
}