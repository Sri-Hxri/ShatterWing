using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Movement/Maximum Velocity")]
public class MaxVelocityUpgrade : UpgradeData
{
    public float speedIncrease = 1.5f;

    public override void Apply(GameObject player)
    {
        if (player == null)
        {
            Debug.LogError("MaxVelocityUpgrade: Player is null");
            return;
        }

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc == null)
        {
            Debug.LogError("MaxVelocityUpgrade: PlayerController not found on player");
            return;
        }

        pc.maxSpeedBonus += speedIncrease;
    }
}
