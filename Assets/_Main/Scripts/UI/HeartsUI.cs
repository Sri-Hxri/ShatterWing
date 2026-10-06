using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HeartsUI : MonoBehaviour
{
    public PlayerHealth playerHealth;

    [Header("UI")]
    public Image heartPrefab;
    public Transform heartContainer;

    List<Image> hearts = new List<Image>();

    void Start()
    {
        playerHealth.OnHeartsChanged += UpdateHearts;
        BuildHearts(playerHealth.Hearts);
    }

    void OnDestroy()
    {
        if (playerHealth) playerHealth.OnHeartsChanged -= UpdateHearts;
    }

    void UpdateHearts(int count)
    {
        BuildHearts(count);
    }

    void BuildHearts(int count)
    {
        foreach (var h in hearts)
            Destroy(h.gameObject);

        hearts.Clear();

        for (int i = 0; i < count; i++)
        {
            Image heart = Instantiate(heartPrefab, heartContainer);
            hearts.Add(heart);
        }
    }
}
