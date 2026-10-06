using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SceneEffects : MonoBehaviour
{
    public static SceneEffects Instance;
    public Image flashImage;
    int pending;
    Coroutine sequence;
    void Awake() { Instance = this; }
    public void PlayLevelUpSequence()
    {
        if (GameState.IsDead) return;
        pending++;
        if (sequence == null) sequence = StartCoroutine(OpenNext());
    }
    IEnumerator OpenNext()
    {
        GameState.Pause(true);
        FindAnyObjectByType<WeaponController>()?.SetFiring(false);
        SoundManager.Instance?.PlayUpgradePopup();
        CombatFeedback.Instance?.Flash(CombatFeedback.Cyan, 0.1f);
        yield return new WaitForSecondsRealtime(0.25f);
        sequence = null;
        if (!GameState.IsDead) UpgradeUI.Instance.ShowUpgrades();
    }
    public void OnUpgradeClosed()
    {
        pending = Mathf.Max(0, pending - 1);
        if (GameState.IsDead) return;
        if (pending > 0) sequence = StartCoroutine(OpenNext());
        else GameState.Pause(false);
    }
}
