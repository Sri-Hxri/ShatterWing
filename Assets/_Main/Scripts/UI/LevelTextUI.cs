using UnityEngine;
using TMPro;

public class LevelTextUI : MonoBehaviour
{
    public PlayerExperience playerExp;
    TextMeshProUGUI text;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void Start()
    {
        UpdateLevel(playerExp.level);
        playerExp.OnLevelUp += UpdateLevel;
    }

    void OnDestroy()
    {
        playerExp.OnLevelUp -= UpdateLevel;
    }

    void ResetScale()
    {
        transform.localScale = Vector3.one;
    }

    void UpdateLevel(int level)
    {
        text.text = $"LVL {level}";
        transform.localScale = Vector3.one * 1.2f;
        Invoke(nameof(ResetScale), 0.1f);

    }
}
