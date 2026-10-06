using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] int hearts = 3;
    public float invincibleTime = 1f;
    public int Hearts => hearts;
    public const int MaxHearts = 8;
    public event Action<int> OnHeartsChanged;
    public event Action OnPlayerDied;
    float safeUntil;
    SpriteRenderer[] visuals;
    void Awake() { visuals = GetComponentsInChildren<SpriteRenderer>(); }
    void Start() { OnHeartsChanged?.Invoke(hearts); }
    void Update()
    {
        float alpha = Time.time < safeUntil ? 0.45f + 0.55f * Mathf.Abs(Mathf.Sin(Time.time * 30f)) : 1f;
        foreach (var visual in visuals) { var c = visual.color; c.a = alpha; visual.color = c; }
    }
    public void GrantInvulnerability(float seconds) { safeUntil = Mathf.Max(safeUntil, Time.time + seconds); }
    public void TakeHit()
    {
        if (GameState.IsPaused || GameState.IsDead || Time.time < safeUntil) return;
        hearts = Mathf.Max(0, hearts - 1);
        GrantInvulnerability(invincibleTime);
        SoundManager.Instance?.PlayHitSound();
        CombatFeedback.Instance?.Burst(transform.position, CombatFeedback.Coral, 22, 5f);
        CombatFeedback.Instance?.Shake(0.22f, 0.25f);
        CombatFeedback.Instance?.Flash(CombatFeedback.Coral, 0.12f);
        OnHeartsChanged?.Invoke(hearts);
        if (hearts == 0)
        {
            GameState.EndRun();
            OnPlayerDied?.Invoke();
            EndScreenUI.Instance.Show(GetComponent<PlayerExperience>().level, RunTimer.Instance.GetTime(), Asteroid.totalDestroyed);
        }
    }
    public void AddHeart(int amount = 1)
    {
        hearts = Mathf.Min(MaxHearts, hearts + amount);
        OnHeartsChanged?.Invoke(hearts);
    }
}
