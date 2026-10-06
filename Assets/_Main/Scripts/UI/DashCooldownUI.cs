using UnityEngine;
using UnityEngine.UI;

public class DashCooldownUI : MonoBehaviour
{
    public PlayerController player;
    public Image cooldownImage;

    public Vector3 offset = new Vector3(0f, -1.2f, 0f);

    void Update()
    {
        if (player == null)
            return;

        if (!player.dashUnlocked)
        {
            cooldownImage.enabled = false;
            return;
        }

        float normalized = player.DashCooldownNormalized();
        cooldownImage.fillAmount = normalized;
        cooldownImage.enabled = normalized > 0f;
    }


}
