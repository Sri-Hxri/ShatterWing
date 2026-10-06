using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float baseFireRate = 6f;
    public int baseBulletCount = 1;
    public float fireRateBonus;
    public int extraBullets;
    public float spreadAngle = 8f, minSpreadAngle = 2f;
    public int bulletPierce;
    public bool splitShot;
    public float baseBulletSpeed = 15f, bulletSpeedBonus;
    public int focusRank, sweepRank, chargeRank;
    public bool dashVolley;
    float fireCooldown;
    int volleys;
    bool isFiring;
    public int PelletCount => Mathf.Min(7, baseBulletCount + extraBullets + (sweepRank > 0 ? 2 : 0));
    // Extra pellets increase coverage without multiplying sustained damage unchecked.
    public float EffectiveFireRate => Mathf.Clamp(baseFireRate + fireRateBonus, 1f, 10f)
        / Mathf.Sqrt(PelletCount) * (focusRank > 0 ? 0.65f : 1f);
    public float EffectiveSpread => focusRank > 0 ? 2f : sweepRank > 0 ? 28f : Mathf.Max(minSpreadAngle, spreadAngle);
    public float EffectiveRange => sweepRank > 0 ? 11f + sweepRank * 2f : 32f;
    void Update()
    {
        if (GameState.IsPaused) return;
        fireCooldown = Mathf.Max(0f, fireCooldown - Time.deltaTime);
        if (isFiring && fireCooldown <= 0f) { Shoot(); fireCooldown = 1f / EffectiveFireRate; }
    }
    public void SetFiring(bool firing) { isFiring = firing; }
    void Shoot()
    {
        SoundManager.Instance?.PlayPlayerShoot();
        CombatFeedback.Instance?.Burst(firePoint.position, CombatFeedback.Cyan, 3, 1.6f);
        volleys++;
        bool charged = chargeRank > 0 && volleys % Mathf.Max(2, 5 - chargeRank) == 0;
        for (int i = 0; i < PelletCount; i++)
        {
            float angle = PelletCount == 1 ? 0 : Mathf.Lerp(-EffectiveSpread, EffectiveSpread, i / (float)(PelletCount - 1));
            SpawnBullet(angle, (focusRank > 0 ? 2 : 1) + (charged ? 1 : 0), EffectiveRange, charged);
        }
    }
    public void FireDashVolley()
    {
        if (!dashVolley || GameState.IsPaused) return;
        for (int i = -2; i <= 2; i++) SpawnBullet(i * 14f, 1, 12f, false, false);
    }
    void SpawnBullet(float angle, int damage, float range, bool charged, bool modifiers = true)
    {
        var shot = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation * Quaternion.Euler(0, 0, angle)).GetComponent<Bullet>();
        float speed = baseBulletSpeed + bulletSpeedBonus;
        shot.damage = damage;
        shot.pierceCount = modifiers ? Mathf.Min(4, bulletPierce + (focusRank >= 2 ? 1 : 0)) : 0;
        shot.canSplit = modifiers && splitShot;
        shot.SetSpeed(speed);
        shot.lifeTime = range / speed;
        if (charged)
        {
            shot.transform.localScale *= 1.35f;
            shot.GetComponent<SpriteRenderer>().color = new Color(1f, 0.85f, 0.4f);
        }
    }
}
