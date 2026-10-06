using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Progression/Greedy Core")]
public class GreedyCoreUpgrade : UpgradeData
{
    public float expBonus = 0.3f;
    public float asteroidSpeedIncrease = 0.15f;

    public override void Apply(GameObject player)
    {
        PlayerExperience exp = player.GetComponent<PlayerExperience>();
        if (exp != null)
        {
            exp.expMultiplier += expBonus;
        }

        AsteroidSpawner.Instance.globalSpeedMultiplier += asteroidSpeedIncrease;
    }
}
