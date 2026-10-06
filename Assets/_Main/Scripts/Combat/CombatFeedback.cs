using UnityEngine;

public class CombatFeedback : MonoBehaviour
{
    public static CombatFeedback Instance;
    public static readonly Color Cyan = new Color(0.48f, 0.86f, 1f);
    public static readonly Color Coral = new Color(1f, 0.42f, 0.35f);
    public Sprite particleSprite;
    public Material particleMaterial;
    public UnityEngine.UI.Image flash;
    public float shakeScale = 1f;
    ParticleSystem[] pool;
    int cursor;
    Camera cam;
    Vector3 cameraOrigin;
    float shakeLeft, shakeDuration, strength, flashAlpha;
    Color flashColor;
    void Awake()
    {
        Instance = this;
        cam = Camera.main;
        cameraOrigin = cam.transform.position;
        pool = new ParticleSystem[24];
        for (int i = 0; i < pool.Length; i++)
        {
            var go = new GameObject("Reusable sparks " + i);
            go.transform.SetParent(transform);
            var ps = go.AddComponent<ParticleSystem>();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = ps.main;
            main.playOnAwake = false; main.loop = false; main.duration = 0.5f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.12f, 0.35f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.13f);
            main.maxParticles = 48; main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission; emission.enabled = false;
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = 0.08f;
            var size = ps.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0));
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = particleMaterial; renderer.sortingOrder = 20;
            pool[i] = ps;
        }
    }
    public void Burst(Vector3 position, Color color, int count = 8, float speed = 3f)
    {
        var ps = pool[cursor++ % pool.Length];
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.transform.position = position;
        var main = ps.main;
        main.startColor = new Color(color.r * 2.4f, color.g * 2.4f, color.b * 2.4f, 1f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.3f, speed);
        ps.Play(); ps.Emit(Mathf.Clamp(count, 1, 48));
    }
    public void Shake(float amount, float duration)
    {
        strength = Mathf.Max(strength, amount);
        shakeLeft = shakeDuration = Mathf.Max(shakeLeft, duration);
    }
    public void Flash(Color color, float alpha)
    {
        flashColor = color; flashAlpha = Mathf.Max(flashAlpha, alpha);
    }
    void LateUpdate()
    {
        shakeLeft = Mathf.Max(0, shakeLeft - Time.unscaledDeltaTime);
        Vector2 offset = Random.insideUnitCircle * strength * shakeScale * (shakeDuration > 0 ? shakeLeft / shakeDuration : 0);
        cam.transform.position = cameraOrigin + (Vector3)offset;
        if (shakeLeft == 0) strength = 0;
        flashAlpha = Mathf.MoveTowards(flashAlpha, 0f, Time.unscaledDeltaTime * 0.7f);
        if (flash) flash.color = new Color(flashColor.r, flashColor.g, flashColor.b, flashAlpha);
    }
    void OnDestroy() { if (cam) cam.transform.position = cameraOrigin; if (Instance == this) Instance = null; }
}
