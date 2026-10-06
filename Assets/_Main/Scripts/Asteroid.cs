using UnityEngine;
using System.Collections.Generic;

public enum AsteroidSize { Small, Medium, Large }
public class Asteroid : MonoBehaviour
{
    public AsteroidSize size;
    public static int totalDestroyed;
    public float maxLifetime = 24f, maxDistanceFromPlayer = 70f;
    public float moveSpeed;
    public int health, expValue;
    public Sprite[] variants;
    public ExperiencePickup pickupPrefab;
    float life, flashTime;
    bool destroyed;
    Rigidbody2D rb;
    SpriteRenderer visual;
    Color baseColor;
    void Awake() { rb = GetComponent<Rigidbody2D>(); visual = GetComponent<SpriteRenderer>(); life = maxLifetime; }
    void Update()
    {
        if (GameState.IsPaused) return;
        if ((life -= Time.deltaTime) <= 0f) Destroy(gameObject);
        if (flashTime > 0f) { flashTime -= Time.deltaTime; visual.color = flashTime > 0 ? Color.white * 2f : baseColor; }
    }
    public void Init(AsteroidSize newSize, Vector2 direction)
    {
        size = newSize;
        int rank = (int)size;
        bool dense = Random.value < 0.25f;
        health = 1 + rank * 2 + (dense ? 1 : 0);
        expValue = 2 + rank * 2 + (dense ? 1 : 0);
        moveSpeed = (3.2f - rank * 0.65f) * Random.Range(0.85f, 1.15f) * (dense ? 0.8f : 1f);
        moveSpeed *= AsteroidSpawner.Instance ? AsteroidSpawner.Instance.globalSpeedMultiplier : 1f;
        
        if (variants != null && variants.Length > 0) visual.sprite = variants[Random.Range(0, variants.Length)];
        float diameter = 1.1f + rank * 0.85f;
        float spriteSpan = Mathf.Max(visual.sprite.bounds.size.x, visual.sprite.bounds.size.y);
        transform.localScale = Vector3.one * (diameter / Mathf.Max(0.01f, spriteSpan));
        FitColliderToSprite();
        baseColor = dense ? new Color(0.62f, 0.68f, 0.76f) : new Color(0.86f, 0.88f, 0.94f);
        visual.color = baseColor;
        rb.linearVelocity = direction.normalized * moveSpeed;
        rb.angularVelocity = Random.Range(-65f, 65f);
    }
    public void FitColliderToSprite()
    {
        var polygon = GetComponent<PolygonCollider2D>();
        if (!polygon) polygon = gameObject.AddComponent<PolygonCollider2D>();
        foreach (var collider in GetComponents<Collider2D>()) if (collider != polygon) collider.enabled = false;
        Sprite sprite = visual ? visual.sprite : GetComponent<SpriteRenderer>().sprite;
        int count = sprite.GetPhysicsShapeCount();
        polygon.offset = Vector2.zero;
        if (count > 0)
        {
            polygon.pathCount = count;
            var points = new List<Vector2>();
            for (int i = 0; i < count; i++)
            {
                sprite.GetPhysicsShape(i, points);
                for (int j = 0; j < points.Count; j++) points[j] *= 0.94f;
                polygon.SetPath(i, points);
            }
        }
        else
        {
            // Tight sprite meshes supply silhouette points when no physics outline is authored.
            var points = new List<Vector2>(sprite.vertices);
            points.Sort((a, b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
            var hull = new List<Vector2>();
            foreach (var point in points)
            {
                while (hull.Count >= 2 && Cross(hull[hull.Count - 1] - hull[hull.Count - 2], point - hull[hull.Count - 1]) <= 0) hull.RemoveAt(hull.Count - 1);
                hull.Add(point);
            }
            int lower = hull.Count;
            for (int i = points.Count - 2; i >= 0; i--)
            {
                while (hull.Count > lower && Cross(hull[hull.Count - 1] - hull[hull.Count - 2], points[i] - hull[hull.Count - 1]) <= 0) hull.RemoveAt(hull.Count - 1);
                hull.Add(points[i]);
            }
            if (hull.Count > 1) hull.RemoveAt(hull.Count - 1);
            for (int i = 0; i < hull.Count; i++) hull[i] *= 0.94f;
            polygon.pathCount = 1; polygon.SetPath(0, hull);
        }
    }
    static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
    public void TakeDamage(int damage)
    {
        if (destroyed || GameState.IsDead) return;
        health -= damage; flashTime = 0.08f;
        if (health > 0) return;
        destroyed = true;
        totalDestroyed++;
        SoundManager.Instance?.PlayAsteroidDestroySound();
        CombatFeedback.Instance?.Burst(transform.position, new Color(0.8f, 0.85f, 0.95f), 10 + (int)size * 5, 3f);
        if (size != AsteroidSize.Small) CombatFeedback.Instance?.Shake(0.05f, 0.1f);
        ExperiencePickup.Drop(pickupPrefab, transform.position, expValue);
        AsteroidSpawner.Instance?.SplitAsteroid(this);
        Destroy(gameObject);
    }
}

