using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Add Heart")]
public class AddHeartUpgrade : UpgradeData
{
    public int heartsToAdd = 1;

    public override void Apply(GameObject player)
    {
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null)
            health.AddHeart(heartsToAdd);
    }
}
