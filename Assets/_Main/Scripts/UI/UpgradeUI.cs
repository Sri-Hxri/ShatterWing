using UnityEngine;
using System.Collections.Generic;

public class UpgradeUI : MonoBehaviour
{
    public static UpgradeUI Instance;

    [Header("UI")]
    public GameObject panel;
    public UpgradeButtonUI[] buttons;

    GameObject player;
    bool isOpen = false;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void ShowUpgrades()
    {
        if (isOpen || GameState.IsDead)
            return;

        // Resolve player safely ONCE
        PlayerController pc = FindAnyObjectByType<PlayerController>();
        if (pc == null)
        {
            Debug.LogError("UpgradeUI: PlayerController not found!");
            return;
        }

        player = pc.gameObject;

        List<UpgradeData> upgrades =
            UpgradeManager.Instance.GetRandomUpgrades(3);

        if (upgrades == null || upgrades.Count == 0)
        { SceneEffects.Instance.OnUpgradeClosed(); return; }

        isOpen = true;
        GameState.Pause(true);
        panel.SetActive(true);

        // Disable all buttons first
        for (int i = 0; i < buttons.Length; i++)
            buttons[i].gameObject.SetActive(false);

        // Setup only valid upgrades
        for (int i = 0; i < upgrades.Count && i < buttons.Length; i++)
        {
            buttons[i].gameObject.SetActive(true);
            buttons[i].Setup(upgrades[i], player);
        }
    }

    public void Close()
    {
        if (!isOpen)
            return;

        isOpen = false;
        // SceneEffects drains queued choices before resuming.
        panel.SetActive(false);

        SceneEffects.Instance?.OnUpgradeClosed();
    }
}

