using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Drones/Twin Fire")]
public class DroneMultishotUpgrade : UpgradeData
{
    public int bullets = 1;

    public override void Apply(GameObject player)
    {
        DroneManager dm = player.GetComponent<DroneManager>();
        if (dm == null) return;

        dm.extraBullets += bullets;
        dm.Sync();
    }
}

