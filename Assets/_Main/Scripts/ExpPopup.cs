using UnityEngine;
using TMPro;

public class ExpPopup : MonoBehaviour
{
    public TextMeshProUGUI text; // drag manually
    public float floatSpeed = 1f;
    public float lifetime = 1f;

    float timer;

    public void Init(int expAmount)
    {
        text.text = $"+{expAmount} EXP";
        timer = lifetime;
    }

    void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        timer -= Time.deltaTime;
        if (timer <= 0f)
            Destroy(gameObject);
    }
}
