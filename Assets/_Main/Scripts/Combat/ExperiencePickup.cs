using UnityEngine;
public class ExperiencePickup : MonoBehaviour
{
    public int value;
    Transform player;
    PlayerExperience xp;
    float age;
    Vector3 baseScale;
    public static void Drop(ExperiencePickup prefab, Vector3 position, int amount)
    {
        if (!prefab) { FindAnyObjectByType<PlayerExperience>()?.GainExp(amount); return; }
        var cam = Camera.main;
        var center = FindAnyObjectByType<EnemyDirector>();
        Vector2 c = center ? center.Center : Vector2.zero;
        position.x = Mathf.Clamp(position.x, c.x - cam.orthographicSize * cam.aspect + 1, c.x + cam.orthographicSize * cam.aspect - 1);
        position.y = Mathf.Clamp(position.y, c.y - cam.orthographicSize + 1.5f, c.y + cam.orthographicSize - 3f);
        Instantiate(prefab, position, Quaternion.identity).value = amount;
    }
    void Start() { xp = FindAnyObjectByType<PlayerExperience>(); player = xp.transform; baseScale = transform.localScale; }
    void Update()
    {
        if (GameState.IsPaused || !player) return;
        age += Time.deltaTime;
        transform.localScale = baseScale * (1f + 0.12f * Mathf.Sin(age * 5f));
        float distance = Vector2.Distance(transform.position, player.position);
        if (distance < xp.pickupRadius || age > 20f) transform.position = Vector3.MoveTowards(transform.position, player.position, (12f + age) * Time.deltaTime);
        if (distance < 0.7f)
        {
            xp.GainExp(value);
            CombatFeedback.Instance?.Burst(transform.position, CombatFeedback.Cyan, 3, 1.3f);
            Destroy(gameObject);
        }
    }
}

