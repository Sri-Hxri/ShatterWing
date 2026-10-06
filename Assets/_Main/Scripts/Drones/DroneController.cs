using UnityEngine;
public class DroneController : MonoBehaviour
{
    public float orbitRadius = 2.2f, orbitSpeed = 60f, range = 10f, fireRate = 1.5f;
    public GameObject bulletPrefab;
    public float fireRateBonus, rangeBonus, bulletSpeedBonus, baseAngle;
    public int extraBullets;
    Transform player;
    float fireTimer;
    void Start() { player = FindAnyObjectByType<PlayerController>().transform; }
    void Update()
    {
        if (GameState.IsPaused || !player) return;
        transform.position = player.position + Quaternion.Euler(0, 0, Time.time * orbitSpeed + baseAngle) * Vector3.up * orbitRadius;
        if ((fireTimer -= Time.deltaTime) > 0) return;
        fireTimer = 0.2f;
        Transform target = null; float nearest = range + rangeBonus;
        foreach (var enemy in EnemyShip.Active)
        {
            float d = Vector2.Distance(transform.position, enemy.transform.position);
            if (d < nearest) { nearest = d; target = enemy.transform; }
        }
        if (!target) foreach (var rock in FindObjectsByType<Asteroid>())
        {
            float d = Vector2.Distance(transform.position, rock.transform.position);
            if (d < nearest) { nearest = d; target = rock.transform; }
        }
        if (!target) return;
        Vector2 dir = target.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        int count = Mathf.Min(3, 1 + extraBullets);
        for (int i = 0; i < count; i++)
        {
            var shot = Instantiate(bulletPrefab, transform.position, Quaternion.Euler(0, 0, angle + (i - (count - 1) * 0.5f) * 12f)).GetComponent<Bullet>();
            shot.SetSpeed(30 + bulletSpeedBonus);
            shot.pierceCount = bulletSpeedBonus > 0 ? 1 : 0;
            shot.lifeTime = (range + rangeBonus + 2f) / (30f + bulletSpeedBonus);
        }
        CombatFeedback.Instance?.Burst(transform.position, CombatFeedback.Cyan, 2, 1f);
        fireTimer = Mathf.Sqrt(count) / Mathf.Min(3f, fireRate + fireRateBonus);
    }
}

