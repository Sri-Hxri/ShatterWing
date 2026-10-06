using UnityEngine;
public class EnemyDirector : MonoBehaviour
{
    public static EnemyDirector Instance;
    public EnemyShip[] enemies;
    public Material lineMaterial;
    public Vector2 Center { get; private set; }
    public string Sector => elapsed < 60 ? "01 / FIRST CONTACT" : elapsed < 150 ? "02 / CROSS FIRE" : "03 / DEEP SPACE";
    float elapsed, timer = 8f;
    Camera cam;
    LineRenderer border;
    void Awake() { Instance = this; cam = Camera.main; Center = cam.transform.position; }
    void Start()
    {
        border = new GameObject("Arena perimeter").AddComponent<LineRenderer>(); border.transform.SetParent(transform);
        border.sharedMaterial = lineMaterial; border.useWorldSpace = true; border.loop = true; border.positionCount = 4;
        border.startWidth = border.endWidth = 0.025f;
        border.startColor = border.endColor = new Color(0.25f, 0.4f, 0.52f, 0.5f); border.sortingOrder = -5;
    }
    void Update()
    {
        float w = cam.orthographicSize * cam.aspect - 0.25f, h = cam.orthographicSize - 0.25f;
        border.SetPosition(0, Center + new Vector2(-w, -h + 0.7f)); border.SetPosition(1, Center + new Vector2(w, -h + 0.7f));
        border.SetPosition(2, Center + new Vector2(w, h - 2.2f)); border.SetPosition(3, Center + new Vector2(-w, h - 2.2f));
        if (GameState.IsPaused) return;
        elapsed += Time.deltaTime;
        if ((timer -= Time.deltaTime) > 0) return;
        timer = Mathf.Max(2.5f, 6f - elapsed / 60f) * (elapsed % 60f >= 48f ? 1.6f : 1f);
        if (EnemyShip.Active.Count >= 10) return;
        int unlocked = elapsed < 25 ? 1 : elapsed < 55 ? 2 : elapsed < 90 ? 3 : 4;
        int index = Random.Range(0, Mathf.Min(unlocked, enemies.Length));
        var enemy = Instantiate(enemies[index], AsteroidSpawner.SpawnPosition(cam, Center, 1f), Quaternion.identity);
        enemy.health += Mathf.Min(3, Mathf.FloorToInt(elapsed / 150f));
    }
}


