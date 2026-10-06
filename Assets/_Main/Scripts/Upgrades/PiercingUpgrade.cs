using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Bullet/Piercing")]
public class PiercingUpgrade : UpgradeData
{
    public int amount = 1;

    public override void Apply(GameObject player)
    {
        var weapon = player.GetComponent<WeaponController>(); weapon.bulletPierce = Mathf.Min(4, weapon.bulletPierce + amount);
    }
}

