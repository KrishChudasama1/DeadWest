using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using TMPro;

public class NPCDialogue : MonoBehaviour
{
    [Serializable]
    public class DialogueLine
    {
        [TextArea(2, 4)]
        public string text;
    }

    public event Action DialogueFinished;

    [Header("Dialogue")]
    [SerializeField] private DialogueLine[] lines;

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
    public float dialogueTransitionSpeed = 0.5f; // how fast it fades in/out

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
                dialogueText.text = lines[currentLine].text;
                isTyping = false;
            }
            else
            {
                currentLine++;
                if (currentLine < lines.Length)
                    typingCoroutine = StartCoroutine(TypeLine(lines[currentLine].text));
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
        if (lines != null && lines.Length > 0)
            typingCoroutine = StartCoroutine(TypeLine(lines[currentLine].text));
        else
            EndDialogue();

        // Transition to the quiet Dialogue snapshot
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

        // Transition back to normal volume
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
