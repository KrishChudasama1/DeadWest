using System.Collections;
using UnityEngine;
using TMPro;

public class NPCDialogue : MonoBehaviour
{
    [Header("Dialogue")]
    private string[] lines = new string[]
    {
        "Sheriff... you came back. I never thought I'd see you again.",
        "I am what remains of Old Man Hector. The town's former gravedigger.",
        "Something terrible happened here. A cult of outlaws performed a dark ritual seeking immortality.",
        "They were fools. The ritual went wrong and unleashed forces beyond their control.",
        "In one night the entire town was wiped out. Everyone... gone.",
        "The streets are now crawling with the undead, cursed spirits and twisted creatures.",
        "But there is still hope, Sheriff. There is a way to break the curse.",
        "Five sacred relics were scattered across the cursed lands surrounding the town.",
        "Each one is guarded by powerful supernatural enemies. They won't give them up easily.",
        "You must find all five relics and bring them to the old church.",
        "The relics can be assembled into a sacred artifact at the church altar.",
        "Activating it will lift the curse and free every trapped soul in this town.",
        "Including me...",
        "The relics are hidden in the old mine, the burial grounds, the canyon, the swamp and the outlaw fort.",
        "Be careful out there Sheriff. Whatever you do... don't let the darkness take you too.",
        "This town is counting on you. You're our only hope."
    };

    [Header("UI")]
    public GameObject dialogueBox;
    public TMP_Text dialogueText;
    public TMP_Text promptText;

    [Header("Settings")]
    public float typingSpeed = 0.04f;
    public float triggerDistance = 2f;
    public KeyCode interactKey = KeyCode.Y;

    private int currentLine = 0;
    private bool isTyping = false;
    private bool isDialogueOpen = false;
    private bool playerInRange = false;
    private Transform player;
    private Coroutine typingCoroutine;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        dialogueBox.SetActive(false);
        promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        playerInRange = distance <= triggerDistance;

        promptText.gameObject.SetActive(playerInRange && !isDialogueOpen);

        if (playerInRange && Input.GetKeyDown(interactKey) && !isDialogueOpen)
        {
            StartDialogue();
        }

        if (isDialogueOpen && Input.GetKeyDown(interactKey))
        {
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = lines[currentLine];
                isTyping = false;
            }
            else
            {
                currentLine++;
                if (currentLine < lines.Length)
                    typingCoroutine = StartCoroutine(TypeLine(lines[currentLine]));
                else
                    EndDialogue();
            }
        }
    }

    void StartDialogue()
    {
        currentLine = 0;
        isDialogueOpen = true;
        dialogueBox.SetActive(true);
        typingCoroutine = StartCoroutine(TypeLine(lines[currentLine]));
    }

    void EndDialogue()
    {
        isDialogueOpen = false;
        isTyping = false;
        dialogueBox.SetActive(false);
        currentLine = 0;
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in line)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}