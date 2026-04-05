using UnityEngine;

public class ActivateGameObjectOnDialogueFinished : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private NPCDialogue dialogueTrigger;

    [Header("Target")]
    [SerializeField] private GameObject objectToActivate;
    [SerializeField] private bool deactivateOnStart = true;

    [Header("Audio")]
    [SerializeField] private AudioClip activateClip;
    [SerializeField] private bool playOnActivate = true;

    private bool hasActivated;

    private void Start()
    {
        if (objectToActivate != null && deactivateOnStart)
            objectToActivate.SetActive(false);

        if (dialogueTrigger == null)
            dialogueTrigger = FindFirstObjectByType<NPCDialogue>();

        if (dialogueTrigger == null)
        {
            Debug.LogWarning("ActivateGameObjectOnDialogueFinished could not find NPCDialogue.");
            return;
        }

        if (dialogueTrigger.HasDialogueCompleted)
            ActivateTarget();
        else
            dialogueTrigger.DialogueFinished += ActivateTarget;
    }

    private void OnDestroy()
    {
        if (dialogueTrigger != null)
            dialogueTrigger.DialogueFinished -= ActivateTarget;
    }

    private void ActivateTarget()
    {
        if (hasActivated)
            return;

        hasActivated = true;

        if (dialogueTrigger != null)
            dialogueTrigger.DialogueFinished -= ActivateTarget;

        if (objectToActivate != null)
            objectToActivate.SetActive(true);

        if (playOnActivate)
            PlayActivateSound();
    }

    private void PlayActivateSound()
    {
        if (activateClip == null)
            return;

        AudioSource.PlayClipAtPoint(activateClip, Camera.main != null ? Camera.main.transform.position : transform.position);
    }
}