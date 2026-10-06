using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Drones/Drone Bullet Speed")]
public class DroneBulletSpeedUpgrade : UpgradeData
{
    public float bonus = 3f;

    public override void Apply(GameObject player)
    {
        DroneManager dm = player.GetComponent<DroneManager>();
        if (dm == null) return;

        dm.speedBonus += bonus;
        dm.Sync();
    }
}

