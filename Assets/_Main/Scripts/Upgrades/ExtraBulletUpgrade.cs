using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Weapon/Extra Bullet")]
public class ExtraBulletUpgrade : UpgradeData
{
    public int bulletsToAdd = 1;

    public override void Apply(GameObject player)
    {
        WeaponController weapon = player.GetComponent<WeaponController>();
        if (weapon != null)
        {
            weapon.extraBullets = Mathf.Min(6, weapon.extraBullets + bulletsToAdd);
        }
    }
}

