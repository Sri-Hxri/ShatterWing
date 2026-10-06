using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Progression/EXP Booster")]
public class ExpBoosterUpgrade : UpgradeData
{
    public float bonus = 0.2f; // +20%

    public override void Apply(GameObject player)
    {
        PlayerExperience exp = player.GetComponent<PlayerExperience>();
        if (exp != null)
        {
            exp.expMultiplier += bonus; exp.pickupRadius += 2.5f;
        }
    }
}
