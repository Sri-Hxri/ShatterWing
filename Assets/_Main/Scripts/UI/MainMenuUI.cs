using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void startGame()
    {
        GameState.ResetRun();
        SceneManager.LoadScene("GameScene");
    }

    public void Quitgame()
    {
        Application.Quit();
    }
}

