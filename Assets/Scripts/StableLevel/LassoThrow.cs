using UnityEngine;

/// <summary>
/// Handles lasso throwing input. On pressing F, instantiates a lasso projectile
/// aimed at the mouse cursor position.
/// </summary>
public class LassoThrow : MonoBehaviour
{
    [Header("Lasso Settings")]
    [Tooltip("Prefab for the lasso projectile. Must have a LassoProjectile component.")]
    [SerializeField] private GameObject lassoProjectilePrefab;

    [Tooltip("Cooldown between lasso throws in seconds.")]
    [SerializeField] private float throwCooldown = 0.8f;

    [Tooltip("Offset from player center where the lasso spawns.")]
    [SerializeField] private float spawnOffset = 0.5f;

    private float _lastThrowTime = -Mathf.Infinity;
    private bool _enabled = true;

    /// <summary>
    /// Enables or disables lasso throwing.
    /// </summary>
    /// <param name="active">Whether throwing is allowed.</param>
    public void SetActive(bool active)
    {
        _enabled = active;
    }

    private void Update()
    {
        if (!_enabled) return;
        if (lassoProjectilePrefab == null) return;

        if (Input.GetKeyDown(KeyCode.F) && Time.time - _lastThrowTime >= throwCooldown)
        {
            ThrowLasso();
        }
    }

    /// <summary>
    /// Instantiates a lasso projectile aimed at the current mouse world position.
    /// </summary>
    private void ThrowLasso()
    {
        _lastThrowTime = Time.time;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 direction = ((Vector2)mouseWorld - (Vector2)transform.position).normalized;
        Vector2 spawnPos = (Vector2)transform.position + direction * spawnOffset;

        GameObject lassoGO = Instantiate(lassoProjectilePrefab, spawnPos, Quaternion.identity);
        LassoProjectile lasso = lassoGO.GetComponent<LassoProjectile>();
        if (lasso != null)
            lasso.Init(direction);

        Debug.Log("[LassoThrow] Lasso thrown!");
    }
}
