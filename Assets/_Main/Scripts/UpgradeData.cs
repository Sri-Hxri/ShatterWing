using UnityEngine;

public abstract class UpgradeData : ScriptableObject
{
    [Header("Info")]
    public string upgradeName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Meta")]
    public UpgradeRarity rarity;
    public UpgradeCategory category;

    [Tooltip("Higher = appears later")]
    public int minLevel = 1;
    public int maxRanks = 3;

    public abstract void Apply(GameObject player);

    public enum UpgradeRarity
    {
        Common,
        Rare,
        Epic
    }

    public enum UpgradeCategory
    {
        Weapon,
        Drone,
        Dash,
        Movement,
        Progression,
        Survivability
    }

}

