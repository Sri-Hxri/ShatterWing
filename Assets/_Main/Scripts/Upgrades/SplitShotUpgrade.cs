using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Bullet/Split Shot")]
public class SplitShotUpgrade : UpgradeData
{
    public override void Apply(GameObject player)
    {
        player.GetComponent<WeaponController>().splitShot = true;
    }
}
