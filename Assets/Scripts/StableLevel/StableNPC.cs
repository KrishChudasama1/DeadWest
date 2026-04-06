using System;
using System.Collections;
using UnityEngine;
using TMPro;

namespace StableLevel
{
    /// <summary>
    /// NPC that auto-starts a briefing dialogue when the player enters the stable.
    /// Uses the same typewriter + press-key-to-advance pattern as NPCDialogue.
    /// Place this on an NPC sprite in the HorseStable scene.
    /// </summary>
    public class StableNPC : MonoBehaviour
    {
        // ── Dialogue lines (editable in Inspector) ──────────────
        [Serializable]
        public class DialogueLine
        {
            [TextArea(2, 5)]
            public string text;
        }

        [Header("Dialogue")]
        [SerializeField] private DialogueLine[] lines = new DialogueLine[]
        {
            new DialogueLine { text = "Well, well... you finally made it to the stable, partner." },
            new DialogueLine { text = "Listen close — there's a lasso somewhere around here. Find it and pick it up." },
            new DialogueLine { text = "Once you've got it, a horde of ranch hands are gonna come for ya. Put 'em all down." },
            new DialogueLine { text = "After they're dealt with, a gate to the horse track will open up." },
            new DialogueLine { text = "That's where the Phantom Rider is holdin' the relic. Lasso that ghost three times and it's yours." },
            new DialogueLine { text = "Good luck out there, cowboy. You're gonna need it." }
        };

        [Header("UI References")]
        [Tooltip("Panel/image that holds the dialogue text. Will be toggled on/off.")]
        public GameObject dialogueBox;
        [Tooltip("TMP text component inside the dialogue box.")]
        public TMP_Text dialogueText;
        [Tooltip("Small prompt text shown before dialogue starts (e.g. 'Press Y').")]
        public TMP_Text promptText;

        [Header("Settings")]
        [SerializeField] private float typingSpeed = 0.035f;
        [SerializeField] private float triggerDistance = 3f;
        [SerializeField] private KeyCode advanceKey = KeyCode.Y;
        [Tooltip("If true, dialogue starts automatically when player is in range (no key press needed to open).")]
        [SerializeField] private bool autoStart = true;
        [Tooltip("Seconds to wait after scene load before allowing auto-start.")]
        [SerializeField] private float autoStartDelay = 1f;

        /// <summary>Fired when the player finishes all dialogue lines.</summary>
        public event Action OnBriefingComplete;

        // ── Private state ───────────────────────────────────────
        private int _currentLine;
        private bool _isTyping;
        private bool _isDialogueOpen;
        private bool _hasCompleted;
        private bool _canAutoStart;
        private Transform _player;
        private Coroutine _typingCoroutine;

        public bool HasCompleted => _hasCompleted;

        // ────────────────────────────────────────────────────────
        // Unity callbacks
        // ────────────────────────────────────────────────────────

        private void Start()
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) _player = p.transform;

            if (dialogueBox != null) dialogueBox.SetActive(false);
            if (promptText != null) promptText.gameObject.SetActive(false);

            if (autoStart)
                StartCoroutine(EnableAutoStartAfterDelay());
        }

        private IEnumerator EnableAutoStartAfterDelay()
        {
            yield return new WaitForSeconds(autoStartDelay);
            _canAutoStart = true;
        }

        private void Update()
        {
            if (_hasCompleted) return;
            if (_player == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) _player = p.transform;
                else return;
            }

            float dist = Vector2.Distance(transform.position, _player.position);
            bool inRange = dist <= triggerDistance;

            // Show/hide prompt
            if (promptText != null)
                promptText.gameObject.SetActive(inRange && !_isDialogueOpen && !_hasCompleted);

            // Auto-start when player walks in range
            if (autoStart && _canAutoStart && inRange && !_isDialogueOpen && !_hasCompleted)
            {
                StartDialogue();
                return;
            }

            // Manual start via key
            if (!autoStart && inRange && !_isDialogueOpen && Input.GetKeyDown(advanceKey))
            {
                StartDialogue();
                return;
            }

            // Advance dialogue
            if (_isDialogueOpen && Input.GetKeyDown(advanceKey))
            {
                if (_isTyping)
                {
                    // Skip typing — show full line immediately
                    if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
                    if (dialogueText != null) dialogueText.text = lines[_currentLine].text;
                    _isTyping = false;
                }
                else
                {
                    _currentLine++;
                    if (_currentLine < lines.Length)
                        _typingCoroutine = StartCoroutine(TypeLine(lines[_currentLine].text));
                    else
                        EndDialogue();
                }
            }
        }

        // ────────────────────────────────────────────────────────
        // Dialogue flow
        // ────────────────────────────────────────────────────────

        private void StartDialogue()
        {
            _currentLine = 0;
            _isDialogueOpen = true;

            // Lock player movement while dialogue is open
            if (_player != null)
            {
                var pm = _player.GetComponent<PlayerMovement>();
                if (pm != null) pm.SetMovementLocked(true);
            }
            PlayerShooting.IsInteracting = true;

            if (dialogueBox != null) dialogueBox.SetActive(true);
            if (promptText != null) promptText.gameObject.SetActive(false);

            if (lines != null && lines.Length > 0)
                _typingCoroutine = StartCoroutine(TypeLine(lines[0].text));
            else
                EndDialogue();
        }

        private void EndDialogue()
        {
            _isDialogueOpen = false;
            _isTyping = false;
            _hasCompleted = true;

            if (dialogueBox != null) dialogueBox.SetActive(false);
            if (promptText != null) promptText.gameObject.SetActive(false);

            // Unlock player movement
            if (_player != null)
            {
                var pm = _player.GetComponent<PlayerMovement>();
                if (pm != null) pm.SetMovementLocked(false);
            }
            PlayerShooting.IsInteracting = false;

            Debug.Log("StableNPC: Briefing complete.");
            OnBriefingComplete?.Invoke();
        }

        // ────────────────────────────────────────────────────────
        // Typewriter effect
        // ────────────────────────────────────────────────────────

        private IEnumerator TypeLine(string line)
        {
            _isTyping = true;
            if (dialogueText != null) dialogueText.text = "";

            foreach (char letter in line)
            {
                if (dialogueText != null) dialogueText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }

            _isTyping = false;
        }

        // ────────────────────────────────────────────────────────
        // Gizmos
        // ────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, triggerDistance);
        }
    }
}
