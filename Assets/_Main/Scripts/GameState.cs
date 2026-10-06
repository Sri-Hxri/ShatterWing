using UnityEngine;

// One owner for simulation time. Feedback animations use unscaled time.
public static class GameState
{
    public static bool IsPaused;
    public static bool IsDead;
    public static void ResetRun()
    {
        IsPaused = IsDead = false;
        Asteroid.totalDestroyed = 0;
        EnemyShip.TotalDestroyed = 0;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
    public static void Pause(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused || IsDead ? 0f : 1f;
    }
    public static void EndRun()
    {
        IsDead = true;
        Pause(true);
    }
}
