using UnityEngine;

public class DamagePlayerOnDialogueFinished : MonoBehaviour
{
    [SerializeField] private NPCDialogue dialogueTrigger;
    [SerializeField] private int damageAmount = 15;
    [SerializeField] private bool damageOnlyOnce = true;

    private bool hasDamaged;

    private void Start()
    {
        if (dialogueTrigger == null)
            dialogueTrigger = GetComponent<NPCDialogue>();

        if (dialogueTrigger == null)
            dialogueTrigger = FindFirstObjectByType<NPCDialogue>();

        if (dialogueTrigger == null)
            return;

        dialogueTrigger.DialogueFinished += DealDamage;
    }

    private void OnDestroy()
    {
        if (dialogueTrigger != null)
            dialogueTrigger.DialogueFinished -= DealDamage;
    }

    private void DealDamage()
    {
        if (damageOnlyOnce && hasDamaged)
            return;

        hasDamaged = true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        PlayerHealth hp = player.GetComponent<PlayerHealth>();
        if (hp != null)
            hp.TakeDamage(damageAmount);
    }
}