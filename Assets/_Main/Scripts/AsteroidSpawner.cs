using UnityEngine;
public class AsteroidSpawner : MonoBehaviour
{
    public static AsteroidSpawner Instance;
    public Asteroid asteroidPrefab;
    public float spawnRadius = 12f, spawnInterval = 1.4f, globalSpeedMultiplier = 1f;
    float timer = 1f, gameTime;
    Camera cam;
    Vector2 center;
    void Awake() { Instance = this; cam = Camera.main; center = cam.transform.position; }
    void Update()
    {
        if (GameState.IsPaused) return;
        gameTime += Time.deltaTime;
        if ((timer -= Time.deltaTime) > 0f) return;
        timer = Mathf.Max(0.85f, spawnInterval - gameTime / 500f);
        if (FindObjectsByType<Asteroid>().Length >= 35) return;
        Vector2 pos = SpawnPosition(cam, center, 1.5f);
        Vector2 target = center + Random.insideUnitCircle * cam.orthographicSize * 0.65f;
        float roll = Random.value;
        AsteroidSize size = gameTime < 20 ? AsteroidSize.Small : (roll < 0.15f && gameTime > 60 ? AsteroidSize.Large : roll < 0.55f ? AsteroidSize.Medium : AsteroidSize.Small);
        Instantiate(asteroidPrefab, pos, Quaternion.identity).Init(size, target - pos);
    }
    public static Vector2 SpawnPosition(Camera cam, Vector2 center, float margin)
    {
        float h = cam.orthographicSize + margin, w = cam.orthographicSize * cam.aspect + margin;
        switch (Random.Range(0, 4))
        {
            case 0: return center + new Vector2(-w, Random.Range(-h, h));
            case 1: return center + new Vector2(w, Random.Range(-h, h));
            case 2: return center + new Vector2(Random.Range(-w, w), h);
            default: return center + new Vector2(Random.Range(-w, w), -h);
        }
    }
    public void SplitAsteroid(Asteroid source)
    {
        if (source.size == AsteroidSize.Small) return;
        for (int i = 0; i < 2; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            Instantiate(asteroidPrefab, source.transform.position + (Vector3)dir * 0.6f, Quaternion.identity).Init((AsteroidSize)((int)source.size - 1), dir);
        }
    }
}


