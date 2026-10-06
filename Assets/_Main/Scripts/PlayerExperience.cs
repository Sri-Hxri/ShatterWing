using UnityEngine;
using System;

public class PlayerExperience : MonoBehaviour
{
    public int level = 1, currentExp, expToNextLevel = 12;
    public float expGrowthMultiplier = 1.18f, expMultiplier = 1f, expRequirementMultiplier = 1f;
    public float pickupRadius = 3.5f;
    float fractionalExp;
    public event Action<int, int> OnExpChanged;
    public event Action<int> OnLevelUp;
    void Start() { OnExpChanged?.Invoke(currentExp, expToNextLevel); }
    public void GainExp(int amount)
    {
        if (GameState.IsDead) return;
        if (amount <= 0) return;
        fractionalExp += amount * expMultiplier;
        int awarded = Mathf.FloorToInt(fractionalExp + 0.0001f);
        fractionalExp -= awarded;
        currentExp += awarded;
        while (currentExp >= expToNextLevel)
        {
            currentExp -= expToNextLevel;
            level++;
            // Recompute from the base curve; discounts do not compound each level.
            expToNextLevel = Mathf.Max(12, Mathf.RoundToInt((12f + 5f * (level - 1) + 0.65f * (level - 1) * (level - 1)) * expRequirementMultiplier));
            if (level % 5 == 0) GetComponent<PlayerHealth>()?.AddHeart();
            OnLevelUp?.Invoke(level);
            SceneEffects.Instance?.PlayLevelUpSequence();
        }
        OnExpChanged?.Invoke(currentExp, expToNextLevel);
    }
}
