using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;

public class NPCDialogue : MonoBehaviour
{
    public event Action DialogueFinished;

    [Header("Dialogue")]
    private string[] lines = new string[]
    {
        "Well, looky here... the Law's back. A bit late for a patrol, wouldn't ya say, Sheriff? Look around.",
        "This dirt don't grow corn no more—it only grows shadows.",
        "That rogue bunch... the cult... they reached for godhood and pulled down hell instead.",
        "Now, the folks you used to protect? They're just fuel for the fire that's burnin' this town 'til kingdom come.",
        "But you... you got that look in your eye. If you want to put these poor souls to rest, you gotta find 'em. Five relics, Sheriff.",
        "Hidden in the dark corners where the sun don't dare shine. They're the keys to the ritual.",
        "Scour the wasteland. Pry 'em from the cold, dead hands of the things guardin' 'em.",
        "Once you got all five, get yourself to the Church. Bring 'em together, and maybe—just maybe—we'll see a sunrise again.",
        "Better check your cylinder, son. The dead don't take kindly to trespassers."
    };

    [Header("UI")]
    public GameObject dialogueBox;
    public TMP_Text dialogueText;
    public TMP_Text promptText;

    [Header("Settings")]
    public float typingSpeed = 0.04f;
    public float triggerDistance = 2f;
    public KeyCode interactKey = KeyCode.Y;

    [Header("Audio")]
    public AudioMixer audioMixer;
    [Range(0f, 1f)]
    public float dialogueTransitionSpeed = 0.5f;

    private int currentLine = 0;
    private bool isTyping = false;
    private bool isDialogueOpen = false;
    private bool hasDialogueCompleted = false;
    private bool playerInRange = false;
    private Transform player;
    private Coroutine typingCoroutine;

    public bool HasDialogueCompleted => hasDialogueCompleted;

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
            StartDialogue();

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

        if (audioMixer != null)
            audioMixer.FindSnapshot("Dialogue").TransitionTo(dialogueTransitionSpeed);
    }

    void EndDialogue()
    {
        isDialogueOpen = false;
        isTyping = false;
        dialogueBox.SetActive(false);
        currentLine = 0;
        hasDialogueCompleted = true;

        DialogueFinished?.Invoke();

        if (audioMixer != null)
            audioMixer.FindSnapshot("Normal").TransitionTo(dialogueTransitionSpeed);
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
