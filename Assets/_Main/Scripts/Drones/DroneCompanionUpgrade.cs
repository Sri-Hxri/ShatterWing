using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Drones/Drone Companion")]
public class DroneCompanionUpgrade : UpgradeData
{
    public override void Apply(GameObject player)
    {
        DroneManager dm = player.GetComponent<DroneManager>();
        if (dm != null)
        {
            dm.AddDrone();
        }
    }
}
