using UnityEngine;
using UnityEngine.UI;

public class ExpBarUI : MonoBehaviour
{
    public PlayerExperience playerExp;
    public Image fillImage;
    float target;
    int shownLevel;
    void Start()
    {
        shownLevel = playerExp.level;
        playerExp.OnExpChanged += UpdateBar;
        UpdateBar(playerExp.currentExp, playerExp.expToNextLevel);
        fillImage.fillAmount = target;
    }
    void Update()
    {
        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, target, 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime));
    }
    void OnDestroy() { if (playerExp) playerExp.OnExpChanged -= UpdateBar; }
    void UpdateBar(int current, int required)
    {
        target = Mathf.Clamp01(current / (float)Mathf.Max(1, required));
        if (shownLevel != playerExp.level) { shownLevel = playerExp.level; fillImage.fillAmount = 0f; }
    }
}
