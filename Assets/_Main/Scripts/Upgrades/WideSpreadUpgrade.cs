using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Weapon/Wide Spread")]
public class WideSpreadUpgrade : UpgradeData
{
    public float spreadIncrease = 4f;

    public override void Apply(GameObject player)
    {
        WeaponController weapon = player.GetComponent<WeaponController>();
        if (weapon != null)
        {
            weapon.sweepRank = Mathf.Min(3, weapon.sweepRank + 1);
        }
    }
}

