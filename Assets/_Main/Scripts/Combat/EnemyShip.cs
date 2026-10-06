using UnityEngine;
using System.Collections.Generic;

public enum EnemyRole { Interceptor, Gunner, Scout, Bulwark }
[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class EnemyShip : MonoBehaviour
{
    public static readonly HashSet<EnemyShip> Active = new HashSet<EnemyShip>();
    public static int TotalDestroyed;
    public EnemyRole role;
    public int health = 3, xpValue = 6;
    public float speed = 4f, attackInterval = 3f;
    public EnemyProjectile projectile;
    public ExperiencePickup pickup;
    public Sprite barSprite;
    Rigidbody2D rb;
    Transform player;
    SpriteRenderer visual, healthBar, warning;
    int maxHealth;
    float attackTimer = 2f, chargeTimer, burstTimer, flashTime, age;
    Vector2 chargeDirection;
    bool dead;
    void OnEnable() { Active.Add(this); }
    void OnDisable() { Active.Remove(this); }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); visual = GetComponent<SpriteRenderer>();
        player = FindAnyObjectByType<PlayerController>().transform;
        maxHealth = health;
        healthBar = MakeSprite("Hull", new Vector3(0, -0.9f, 0), new Vector3(1, 0.06f, 1), CombatFeedback.Coral);
        healthBar.enabled = false;
        warning = MakeSprite("Attack telegraph", Vector3.zero, Vector3.one * 1.35f, CombatFeedback.Coral);
        warning.sprite = visual.sprite; warning.sortingOrder = visual.sortingOrder - 1; warning.enabled = false;
    }
    SpriteRenderer MakeSprite(string label, Vector3 pos, Vector3 scale, Color color)
    {
        var go = new GameObject(label); go.transform.SetParent(transform, false);
        go.transform.localPosition = pos; go.transform.localScale = scale;
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = barSprite; sr.color = color; sr.sortingOrder = 12; return sr;
    }
    void FixedUpdate()
    {
        if (GameState.IsPaused || !player || dead) return;
        age += Time.fixedDeltaTime;
        Vector2 delta = (Vector2)player.position - rb.position;
        Vector2 dir = delta.normalized;
        float distance = delta.magnitude;
        Vector2 velocity;
        attackTimer -= Time.fixedDeltaTime;
        if (role == EnemyRole.Interceptor)
        {
            if (burstTimer > 0f) { burstTimer -= Time.fixedDeltaTime; velocity = chargeDirection * 15f; }
            else if (chargeTimer > 0f)
            {
                chargeTimer -= Time.fixedDeltaTime; velocity = Vector2.zero;
                if (chargeTimer <= 0) { burstTimer = 0.65f; warning.enabled = false; }
            }
            else
            {
                velocity = dir * speed;
                if (attackTimer <= 0 && distance < 18f)
                { chargeDirection = dir; chargeTimer = 0.65f; attackTimer = attackInterval; warning.enabled = true; }
            }
        }
        else
        {
            float desired = role == EnemyRole.Bulwark ? 12f : 9f;
            velocity = dir * Mathf.Clamp(distance - desired, -1f, 1f) * speed;
            if (role == EnemyRole.Scout) velocity += new Vector2(-dir.y, dir.x) * speed * 0.8f;
            warning.enabled = attackTimer < 0.65f;
            if (attackTimer <= 0 && distance < 28f)
            {
                Fire(dir); attackTimer = attackInterval; warning.enabled = false;
            }
        }
        if (role == EnemyRole.Interceptor && (chargeTimer > 0 || burstTimer > 0)) dir = chargeDirection;
        rb.MoveRotation(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
        Vector2 next = rb.position + velocity * Time.fixedDeltaTime;
        // After entering, keep combatants near the arena rather than orbiting off-screen.
        if (age > 3f && EnemyDirector.Instance)
        {
            var cam = Camera.main; Vector2 c = EnemyDirector.Instance.Center;
            next.x = Mathf.Clamp(next.x, c.x - cam.orthographicSize * cam.aspect + 0.8f, c.x + cam.orthographicSize * cam.aspect - 0.8f);
            next.y = Mathf.Clamp(next.y, c.y - cam.orthographicSize + 1.5f, c.y + cam.orthographicSize - 3f);
        }
        rb.MovePosition(next);
        flashTime = Mathf.Max(0, flashTime - Time.fixedDeltaTime);
        visual.color = flashTime > 0 ? Color.white * 2.5f : Color.white;
        if (warning.enabled) warning.color = new Color(1f, 0.35f, 0.25f, 0.25f + 0.3f * Mathf.Abs(Mathf.Sin(age * 14)));
    }
    void Fire(Vector2 dir)
    {
        int count = role == EnemyRole.Bulwark ? 8 : role == EnemyRole.Gunner ? 3 : 1;
        for (int i = 0; i < count; i++)
        {
            float angle = role == EnemyRole.Bulwark ? i * 45f : (i - (count - 1) * 0.5f) * 16f;
            Vector2 heading = Quaternion.Euler(0, 0, angle) * dir;
            var shot = Instantiate(projectile, transform.position + (Vector3)heading * 0.9f, Quaternion.identity);
            shot.Init(heading, role == EnemyRole.Scout ? 12f : 8f);
        }
        CombatFeedback.Instance?.Burst(transform.position, CombatFeedback.Coral, 4, 2f);
    }
    public void TakeDamage(int damage)
    {
        if (dead || GameState.IsDead) return;
        health -= damage; flashTime = 0.09f;
        if (healthBar) { healthBar.enabled = true; healthBar.transform.localScale = new Vector3(Mathf.Max(0, (float)health / maxHealth), 0.06f, 1); }
        if (health > 0) return;
        dead = true; TotalDestroyed++;
        ExperiencePickup.Drop(pickup, transform.position, xpValue);
        CombatFeedback.Instance?.Burst(transform.position, CombatFeedback.Coral, 24, 5f);
        CombatFeedback.Instance?.Shake(role == EnemyRole.Bulwark ? 0.15f : 0.08f, 0.16f);
        SoundManager.Instance?.PlayAsteroidDestroySound();
        Destroy(gameObject);
    }
    void OnTriggerEnter2D(Collider2D other)
    { if (!dead && other.TryGetComponent<PlayerHealth>(out var healthComponent)) healthComponent.TakeHit(); }
}

