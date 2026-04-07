using System.Collections;
using UnityEngine;

public class SlowZone : MonoBehaviour
{
    [Tooltip("Multiplier applied to player speed while inside (0.7 = 30% slow).")]
    public float playerSpeedMultiplier = 0.7f;

    [Tooltip("Radius of the zone (world units). If zero, will use attached CircleCollider2D radius.")]
    public float radius = 0f;

    private CircleCollider2D _collider;
    private System.Collections.Generic.Dictionary<PlayerMovement, Coroutine> _activeTimers = new System.Collections.Generic.Dictionary<PlayerMovement, Coroutine>();
    public float stepSlowDuration = 3f;

    private void Awake()
    {
        _collider = GetComponent<CircleCollider2D>();
        if (_collider == null)
        {
            _collider = gameObject.AddComponent<CircleCollider2D>();
        }
        _collider.isTrigger = true;
        if (radius > 0f) _collider.radius = radius;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<global::Bullet>() != null) return;

        if (other.CompareTag("Player"))
        {
            PlayerMovement pm = other.GetComponent<PlayerMovement>();
            if (pm != null)
            {
                if (_activeTimers.TryGetValue(pm, out Coroutine existing))
                {
                    if (existing != null) StopCoroutine(existing);
                    _activeTimers.Remove(pm);
                }

                pm.AddSpeedMultiplier(playerSpeedMultiplier);

                Coroutine c = StartCoroutine(RemoveAfterDelay(pm, playerSpeedMultiplier, stepSlowDuration));
                _activeTimers[pm] = c;
            }
        }
    }

    private IEnumerator RemoveAfterDelay(PlayerMovement pm, float multiplier, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (pm != null)
        {
            pm.RemoveSpeedMultiplier(multiplier);
            pm.RefreshTint();
        }
        if (_activeTimers.ContainsKey(pm))
            _activeTimers.Remove(pm);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
    }
}
