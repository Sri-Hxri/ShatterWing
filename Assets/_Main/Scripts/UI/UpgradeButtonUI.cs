using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeButtonUI : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public TextMeshProUGUI rarityText;
    public Button button;

    UpgradeData upgrade;
    GameObject player;

    public void Setup(UpgradeData data, GameObject playerObj)
    {
        upgrade = data;
        button.interactable = true;
        player = playerObj;

        icon.sprite = data.icon;
        title.text = data.upgradeName;
        icon.preserveAspect = true;
        description.text = data.description;

        Color accent = data.rarity == UpgradeData.UpgradeRarity.Epic ? new Color(0.78f, 0.56f, 1f) : data.rarity == UpgradeData.UpgradeRarity.Rare ? new Color(0.35f, 0.72f, 1f) : new Color(0.70f, 0.78f, 0.83f);
        Color surface = data.rarity == UpgradeData.UpgradeRarity.Epic ? new Color(0.20f, 0.10f, 0.29f) : data.rarity == UpgradeData.UpgradeRarity.Rare ? new Color(0.055f, 0.17f, 0.29f) : new Color(0.13f, 0.17f, 0.20f);
        var colors = button.colors;
        colors.normalColor = surface; colors.highlightedColor = Color.Lerp(surface, accent, 0.3f);
        colors.selectedColor = colors.highlightedColor; colors.pressedColor = Color.Lerp(surface, accent, 0.5f);
        colors.disabledColor = surface; colors.colorMultiplier = 1f; button.colors = colors;
        if (button.targetGraphic) { button.targetGraphic.color = Color.white; button.targetGraphic.CrossFadeColor(surface, 0f, true, true); }
        var outline = GetComponent<Outline>(); if (!outline) outline = gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(accent.r, accent.g, accent.b, 0.85f); outline.effectDistance = new Vector2(2, -2);
        if (rarityText != null)
        {
            rarityText.text = data.rarity.ToString().ToUpper();
            rarityText.color = data.rarity == UpgradeData.UpgradeRarity.Epic ? new Color(0.8f, 0.65f, 1f) : data.rarity == UpgradeData.UpgradeRarity.Rare ? CombatFeedback.Cyan : new Color(0.7f, 0.78f, 0.85f);
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ApplyUpgrade);
    }

    void ApplyUpgrade()
    {
        if (GameState.IsDead || !button.interactable) return;
        button.interactable = false;
        upgrade.Apply(player);
        UpgradeManager.Instance.Record(upgrade);
        SoundManager.Instance?.PlayUpgradeConfirm();
        CombatFeedback.Instance?.Burst(player.transform.position, CombatFeedback.Cyan, 12, 3f);
        UpgradeUI.Instance.Close();
    }
}



