using UnityEngine;
[CreateAssetMenu(menuName = "Upgrades/Movement/Thruster Boost")]
public class ThrusterBoostUpgrade : UpgradeData
{
    public float forceIncrease = 3f;
    public override void Apply(GameObject player) { player.GetComponent<PlayerController>().moveForceBonus += forceIncrease; }
}
