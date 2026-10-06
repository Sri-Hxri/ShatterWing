using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
public class FlightHUD : MonoBehaviour
{
    public TMP_Text timerText, sectorText, dashText;
    public Image dashFill;
    public GameObject pausePanel;
    public PlayerController player;
    bool manualPause;
    bool dashWasReady = true;
    float readyPulse;
    void Update()
    {
        float t = RunTimer.Instance ? RunTimer.Instance.GetTime() : 0;
        timerText.text = $"{(int)t / 60:00}:{(int)t % 60:00}";
        sectorText.text = EnemyDirector.Instance ? EnemyDirector.Instance.Sector : "FIRST CONTACT";
        float cooldown = player.DashCooldownNormalized();
        dashFill.fillAmount = 1f - cooldown;
        dashText.text = cooldown <= 0 ? "DASH / RMB" : "RECHARGING";
        bool ready = cooldown <= 0;
        if (ready && !dashWasReady) readyPulse = 1f;
        dashWasReady = ready;
        readyPulse = Mathf.MoveTowards(readyPulse, 0f, Time.unscaledDeltaTime * 2f);
        dashText.color = ready ? Color.Lerp(CombatFeedback.Cyan, Color.white, readyPulse) : new Color(0.62f, 0.69f, 0.76f);
        dashFill.color = ready ? Color.Lerp(CombatFeedback.Cyan, Color.white, readyPulse) : CombatFeedback.Cyan;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && !GameState.IsDead)
        {
            if (manualPause) Resume();
            else if (!GameState.IsPaused) { manualPause = true; player.weapon.SetFiring(false); GameState.Pause(true); pausePanel.SetActive(true); }
        }
    }
    public void Resume() { manualPause = false; pausePanel.SetActive(false); GameState.Pause(false); }
    public void Menu() { GameState.ResetRun(); UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu"); }
}
