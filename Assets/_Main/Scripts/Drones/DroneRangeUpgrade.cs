using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Drones/Drone Range")]
public class DroneRangeUpgrade : UpgradeData
{
    public float bonus = 1.5f;

    public override void Apply(GameObject player)
    {
        DroneManager dm = player.GetComponent<DroneManager>();
        if (dm == null) return;

        dm.rangeBonus += bonus;
        dm.Sync();
    }
}

