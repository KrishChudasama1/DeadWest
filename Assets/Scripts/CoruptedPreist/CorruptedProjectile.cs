using UnityEngine;

public class CorruptedProjectile : MonoBehaviour
{
    private Vector2 direction;
    private int damage;
    private float speed;
    public float lifetime = 4f;

    public void Init(Vector2 dir, int dmg, float spd)
    {
        direction = dir;
        damage = dmg;
        speed = spd;
        Destroy(gameObject, lifetime);
        if (dir.x < 0)
            GetComponent<SpriteRenderer>().flipX = true;
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}