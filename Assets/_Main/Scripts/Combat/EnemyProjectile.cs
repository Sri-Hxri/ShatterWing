using UnityEngine;
public class EnemyProjectile : MonoBehaviour
{
    Vector2 heading;
    float speed, life = 7f;
    public void Init(Vector2 direction, float velocity)
    { heading = direction.normalized; speed = velocity; transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(heading.y, heading.x) * Mathf.Rad2Deg - 90); }
    void FixedUpdate()
    {
        if (GameState.IsPaused) return;
        GetComponent<Rigidbody2D>().MovePosition((Vector2)transform.position + heading * speed * Time.fixedDeltaTime);
        if ((life -= Time.fixedDeltaTime) <= 0) Destroy(gameObject);
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (GameState.IsPaused || !other.TryGetComponent<PlayerHealth>(out var health)) return;
        health.TakeHit(); Destroy(gameObject);
    }
}
