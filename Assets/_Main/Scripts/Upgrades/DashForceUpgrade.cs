using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Dash/Dash Force")]
public class DashForceUpgrade : UpgradeData
{
    public float bonus = 5f;

    public override void Apply(GameObject player)
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.dashForceBonus += bonus; pc.weapon.dashVolley = true;
        }
    }
}
