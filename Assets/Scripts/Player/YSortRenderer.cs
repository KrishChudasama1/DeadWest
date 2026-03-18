using UnityEngine;

/// <summary>
/// Automatically sets the SpriteRenderer's sortingOrder based on the
/// object's Y position. Lower Y = rendered in front (higher order).
/// Essential for 2D top-down games so characters walk behind/in-front
/// of buildings and props correctly.
///
/// Attach to every sprite that needs depth sorting (player, NPCs, buildings, props).
/// </summary>
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
        // Negate Y so that lower positions (closer to camera in top-down) render on top
        spriteRenderer.sortingOrder = -(int)((transform.position.y + sortingOffset) * precision);
    }
}
