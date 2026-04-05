using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class BreakableObject : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 30;
    private int currentHealth;

    [Header("Damage Sprites")]
    [Tooltip("Sprites in order from healthy to most damaged. First = full health, Last = nearly broken.")]
    public Sprite[] damageStages;

    [Header("Flash")]
    public float flashDuration = 0.1f;

    [Header("Audio")]
    public AudioClip hitSound;
    public AudioClip breakSound;
    [Range(0f, 1f)] public float hitVolume  = 0.5f;
    [Range(0f, 1f)] public float breakVolume = 0.7f;

    private SpriteRenderer sr;
    private AudioSource audioSource;
    private bool isDead = false;

    private void Start()
    {
        sr          = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

        currentHealth = maxHealth;
        UpdateSprite();

        GetComponent<Collider2D>().isTrigger = false;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth  = Mathf.Max(currentHealth, 0);

        audioSource.PlayOneShot(hitSound, hitVolume);
        StartCoroutine(FlashWhite());
        UpdateSprite();

        if (currentHealth <= 0)
            Break();
    }

    private void UpdateSprite()
    {
        if (damageStages == null || damageStages.Length == 0) return;

        float healthPercent = (float)currentHealth / maxHealth;
        int stageCount      = damageStages.Length;
        int index           = Mathf.FloorToInt((1f - healthPercent) * stageCount);
        index               = Mathf.Clamp(index, 0, stageCount - 1);

        if (damageStages[index] != null)
            sr.sprite = damageStages[index];
    }

    private void Break()
    {
        isDead = true;

        GetComponent<Collider2D>().enabled = false;

        audioSource.PlayOneShot(breakSound, breakVolume);
        sr.enabled = false;

        Destroy(gameObject, breakSound != null ? breakSound.length + 0.1f : 0.1f);
    }

    private IEnumerator FlashWhite()
    {
        sr.color = Color.white * 2f;
        yield return new WaitForSeconds(flashDuration);
        sr.color = Color.white;
    }
    
    private void LateUpdate()
    {
        if (sr != null)
        {
            float bottom = transform.position.y - sr.bounds.extents.y;
            sr.sortingOrder = Mathf.RoundToInt(-bottom * 100);
        }
    }
}
