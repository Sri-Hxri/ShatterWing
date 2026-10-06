using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Drones/Drone Fire Rate")]
public class DroneFireRateUpgrade : UpgradeData
{
    public float bonus = 0.5f;

    public override void Apply(GameObject player)
    {
        DroneManager dm = player.GetComponent<DroneManager>();
        if (dm == null) return;

        dm.fireBonus += bonus;
        dm.Sync();
    }
}

