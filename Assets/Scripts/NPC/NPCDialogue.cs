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

    [Tooltip("If true, the NPC will fade out after finishing dialogue.")]
    public bool fadeAfterDialogue = false;

    [Tooltip("Delay (seconds) before starting the fade after dialogue ends.")]
    public float fadeDelay = 0.2f;

    [Tooltip("How long the fade takes (seconds).")]
    public float fadeDuration = 0.8f;

    [Tooltip("If true, dialogue starts automatically when the player is in range (no key press to open).")]
    public bool autoStart = false;

    [Tooltip("If true, the NPC can only be talked to once per scene load.")]
    public bool oneShot = false;

    [Tooltip("Lock player movement and shooting while dialogue is open.")]
    public bool lockPlayerDuringDialogue = true;

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

    
    private SpriteRenderer[] _spriteRenderers;
    private Collider2D[] _colliders2D;

    public bool HasDialogueCompleted => hasDialogueCompleted;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;

        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (promptText != null) promptText.gameObject.SetActive(false);

   
    _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    _colliders2D = GetComponentsInChildren<Collider2D>(true);
    }

    void Update()
    {
       
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else return;
        }

        float distance = Vector2.Distance(transform.position, player.position);
        playerInRange = distance <= triggerDistance;

        if (promptText != null)
            promptText.gameObject.SetActive(playerInRange && !isDialogueOpen && !hasDialogueCompleted);

      
        if (autoStart && playerInRange && !isDialogueOpen && !hasDialogueCompleted)
        {
            StartDialogue();
            return;
        }

        
        if (!autoStart && playerInRange && Input.GetKeyDown(interactKey) && !isDialogueOpen)
        {
            if (oneShot && hasDialogueCompleted) return;
            StartDialogue();
        }

        
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

        if (lockPlayerDuringDialogue)
        {
            PlayerMovement pm = player != null ? player.GetComponent<PlayerMovement>() : null;
            if (pm != null) pm.SetMovementLocked(true);
            PlayerShooting.IsInteracting = true;
        }

        if (dialogueBox != null) dialogueBox.SetActive(true);
        if (promptText != null) promptText.gameObject.SetActive(false);

        if (lines != null && lines.Length > 0)
            typingCoroutine = StartCoroutine(TypeLine(lines[currentLine].text));
        else
            EndDialogue();

        if (audioMixer != null)
            audioMixer.FindSnapshot("Dialogue").TransitionTo(dialogueTransitionSpeed);
    }

    void EndDialogue()
    {
        isDialogueOpen = false;
        isTyping = false;
        hasDialogueCompleted = true;

        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (promptText != null) promptText.gameObject.SetActive(false);
        currentLine = 0;

        if (lockPlayerDuringDialogue)
        {
            PlayerMovement pm = player != null ? player.GetComponent<PlayerMovement>() : null;
            if (pm != null) pm.SetMovementLocked(false);
            PlayerShooting.IsInteracting = false;
        }

        DialogueFinished?.Invoke();

        if (fadeAfterDialogue)
        {
            
            if (_colliders2D != null)
            {
                foreach (var c in _colliders2D)
                {
                    if (c != null) c.enabled = false;
                }
            }

            StartCoroutine(FadeAndDisable());
        }

        if (audioMixer != null)
            audioMixer.FindSnapshot("Normal").TransitionTo(dialogueTransitionSpeed);
    }

    IEnumerator FadeAndDisable()
    {
        
        if (fadeDelay > 0f) yield return new WaitForSeconds(fadeDelay);

        if (_spriteRenderers == null || _spriteRenderers.Length == 0)
        {
           
            gameObject.SetActive(false);
            yield break;
        }

        float elapsed = 0f;
     
        Color[] initialColors = new Color[_spriteRenderers.Length];
        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            var r = _spriteRenderers[i];
            initialColors[i] = r != null ? r.color : Color.clear;
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float a = Mathf.Lerp(1f, 0f, t);
            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                var r = _spriteRenderers[i];
                if (r == null) continue;
                Color c = initialColors[i];
                c.a = a;
                r.color = c;
            }
            yield return null;
        }

        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            var r = _spriteRenderers[i];
            if (r == null) continue;
            Color c = initialColors[i];
            c.a = 0f;
            r.color = c;
        }
        
        gameObject.SetActive(false);
    }

   
    public void SetLines(DialogueLine[] newLines)
    {
        lines = newLines;
        hasDialogueCompleted = false;
        currentLine = 0;
    }

    IEnumerator TypeLine(string line)
    {
        isTyping = true;
        if (dialogueText != null) dialogueText.text = "";

        foreach (char letter in line)
        {
            if (dialogueText != null) dialogueText.text += letter;
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
