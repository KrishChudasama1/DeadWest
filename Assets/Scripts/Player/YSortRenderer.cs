using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSortRenderer : MonoBehaviour
{
    [Tooltip("Offset from the object's pivot for sorting (useful if pivot isn't at feet).")]
    [SerializeField] private float sortingOffset = 0f;

    [Tooltip("Precision multiplier. Higher = more granular sorting.")]
    [SerializeField] private int precision = 100;

    [Tooltip("If true, updates every frame (for moving objects like the player). " +
             "Disable for static props to save performance.")]
    [SerializeField] private bool isDynamic = false;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateSortOrder();
    }

    private void LateUpdate()
    {
        if (isDynamic)
            UpdateSortOrder();
    }

    private void UpdateSortOrder()
    {
        
        spriteRenderer.sortingOrder = -(int)((transform.position.y + sortingOffset) * precision);
    }
}
