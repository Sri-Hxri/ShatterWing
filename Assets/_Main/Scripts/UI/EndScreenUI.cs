using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class EndScreenUI : MonoBehaviour
{
    public static EndScreenUI Instance;

    [Header("Texts")]
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI asteroidsText;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(int level, float timeSurvived, int asteroidsDestroyed)
    {
        gameObject.SetActive(true);
        GameState.EndRun();

        levelText.text = $"LEVEL REACHED: {level}";
        timeText.text = $"TIME SURVIVED: {FormatTime(timeSurvived)}";
        asteroidsText.text = $"ASTEROIDS: {asteroidsDestroyed}   /   SHIPS: {EnemyShip.TotalDestroyed}";
    }

    public void Retry()
    {
        GameState.ResetRun();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu()
    {
        GameState.ResetRun();
        SceneManager.LoadScene("MainMenu");
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}

