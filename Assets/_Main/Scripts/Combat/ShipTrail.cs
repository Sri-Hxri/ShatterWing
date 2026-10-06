using UnityEngine;
public class ShipTrail : MonoBehaviour
{
    public Sprite sprite;
    public Material material;
    ParticleSystem engine;
    Rigidbody2D body;
    PlayerController player;
    void Start()
    {
        body = GetComponent<Rigidbody2D>(); player = GetComponent<PlayerController>();
        var go = new GameObject("Engine wake"); go.transform.SetParent(transform, false); go.transform.localPosition = new Vector3(0, -0.65f, 0);
        engine = go.AddComponent<ParticleSystem>(); engine.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = engine.main; main.startLifetime = 0.25f; main.startSpeed = 0; main.startSize = 0.1f; main.maxParticles = 80; main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startColor = CombatFeedback.Cyan * 1.6f;
        var shape = engine.shape; shape.enabled = false;
        var size = engine.sizeOverLifetime; size.enabled = true; size.size = new ParticleSystem.MinMaxCurve(1, AnimationCurve.Linear(0, 1, 1, 0));
        engine.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
        engine.Play();
    }
    void Update()
    {
        var emission = engine.emission;
        emission.rateOverTime = !GameState.IsPaused && player.IsDashing ? 180 : 0;
    }
}

