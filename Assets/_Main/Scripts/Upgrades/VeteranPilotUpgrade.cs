using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Progression/Veteran Pilot")]
public class VeteranPilotUpgrade : UpgradeData
{
    public float reduction = 0.1f; // 10%

    public override void Apply(GameObject player)
    {
        PlayerExperience exp = player.GetComponent<PlayerExperience>();
        if (exp != null)
        {
            exp.expRequirementMultiplier =
                Mathf.Clamp(exp.expRequirementMultiplier - reduction, 0.75f, 1f);
        }
    }
}

