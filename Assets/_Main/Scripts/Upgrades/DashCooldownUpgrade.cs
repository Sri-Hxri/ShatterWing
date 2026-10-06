using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Dash/Dash Cooldown Reduction")]
public class DashCooldownUpgrade : UpgradeData
{
    public float reduction = 0.5f; // seconds

    public override void Apply(GameObject player)
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.dashCooldownBonus += reduction;
        }
    }
}
