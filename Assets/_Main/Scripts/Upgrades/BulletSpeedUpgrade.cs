using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Weapon/Bullet Speed")]
public class BulletSpeedUpgrade : UpgradeData
{
    public float speedBonus = 3f;

    public override void Apply(GameObject player)
    {
        if (player == null)
        {
            Debug.LogError("BulletSpeedUpgrade: Player is null");
            return;
        }

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc == null)
        {
            Debug.LogError("BulletSpeedUpgrade: PlayerController not found");
            return;
        }

        if (pc.weapon == null)
        {
            Debug.LogError("BulletSpeedUpgrade: WeaponController not assigned");
            return;
        }

        pc.weapon.bulletSpeedBonus += speedBonus; pc.weapon.chargeRank++;
    }
}
