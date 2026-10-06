using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    public float moveForce = 30f, maxSpeed = 9f, damping = 0.96f;
    public float rotationSpeed = 18f;
    public float moveForceBonus, maxSpeedBonus, dampingBonus;
    public bool dashUnlocked = true;
    public float dashForce = 24f, dashCooldown = 2.5f;
    public float dashForceBonus, dashCooldownBonus;
    public WeaponController weapon;
    public bool IsDashing => dashDuration > 0f;
    public bool phaseDash;
    float dashTimer, dashDuration;
    Rigidbody2D rb;
    Camera cam;
    Vector2 moveInput, mouseScreenPos, dashDirection;
    Vector2 arenaCenter;
    float CurrentMaxSpeed => maxSpeed + maxSpeedBonus;
    float CurrentDashCooldown => Mathf.Max(1.2f, dashCooldown - dashCooldownBonus);
    void Awake()
    {
        GameState.ResetRun();
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        arenaCenter = cam.transform.position;
        if (!weapon) weapon = GetComponent<WeaponController>();
        dashUnlocked = true;
    }
    void Update()
    {
        if (GameState.IsPaused) return;
        dashTimer = Mathf.Max(0f, dashTimer - Time.deltaTime);
        dashDuration = Mathf.Max(0f, dashDuration - Time.deltaTime);
    }
    void FixedUpdate()
    {
        if (GameState.IsPaused) return;
        Vector2 dir = (Vector2)cam.ScreenToWorldPoint(mouseScreenPos) - rb.position;
        if (dir.sqrMagnitude > 0.001f)
            rb.rotation = Mathf.LerpAngle(rb.rotation, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f, rotationSpeed * Time.fixedDeltaTime);
        if (IsDashing) rb.linearVelocity = dashDirection * (dashForce + dashForceBonus);
        else
        {
            rb.AddForce(moveInput.normalized * (moveForce + moveForceBonus));
            float retention = moveInput.sqrMagnitude > 0.01f ? 0.985f : Mathf.Clamp(damping - dampingBonus, 0.88f, 0.99f);
            rb.linearVelocity *= Mathf.Pow(retention, Time.fixedDeltaTime / 0.02f);
            rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, CurrentMaxSpeed);
        }
        ConstrainToArena();
    }
    void LateUpdate() { if (!GameState.IsPaused) ConstrainToArena(); }
    void ConstrainToArena()
    {
        float h = cam.orthographicSize - 0.8f;
        float w = cam.orthographicSize * cam.aspect - 0.8f;
        Vector2 p = rb.position, v = rb.linearVelocity;
        Vector2 bounded = new Vector2(Mathf.Clamp(p.x, arenaCenter.x - w, arenaCenter.x + w), Mathf.Clamp(p.y, arenaCenter.y - h + 0.7f, arenaCenter.y + h - 2.2f));
        if (bounded.x != p.x) v.x = 0;
        if (bounded.y != p.y) v.y = 0;
        rb.position = bounded;
        rb.linearVelocity = v;
    }
    public void Dash()
    {
        if (GameState.IsPaused || !dashUnlocked || dashTimer > 0f) return;
        dashDirection = moveInput.sqrMagnitude > 0.1f ? moveInput.normalized : (Vector2)transform.up;
        dashDuration = 0.18f;
        dashTimer = CurrentDashCooldown;
        GetComponent<PlayerHealth>().GrantInvulnerability(phaseDash ? 0.45f : 0.2f);
        CombatFeedback.Instance?.Burst(transform.position, CombatFeedback.Cyan, 16, 4f);
        CombatFeedback.Instance?.Shake(0.06f, 0.12f);
        weapon.FireDashVolley();
        if (phaseDash)
        {
            foreach (var shot in FindObjectsByType<EnemyProjectile>())
                if (Vector2.Distance(transform.position, shot.transform.position) <= 3.5f) Destroy(shot.gameObject);
            CombatFeedback.Instance?.Burst(transform.position, CombatFeedback.Cyan, 30, 9f);
        }
        SoundManager.Instance?.PlayDashSound();
    }
    public void OnMove(InputAction.CallbackContext ctx) { moveInput = ctx.ReadValue<Vector2>(); }
    public void OnAim(InputAction.CallbackContext ctx) { mouseScreenPos = ctx.ReadValue<Vector2>(); }
    public void OnFire(InputAction.CallbackContext ctx)
    {
        if (ctx.started && !GameState.IsPaused) weapon.SetFiring(true);
        if (ctx.canceled) weapon.SetFiring(false);
    }
    public void OnDash(InputAction.CallbackContext ctx) { if (ctx.started) Dash(); }
    public float DashCooldownNormalized() => Mathf.Clamp01(dashTimer / CurrentDashCooldown);
    public bool IsDashReady() => dashTimer <= 0f;
}

