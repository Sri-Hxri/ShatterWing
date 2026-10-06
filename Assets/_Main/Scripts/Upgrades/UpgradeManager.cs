using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using static UpgradeData;
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;
    public List<UpgradeData> allUpgrades;
    public int commonWeight = 65, rareWeight = 28, epicWeight = 7;
    public GameObject player;
    readonly Dictionary<UpgradeData, int> selected = new Dictionary<UpgradeData, int>();
    void Awake() { Instance = this; }
    public int Rank(UpgradeData upgrade) => selected.TryGetValue(upgrade, out var n) ? n : 0;
    public void Record(UpgradeData upgrade) { selected[upgrade] = Rank(upgrade) + 1; }
    public List<UpgradeData> GetRandomUpgrades(int count)
    {
        if (!player) player = GameObject.FindGameObjectWithTag("Player");
        var result = new List<UpgradeData>();
        if (!player) return result;
        var pool = allUpgrades.Where(u => u && IsEligible(u)).ToList();
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            var candidates = i == 0 ? pool.Where(IsCombat).ToList() : i == 1 ? pool.Where(u => !IsCombat(u)).ToList() : pool;
            if (candidates.Count == 0) candidates = pool;
            int total = candidates.Sum(Weight), roll = Random.Range(0, total);
            UpgradeData chosen = candidates[0];
            foreach (var entry in candidates) { roll -= Weight(entry); if (roll < 0) { chosen = entry; break; } }
            result.Add(chosen);
            // Do not offer two tiers of the same behavior in one draft.
            pool.RemoveAll(u => u.GetType() == chosen.GetType());
        }
        return result;
    }
    static bool IsCombat(UpgradeData u) => u.category == UpgradeCategory.Weapon || u.category == UpgradeCategory.Drone;
    int Weight(UpgradeData u) => u.rarity == UpgradeRarity.Common ? commonWeight : u.rarity == UpgradeRarity.Rare ? rareWeight : epicWeight;
    public bool IsEligible(UpgradeData u)
    {
        if (!u || !player) return false;
        var pc = player.GetComponent<PlayerController>(); var w = pc.weapon;
        var dm = player.GetComponent<DroneManager>(); var xp = player.GetComponent<PlayerExperience>();
        if (xp.level < u.minLevel || Rank(u) >= u.maxRanks) return false;
        if (allUpgrades.Any(other => other && other.GetType() == u.GetType() && other.rarity < u.rarity && Rank(other) == 0)) return false;
        if (u is DroneCompanionUpgrade) return dm.GetDrones().Count < dm.maxDrones;
        if (u.category == UpgradeCategory.Drone && dm.GetDrones().Count == 0) return false;
        if (u is PhaseDashUpgrade) return !pc.phaseDash;
        if (u is SplitShotUpgrade) return !w.splitShot;
        if (u is AddHeartUpgrade) return player.GetComponent<PlayerHealth>().Hearts < PlayerHealth.MaxHearts;
        if (u is FocusedShotUpgrade) return w.sweepRank == 0 && w.focusRank < 2;
        if (u is WideSpreadUpgrade) return w.focusRank == 0 && w.sweepRank < 3;
        if (u is RapidFireUpgrade) return w.baseFireRate + w.fireRateBonus < 10f;
        if (u is ExtraBulletUpgrade) return w.extraBullets < 6;
        if (u is PiercingUpgrade) return w.bulletPierce < 4;
        if (u is DashCooldownUpgrade) return pc.dashCooldown - pc.dashCooldownBonus > 1.2f;
        if (u is InertiaControlUpgrade) return pc.dampingBonus < 0.05f;
        if (u is VeteranPilotUpgrade) return xp.expRequirementMultiplier > 0.8f;
        return true;
    }
}
