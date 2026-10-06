using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Movement/Phase Dash")]
public class PhaseDashUpgrade : UpgradeData
{
    public override void Apply(GameObject player)
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.phaseDash = true;
        }
    }

}

