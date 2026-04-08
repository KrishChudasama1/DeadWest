using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SlowZone : MonoBehaviour
{
    [Tooltip("Multiplier applied to player speed while inside (0.7 = 30% slow).")]
    public float playerSpeedMultiplier = 0.7f;

    [Tooltip("How long the slow lasts per step (seconds).")]
    public float stepSlowDuration = 5f;

    [Tooltip("Radius of the zone (world units). If zero, will use attached CircleCollider2D radius.")]
    public float radius = 0f;

    private CircleCollider2D _collider;

    private HashSet<PlayerMovement> _currentlySlowed = new HashSet<PlayerMovement>();

    private void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
        if (_collider == null)
            _collider = gameObject.AddComponent<CircleCollider2D>();

        _collider.isTrigger = true;
        if (radius > 0f) _collider.radius = radius;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<Bullet>() != null) return;

        if (!other.CompareTag("Player")) return;

        PlayerMovement pm = other.GetComponent<PlayerMovement>();
        if (pm == null) return;

        // If this zone already has an active slow on this player, do nothing
        if (_currentlySlowed.Contains(pm)) return;

        _currentlySlowed.Add(pm);
        pm.AddSpeedMultiplier(playerSpeedMultiplier);
        StartCoroutine(RemoveSlow(pm));
    }

    private IEnumerator RemoveSlow(PlayerMovement pm)
    {
        yield return new WaitForSeconds(stepSlowDuration);

        if (pm != null)
        {
            pm.RemoveSpeedMultiplier(playerSpeedMultiplier);
            pm.RefreshTint();
        }

        _currentlySlowed.Remove(pm);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
    }
}
