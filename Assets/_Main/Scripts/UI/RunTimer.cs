using UnityEngine;

public class RunTimer : MonoBehaviour
{
    public static RunTimer Instance;
    float timer;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        timer += Time.deltaTime;
    }

    public float GetTime()
    {
        return timer;
    }
}
