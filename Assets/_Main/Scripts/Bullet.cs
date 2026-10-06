using UnityEngine;
using System.Collections.Generic;

public class Bullet : MonoBehaviour
{
    public float speed = 30f, lifeTime = 2f;
    public int pierceCount; public int damage = 1;
    public bool canSplit;
    bool spent;
    float life;
    Rigidbody2D rb;
    readonly HashSet<GameObject> hit = new HashSet<GameObject>();
    void Awake() { rb = GetComponent<Rigidbody2D>(); }
    void Start() { rb.linearVelocity = transform.up * speed; life = lifeTime; }
    void Update() { if (!GameState.IsPaused && (life -= Time.deltaTime) <= 0f) Destroy(gameObject); }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (spent || GameState.IsPaused) return;
        Asteroid asteroid = other.GetComponent<Asteroid>();
        EnemyShip enemy = other.GetComponent<EnemyShip>();
        if (!asteroid && !enemy) return;
        if (!hit.Add(other.gameObject)) return;
        if (asteroid) asteroid.TakeDamage(damage); else enemy.TakeDamage(damage);
        CombatFeedback.Instance?.Burst(transform.position, CombatFeedback.Cyan, 5, 2.5f);
        if (canSplit)
        {
            canSplit = false;
            for (int i = -1; i <= 1; i += 2)
            {
                var clone = Instantiate(gameObject, transform.position, transform.rotation * Quaternion.Euler(0, 0, i * 22));
                var bullet = clone.GetComponent<Bullet>();
                bullet.canSplit = false; bullet.pierceCount = 0; bullet.damage = 1; bullet.lifeTime = 0.4f;
                bullet.hit.Add(other.gameObject);
            }
        }
        if (pierceCount-- > 0) return;
        spent = true;
        Destroy(gameObject);
    }
    public void SetSpeed(float value) { speed = value; }
}

