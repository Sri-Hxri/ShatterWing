using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Weapon/Rapid Fire")]
public class RapidFireUpgrade : UpgradeData
{
    public float fireRateBonus = 1.5f;

    public override void Apply(GameObject player)
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        pc.weapon.fireRateBonus = Mathf.Min(10f - pc.weapon.baseFireRate, pc.weapon.fireRateBonus + fireRateBonus);
    }
}

