using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Movement/Inertia Control")]
public class InertiaControlUpgrade : UpgradeData
{
    public float dampingIncrease = 0.01f;

    public override void Apply(GameObject player)
    {
        var pc = player.GetComponent<PlayerController>(); pc.dampingBonus = Mathf.Min(0.05f, pc.dampingBonus + dampingIncrease);
    }
}

