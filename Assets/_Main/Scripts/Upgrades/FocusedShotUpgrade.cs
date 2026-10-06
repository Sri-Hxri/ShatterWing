using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Weapon/Focused Shot")]
public class FocusedShotUpgrade : UpgradeData
{
    public float spreadReduction = 3f;

    public override void Apply(GameObject player)
    {
        WeaponController weapon = player.GetComponent<WeaponController>();
        if (weapon != null)
        {
            weapon.focusRank = Mathf.Min(2, weapon.focusRank + 1);
        }
    }
}

