using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor.Events;
using System.Linq;
using System.IO;

public static class SpaceShooterPolishSetup
{
    const string Root = "Assets/_Main/";
    static readonly Color Ink = new Color(0.035f, 0.055f, 0.085f, 0.97f);
    static readonly Color Muted = new Color(0.55f, 0.65f, 0.76f);
    static Material sparks;
    static Sprite square;
    static TMP_FontAsset font;
    static Sprite Art(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Art/Space/" + name + ".png");
    [MenuItem("Tools/Space Shooter/Apply combat and presentation update")]
    public static void Apply()
    {
        PlayerSettings.defaultScreenWidth = 1600; PlayerSettings.defaultScreenHeight = 900; PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        foreach (string file in Directory.GetFiles(Root + "Art/Space", "*.png"))
        {
            var importer = AssetImporter.GetAtPath(file.Replace('\\', '/')) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100; importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear; importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        square = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Sprtites/square-128.png");
        if (!square) square = Art("meteor_squareLarge");
        sparks = AssetDatabase.LoadAssetAtPath<Material>(Root + "Art/Spark.mat");
        if (!sparks) { sparks = new Material(Shader.Find("Sprites/Default")); AssetDatabase.CreateAsset(sparks, Root + "Art/Spark.mat"); }
        sparks.mainTexture = Texture2D.whiteTexture;
        var pickup = CreatePickup();
        var projectile = CreateEnemyProjectile();
        var enemies = new EnemyShip[4];
        int[] hp = { 3, 5, 3, 12 }; int[] xp = { 6, 9, 7, 16 }; float[] speed = { 4f, 2.8f, 5.2f, 1.8f }; float[] interval = { 3.6f, 2.8f, 2f, 4.5f };
        for (int i = 0; i < 4; i++)
        {
            var go = new GameObject(((EnemyRole)i).ToString());
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Art(i == 0 ? "Emeny ship 1" : "Enemy ship " + (i + 1)); sr.sortingOrder = 5;
            var rb = go.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic; rb.gravityScale = 0; rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var collider = go.AddComponent<CircleCollider2D>(); collider.radius = 0.48f; collider.isTrigger = true;
            var enemy = go.AddComponent<EnemyShip>(); enemy.role = (EnemyRole)i; enemy.health = hp[i]; enemy.xpValue = xp[i]; enemy.speed = speed[i]; enemy.attackInterval = interval[i]; enemy.projectile = projectile; enemy.pickup = pickup; enemy.barSprite = square;
            if (i == 3) go.transform.localScale = Vector3.one * 1.35f;
            enemies[i] = PrefabUtility.SaveAsPrefabAsset(go, Root + "Prefab/" + (EnemyRole)i + ".prefab").GetComponent<EnemyShip>(); Object.DestroyImmediate(go);
        }
        ConfigurePrefabs(pickup);
        BalanceUpgrades();
        // Reuse the authored sound setup in either entry scene.
        EditorSceneManager.OpenScene(Root + "Scenes/MainMenu.unity");
        var sound = Object.FindAnyObjectByType<SoundManager>();
        var soundPrefab = PrefabUtility.SaveAsPrefabAsset(sound.gameObject, Root + "Prefab/Audio.prefab");
        BuildMenu(); EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.OpenScene(Root + "Scenes/GameScene.unity");
        if (!Object.FindAnyObjectByType<SoundManager>()) PrefabUtility.InstantiatePrefab(soundPrefab);
        var pc = Object.FindAnyObjectByType<PlayerController>();
        pc.moveForce = 30; pc.maxSpeed = 9; pc.damping = 0.96f; pc.rotationSpeed = 18; pc.dashUnlocked = true; pc.dashForce = 24; pc.dashCooldown = 2.5f;
        pc.weapon.baseFireRate = 4; pc.weapon.baseBulletSpeed = 32;
        var body = pc.GetComponent<Rigidbody2D>(); body.linearDamping = 0; body.angularDamping = 0; body.gravityScale = 0;
        var visual = pc.GetComponentInChildren<SpriteRenderer>(); visual.sprite = Art("Player Ship"); visual.color = Color.white; visual.sortingOrder = 10; visual.transform.localScale = Vector3.one;
        var exp = pc.GetComponent<PlayerExperience>(); exp.expToNextLevel = 12; exp.expGrowthMultiplier = 1.18f; exp.expRequirementMultiplier = 1;
        var spawner = Object.FindAnyObjectByType<AsteroidSpawner>(); spawner.spawnInterval = 1.4f;
        if (!pc.GetComponent<ShipTrail>()) { var trail = pc.gameObject.AddComponent<ShipTrail>(); trail.material = sparks; }
        foreach (var existing in Object.FindObjectsByType<EnemyDirector>()) Object.DestroyImmediate(existing.gameObject);
        foreach (var existing in Object.FindObjectsByType<CombatFeedback>()) Object.DestroyImmediate(existing.gameObject);
        var director = new GameObject("Encounter Director").AddComponent<EnemyDirector>(); director.enemies = enemies; director.lineMaterial = sparks;
        var feedback = new GameObject("Combat Feedback").AddComponent<CombatFeedback>(); feedback.particleMaterial = sparks; feedback.particleSprite = Art("Exp points sprite");
        Presentation(); BuildHUD(pc, feedback);
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
        if (Directory.Exists(Root + "Art/UpgradeIconsOriginal") && Directory.GetFiles(Root + "Art/UpgradeIconsOriginal", "*.png").Length == 22) SpaceShooterArtPass.Apply();
        Debug.Log("POLISH_SETUP_COMPLETE");
    }
    static ExperiencePickup CreatePickup()
    {
        var go = new GameObject("Experience mote"); var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Art("Exp points sprite"); sr.color = CombatFeedback.Cyan * 1.4f; sr.sortingOrder = 4;
        go.transform.localScale = Vector3.one * 0.3f; go.AddComponent<ExperiencePickup>();
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, Root + "Prefab/ExperienceMote.prefab"); Object.DestroyImmediate(go); return prefab.GetComponent<ExperiencePickup>();
    }
    static EnemyProjectile CreateEnemyProjectile()
    {
        var go = new GameObject("Hostile shot"); var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Art("star_tiny"); sr.color = CombatFeedback.Coral * 1.8f; sr.sortingOrder = 8; go.transform.localScale = Vector3.one * 0.65f;
        var rb = go.AddComponent<Rigidbody2D>(); rb.bodyType = RigidbodyType2D.Kinematic; rb.gravityScale = 0; rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var c = go.AddComponent<CircleCollider2D>(); c.radius = 0.16f; c.isTrigger = true;
        go.AddComponent<EnemyProjectile>(); var prefab = PrefabUtility.SaveAsPrefabAsset(go, Root + "Prefab/HostileShot.prefab"); Object.DestroyImmediate(go); return prefab.GetComponent<EnemyProjectile>();
    }
    static void ConfigurePrefabs(ExperiencePickup pickup)
    {
        string path = Root + "Prefab/Asteroid.prefab"; var go = PrefabUtility.LoadPrefabContents(path);
        var asteroid = go.GetComponent<Asteroid>(); asteroid.pickupPrefab = pickup; asteroid.maxLifetime = 28;
        asteroid.variants = new[] { Art("meteor_detailedLarge"), Art("meteor_large"), Art("meteor_squareDetailedLarge"), Art("meteor_squareLarge"), Art("meteor_detailedSmall") };
        go.GetComponent<SpriteRenderer>().sprite = asteroid.variants[0];
        PrefabUtility.SaveAsPrefabAsset(go, path); PrefabUtility.UnloadPrefabContents(go);
        foreach (string name in new[] { "Bullet", "DroneBullet" })
        {
            path = Root + "Prefab/" + name + ".prefab"; go = PrefabUtility.LoadPrefabContents(path);
            go.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            go.GetComponent<Rigidbody2D>().gravityScale = 0;
            go.GetComponent<SpriteRenderer>().color = CombatFeedback.Cyan * 1.5f;
            go.GetComponent<Bullet>().speed = 32; go.GetComponent<Bullet>().lifeTime = 2;
            PrefabUtility.SaveAsPrefabAsset(go, path); PrefabUtility.UnloadPrefabContents(go);
        }
        path = Root + "Prefab/Drone.prefab"; go = PrefabUtility.LoadPrefabContents(path);
        go.GetComponent<SpriteRenderer>().sprite = Art("satellite_A"); go.transform.localScale = Vector3.one * 0.55f;
        PrefabUtility.SaveAsPrefabAsset(go, path); PrefabUtility.UnloadPrefabContents(go);
    }
    static void BalanceUpgrades()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:UpgradeData"))
        {
            var u = AssetDatabase.LoadAssetAtPath<UpgradeData>(AssetDatabase.GUIDToAssetPath(guid)); if (!u) continue;
            int tier = u.name.EndsWith("3") ? 3 : u.name.EndsWith("2") ? 2 : 1;
            u.upgradeName = u.upgradeName.Replace("Thurster", "Thruster");
            u.maxRanks = 2; u.minLevel = tier == 3 ? 10 : tier == 2 ? 6 : 1;
            u.icon = Art("star_large");
            if (u is AddHeartUpgrade heart) { heart.heartsToAdd = 1; u.maxRanks = 8; u.description = "Restore 1 hull point. Maximum hull: 8."; u.icon = Art("icon_plusLarge"); u.rarity = UpgradeData.UpgradeRarity.Common; }
            if (u is BulletSpeedUpgrade bullet) { bullet.speedBonus = 3 * tier; u.description = "+" + bullet.speedBonus + " projectile speed."; u.icon = Art("ship_I"); }
            if (u is RapidFireUpgrade fire) { fire.fireRateBonus = 0.6f * tier; u.description = "+" + fire.fireRateBonus.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture) + " shots per second. Limit: 10/s."; u.icon = Art("ship_K"); }
            if (u is ExtraBulletUpgrade extra) { extra.bulletsToAdd = tier; u.description = "+" + tier + " projectiles per volley. Limit: 7 total."; u.icon = Art("ship_E"); u.minLevel = tier == 1 ? 3 : 10; }
            if (u is PiercingUpgrade pierce) { pierce.amount = tier; u.description = "Shots pass through " + tier + " additional targets. Limit: 4."; u.icon = Art("ship_G"); }
            if (u is SplitShotUpgrade) { u.maxRanks = 1; u.minLevel = 8; u.description = "On first impact, release two short-lived fragments."; u.icon = Art("ship_F"); }
            if (u is FocusedShotUpgrade focused) { focused.spreadReduction = tier * 2; u.description = "Tighten the volley by " + tier * 2 + " degrees for focused damage."; u.icon = Art("ship_C"); }
            if (u is WideSpreadUpgrade wide) { wide.spreadIncrease = tier * 3; u.description = "Widen the volley by " + tier * 3 + " degrees for crowd coverage."; u.icon = Art("ship_D"); u.minLevel = 4; }
            if (u is DashCooldownUpgrade dash) { dash.reduction = tier == 1 ? 0.25f : 0.4f; u.description = "Dash recharges " + dash.reduction.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + " seconds sooner. Minimum: 0.8s."; u.icon = Art("icon_crossLarge"); u.minLevel = 2; }
            if (u is DashForceUpgrade force) { force.bonus = 3 * tier; u.description = "+" + force.bonus + " dash speed for a longer escape."; u.icon = Art("ship_J"); u.minLevel = 2; }
            if (u is PhaseDashUpgrade) { u.maxRanks = 1; u.minLevel = 4; u.description = "Extend dash protection from 0.20 to 0.45 seconds."; u.icon = Art("ship_B"); u.rarity = UpgradeData.UpgradeRarity.Rare; }
            if (u is DroneCompanionUpgrade) { u.maxRanks = 3; u.minLevel = 2; u.description = "Deploy a companion that attacks nearby threats. Maximum: 3."; u.icon = Art("satellite_A"); }
            if (u.category == UpgradeData.UpgradeCategory.Drone && !(u is DroneCompanionUpgrade)) { u.minLevel = 5; u.icon = Art("satellite_B"); }
            if (u is DroneFireRateUpgrade df) { df.bonus = 0.4f; u.description = "All current and future drones gain +0.4 shots/s."; u.icon = Art("satellite_C"); }
            if (u is DroneRangeUpgrade) { u.description = "All current and future drones gain +1.5 targeting range."; u.icon = Art("station_A"); }
            if (u is DroneBulletSpeedUpgrade) { u.description = "All current and future drones gain +3 projectile speed."; u.icon = Art("satellite_D"); }
            if (u is DroneMultishotUpgrade) { u.maxRanks = 2; u.description = "All current and future drones fire +1 projectile."; u.icon = Art("station_B"); }
            if (u is ExpBoosterUpgrade eb) { eb.bonus = 0.15f; u.description = "+15% XP from collected motes."; u.icon = Art("Exp points sprite"); }
            if (u is GreedyCoreUpgrade gc) { gc.expBonus = 0.25f; gc.asteroidSpeedIncrease = 0.12f; u.maxRanks = 2; u.description = "+25% XP, but newly spawned asteroids move 12% faster."; u.icon = Art("meteor_detailedLarge"); }
            if (u is VeteranPilotUpgrade vet) { vet.reduction = 0.1f; u.maxRanks = 2; u.description = "Future level thresholds are reduced by 10% of their base cost."; u.icon = Art("star_medium"); }
            if (u is MaxVelocityUpgrade mv) { mv.speedIncrease = tier; u.description = "+" + tier + " maximum flight speed."; u.icon = Art("Player Ship"); }
            if (u is ThrusterBoostUpgrade thrust) { thrust.forceIncrease = tier * 4; u.category = UpgradeData.UpgradeCategory.Movement; u.description = "+" + tier * 4 + " acceleration for quicker steering."; u.icon = Art("ship_L"); }
            if (u is InertiaControlUpgrade inertia) { inertia.dampingIncrease = tier * 0.01f; u.description = "Reduce drifting for tighter handling."; u.icon = Art("icon_crossSmall"); }
            EditorUtility.SetDirty(u);
        }
    }
    static void Presentation()
    {
        var cam = Camera.main; cam.backgroundColor = new Color(0.015f, 0.022f, 0.042f); cam.orthographicSize = 13f;
        cam.allowHDR = true; cam.GetUniversalAdditionalCameraData().renderPostProcessing = true;
        var pipeline = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset; if (pipeline) { pipeline.supportsHDR = true; EditorUtility.SetDirty(pipeline); }
        var old = GameObject.Find("Starfield"); if (old) Object.DestroyImmediate(old);
        var stars = new GameObject("Starfield"); var random = new System.Random(507);
        for (int i = 0; i < 100; i++)
        {
            var go = new GameObject("Star"); go.transform.SetParent(stars.transform);
            go.transform.position = new Vector3((float)random.NextDouble() * 64 - 32, (float)random.NextDouble() * 36 - 18, 5);
            go.transform.localScale = Vector3.one * (0.025f + (float)random.NextDouble() * 0.055f);
            var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = Art("star_medium"); sr.sortingOrder = -20;
            sr.color = new Color(0.45f, 0.6f, 0.75f, 0.15f + (float)random.NextDouble() * 0.3f);
        }
        var existing = GameObject.Find("Combat bloom"); if (existing) Object.DestroyImmediate(existing);
        var volume = new GameObject("Combat bloom").AddComponent<Volume>(); volume.isGlobal = true; volume.priority = 10;
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(Root + "Art/CombatBloom.asset");
        if (!profile) { profile = ScriptableObject.CreateInstance<VolumeProfile>(); AssetDatabase.CreateAsset(profile, Root + "Art/CombatBloom.asset"); }
        profile.components.RemoveAll(component => !component);
        if (!profile.TryGet<Bloom>(out var bloom)) bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(0.45f); bloom.threshold.Override(1.1f); bloom.scatter.Override(0.55f);
        if (!profile.TryGet<Vignette>(out var vignette)) vignette = profile.Add<Vignette>(true);
        vignette.intensity.Override(0.18f); vignette.smoothness.Override(0.65f);
        foreach (var component in profile.components) if (!AssetDatabase.Contains(component)) AssetDatabase.AddObjectToAsset(component, profile);
        volume.sharedProfile = profile; EditorUtility.SetDirty(profile);
    }
    static Canvas NewCanvas()
    {
        foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include)) if (canvas) Object.DestroyImmediate(canvas.gameObject);
        var go = new GameObject("Flight Interface", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var c = go.GetComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceCamera; c.worldCamera = Camera.main; c.planeDistance = 1; c.sortingOrder = 100;
        var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
        return c;
    }
    static RectTransform Rect(string name, Transform parent, Vector2 pos, Vector2 size, Vector2? anchor = null)
    {
        var go = new GameObject(name, typeof(RectTransform)); var r = go.GetComponent<RectTransform>(); r.SetParent(parent, false);
        r.anchorMin = r.anchorMax = anchor ?? new Vector2(0.5f, 0.5f); r.anchoredPosition = pos; r.sizeDelta = size; return r;
    }
    static Image Image(string name, Transform parent, Vector2 pos, Vector2 size, Color color, Vector2? anchor = null)
    { var r = Rect(name, parent, pos, size, anchor); var im = r.gameObject.AddComponent<Image>(); im.color = color; im.raycastTarget = false; return im; }
    static TextMeshProUGUI Text(string name, Transform parent, string value, Vector2 pos, Vector2 size, int fontSize, Color color, Vector2? anchor = null)
    {
        var r = Rect(name, parent, pos, size, anchor); var t = r.gameObject.AddComponent<TextMeshProUGUI>(); t.font = font; t.text = value; t.fontSize = fontSize; t.color = color; t.alignment = TextAlignmentOptions.Center; t.raycastTarget = false; t.textWrappingMode = TextWrappingModes.Normal; return t;
    }
    static GameObject Overlay(string name, Transform parent)
    {
        var im = Image(name, parent, Vector2.zero, Vector2.zero, new Color(0.015f, 0.025f, 0.045f, 0.96f));
        im.rectTransform.anchorMin = Vector2.zero; im.rectTransform.anchorMax = Vector2.one; im.raycastTarget = true; return im.gameObject;
    }
    static Button Button(string label, Transform parent, Vector2 pos, Vector2 size)
    {
        var im = Image(label, parent, pos, size, new Color(0.09f, 0.15f, 0.2f)); im.raycastTarget = true;
        var b = im.gameObject.AddComponent<Button>(); b.targetGraphic = im;
        var colors = b.colors; colors.highlightedColor = new Color(0.6f, 0.88f, 1f); colors.pressedColor = new Color(0.3f, 0.65f, 0.9f); b.colors = colors;
        Text("Label", b.transform, label, Vector2.zero, size - new Vector2(24, 12), 22, Color.white);
        b.gameObject.AddComponent<UIAnimation>().interactive = true; return b;
    }
    static void BuildHUD(PlayerController pc, CombatFeedback feedback)
    {
        var canvas = NewCanvas(); var parent = canvas.transform;
        var manager = Object.FindAnyObjectByType<UpgradeManager>();
        var upgrades = Object.FindAnyObjectByType<UpgradeUI>(); if (!upgrades) upgrades = manager.gameObject.AddComponent<UpgradeUI>();
        var effects = Object.FindAnyObjectByType<SceneEffects>(); if (!effects) effects = manager.gameObject.AddComponent<SceneEffects>();
        if (!Object.FindAnyObjectByType<RunTimer>()) manager.gameObject.AddComponent<RunTimer>();
        var top = Image("Top bar", parent, new Vector2(0, -60), new Vector2(0, 120), Ink, new Vector2(0.5f, 1)); top.rectTransform.anchorMin = new Vector2(0, 1); top.rectTransform.anchorMax = Vector2.one;
        Text("Title", parent, "S P A C E   /   S U R V I V O R", new Vector2(240, -34), new Vector2(430, 40), 20, Muted, new Vector2(0, 1));
        var hearts = Rect("Hull", parent, new Vector2(245, -82), new Vector2(440, 30), new Vector2(0, 1));
        var layout = hearts.gameObject.AddComponent<HorizontalLayoutGroup>(); layout.spacing = 8; layout.childControlWidth = layout.childControlHeight = false; layout.childForceExpandWidth = layout.childForceExpandHeight = false;
        var healthUI = hearts.gameObject.AddComponent<HeartsUI>(); healthUI.playerHealth = pc.GetComponent<PlayerHealth>(); healthUI.heartContainer = hearts;
        healthUI.heartPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefab/PlayerHearts (1).prefab").GetComponent<Image>();
        var heartRect = healthUI.heartPrefab.rectTransform; heartRect.sizeDelta = new Vector2(28, 28); EditorUtility.SetDirty(healthUI.heartPrefab);
        var level = Text("Level", parent, "LVL 1", new Vector2(0, -32), new Vector2(180, 35), 23, Color.white, new Vector2(0.5f, 1)); level.gameObject.AddComponent<LevelTextUI>().playerExp = pc.GetComponent<PlayerExperience>();
        Image("XP track", parent, new Vector2(0, -77), new Vector2(400, 5), new Color(0.12f, 0.2f, 0.27f), new Vector2(0.5f, 1));
        var xpFill = Image("XP fill", parent, new Vector2(0, -77), new Vector2(400, 5), CombatFeedback.Cyan, new Vector2(0.5f, 1)); xpFill.sprite = square; xpFill.type = UnityEngine.UI.Image.Type.Filled; xpFill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        var xpUI = xpFill.gameObject.AddComponent<ExpBarUI>(); xpUI.playerExp = pc.GetComponent<PlayerExperience>(); xpUI.fillImage = xpFill;
        var hud = canvas.gameObject.AddComponent<FlightHUD>(); hud.player = pc;
        hud.timerText = Text("Timer", parent, "00:00", new Vector2(-165, -35), new Vector2(270, 45), 32, Color.white, Vector2.one);
        hud.sectorText = Text("Sector", parent, "01 / FIRST CONTACT", new Vector2(-165, -81), new Vector2(280, 30), 15, Muted, Vector2.one);
        Text("Controls", parent, "WASD / MOVE     LMB / FIRE     ESC / PAUSE", new Vector2(290, 32), new Vector2(560, 35), 17, Muted, Vector2.zero);
        hud.dashText = Text("Dash", parent, "DASH READY / RMB", new Vector2(-180, 45), new Vector2(310, 32), 18, CombatFeedback.Cyan, new Vector2(1, 0));
        Image("Dash track", parent, new Vector2(-180, 20), new Vector2(270, 4), new Color(0.12f, 0.2f, 0.27f), new Vector2(1, 0));
        hud.dashFill = Image("Dash fill", parent, new Vector2(-180, 20), new Vector2(270, 4), CombatFeedback.Cyan, new Vector2(1, 0)); hud.dashFill.sprite = square; hud.dashFill.type = UnityEngine.UI.Image.Type.Filled; hud.dashFill.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        var flash = Image("Impact flash", parent, Vector2.zero, Vector2.zero, Color.clear); flash.rectTransform.anchorMin = Vector2.zero; flash.rectTransform.anchorMax = Vector2.one; feedback.flash = flash;
        var panel = Overlay("Upgrade selection", parent); upgrades.panel = panel;
        Text("Eyebrow", panel.transform, "S Y S T E M   U P G R A D E", new Vector2(0, 365), new Vector2(1000, 45), 18, CombatFeedback.Cyan);
        Text("Title", panel.transform, "CHOOSE YOUR NEXT EDGE", new Vector2(0, 305), new Vector2(1200, 70), 42, Color.white);
        Text("Hint", panel.transform, "Select one module. Flight is paused while you choose.", new Vector2(0, 249), new Vector2(1100, 40), 21, Muted);
        upgrades.buttons = new UpgradeButtonUI[3];
        for (int i = 0; i < 3; i++)
        {
            var b = Button("", panel.transform, new Vector2((i - 1) * 380, -30), new Vector2(350, 450));
            Object.DestroyImmediate(b.GetComponentInChildren<TextMeshProUGUI>().gameObject);
            var ui = b.gameObject.AddComponent<UpgradeButtonUI>(); ui.button = b;
            ui.icon = Image("Module icon", b.transform, new Vector2(0, 115), new Vector2(88, 88), Color.white); ui.icon.preserveAspect = true;
            ui.title = Text("Name", b.transform, "MODULE", new Vector2(0, 20), new Vector2(310, 70), 25, Color.white);
            ui.description = Text("Effect", b.transform, "", new Vector2(0, -70), new Vector2(290, 110), 20, Muted);
            ui.rarityText = Text("Rarity", b.transform, "COMMON", new Vector2(0, -180), new Vector2(310, 32), 14, CombatFeedback.Cyan);
            upgrades.buttons[i] = ui;
        }
        Text("Footer", panel.transform, "BUILD A SHIP THAT FLIES YOUR WAY", new Vector2(0, -335), new Vector2(1000, 40), 16, Muted);
        panel.AddComponent<UIAnimation>();
        var end = Overlay("Run summary", parent); var endUI = end.AddComponent<EndScreenUI>();
        Text("Header", end.transform, "SIGNAL LOST", new Vector2(0, 240), new Vector2(1000, 100), 64, Color.white);
        Text("Subheading", end.transform, "Every flight leaves you a better pilot.", new Vector2(0, 158), new Vector2(1000, 45), 23, Muted);
        endUI.levelText = Text("Level", end.transform, "", new Vector2(0, 60), new Vector2(1000, 50), 28, CombatFeedback.Cyan);
        endUI.timeText = Text("Time", end.transform, "", Vector2.zero, new Vector2(1000, 50), 24, Color.white);
        endUI.asteroidsText = Text("Kills", end.transform, "", new Vector2(0, -60), new Vector2(1100, 50), 22, Muted);
        UnityEventTools.AddPersistentListener(Button("FLY AGAIN", end.transform, new Vector2(-190, -200), new Vector2(330, 70)).onClick, endUI.Retry);
        UnityEventTools.AddPersistentListener(Button("MAIN MENU", end.transform, new Vector2(190, -200), new Vector2(330, 70)).onClick, endUI.MainMenu);
        end.AddComponent<UIAnimation>();
        hud.pausePanel = Overlay("Pause", parent);
        Text("Paused", hud.pausePanel.transform, "FLIGHT PAUSED", new Vector2(0, 140), new Vector2(1000, 80), 48, Color.white);
        UnityEventTools.AddPersistentListener(Button("RESUME", hud.pausePanel.transform, new Vector2(0, 0), new Vector2(360, 70)).onClick, hud.Resume);
        UnityEventTools.AddPersistentListener(Button("MAIN MENU", hud.pausePanel.transform, new Vector2(0, -100), new Vector2(360, 70)).onClick, hud.Menu);
        hud.pausePanel.SetActive(false);
        // Keep end active in the scene: its Awake registers the singleton then hides it.
        panel.SetActive(false);
    }
    static void BuildMenu()
    {
        Presentation(); var canvas = NewCanvas(); var ui = Object.FindAnyObjectByType<MainMenuUI>();
        if (!ui) ui = new GameObject("Menu controller").AddComponent<MainMenuUI>();
        var ship = Image("Hero ship", canvas.transform, new Vector2(0, 285), new Vector2(115, 115), Color.white); ship.sprite = Art("Player Ship"); ship.preserveAspect = true;
        Text("Eyebrow", canvas.transform, "A R C A D E   O R B I T A L   S U R V I V A L", new Vector2(0, 190), new Vector2(1400, 50), 18, CombatFeedback.Cyan);
        Text("Title", canvas.transform, "SPACE SHOOTER", new Vector2(0, 100), new Vector2(1500, 110), 76, Color.white);
        Text("Subtitle", canvas.transform, "Outfly the swarm. Build your ship. Stay in the fight.", new Vector2(0, 5), new Vector2(1300, 60), 24, Muted);
        UnityEventTools.AddPersistentListener(Button("LAUNCH", canvas.transform, new Vector2(0, -115), new Vector2(390, 75)).onClick, ui.startGame);
        UnityEventTools.AddPersistentListener(Button("QUIT", canvas.transform, new Vector2(0, -215), new Vector2(390, 65)).onClick, ui.Quitgame);
        Text("Controls", canvas.transform, "WASD / MOVE     MOUSE / AIM     LMB / FIRE     RMB / DASH", new Vector2(0, -340), new Vector2(1400, 50), 18, Muted);
        Text("Credits", canvas.transform, "SRI HARI   /   SPACE ART BY KENNEY", new Vector2(0, 35), new Vector2(1200, 40), 15, Muted, new Vector2(0.5f, 0));
    }
}




