using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class SpaceShooterVerification
{
    const string Output = "C:/Users/sriha/Documents/Codex/2026-10-06/referenced-chatgpt-conversation-this-is-an/outputs/";
    static int stage;
    static double next, start;
    static float pausedTimer;
    static EnemyShip hitTarget; static int targetHealth;
    static EnemyProjectile phaseNear, phaseFar;
    static readonly List<string> checks = new List<string>();
    static SpaceShooterVerification()
    {
        EditorApplication.playModeStateChanged += state => {
            if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool("SpaceSmoke", false))
            { stage = 0; start = EditorApplication.timeSinceStartup; next = start + 2; Application.logMessageReceived += Log; EditorApplication.update += Tick; }
        };
    }
    public static void SetupAndRun()
    {
        SpaceShooterPolishSetup.Apply();
        Run();
    }
    [MenuItem("Tools/Space Shooter/Run play-mode smoke checks")]
    public static void Run()
    {
        Directory.CreateDirectory(Output);
        EditorSceneManager.OpenScene("Assets/_Main/Scenes/MainMenu.unity");
        SessionState.SetBool("SpaceSmoke", true);
        EditorApplication.EnterPlaymode();
    }
    static void Check(bool condition, string message)
    { if (!condition) throw new Exception("CHECK FAILED: " + message); checks.Add("PASS: " + message); Debug.Log("SMOKE PASS: " + message); }
    static void Log(string message, string trace, LogType type)
    { if (type == LogType.Exception || type == LogType.Error) Finish(false, message + "\n" + trace); }
    static void Finish(bool success, string reason)
    {
        SessionState.SetBool("SpaceSmoke", false); EditorApplication.update -= Tick; Application.logMessageReceived -= Log;
        File.WriteAllText(Output + "verification.txt", (success ? "PASS" : "FAIL") + "\n" + string.Join("\n", checks) + "\n" + reason);
        Debug.Log(success ? "SMOKE_COMPLETE" : "SMOKE_FAILED: " + reason);
        EditorApplication.Exit(success ? 0 : 1);
    }
    static void Capture(string name)
    {
        var cam = Camera.main; var rt = new RenderTexture(1600, 900, 24); rt.Create();
        var previous = cam.targetTexture; cam.targetTexture = rt;
        Canvas.ForceUpdateCanvases();
        RenderPipeline.SubmitRenderRequest(cam, new UniversalRenderPipeline.SingleCameraRequest { destination = rt });
        var prior = RenderTexture.active; RenderTexture.active = rt;
        var texture = new Texture2D(1600, 900, TextureFormat.RGB24, false); texture.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0); texture.Apply();
        File.WriteAllBytes(Output + name + ".png", texture.EncodeToPNG());
        RenderTexture.active = prior; cam.targetTexture = previous; rt.Release(); Object.DestroyImmediate(texture); Object.DestroyImmediate(rt);
    }
    static void Tick()
    {
        if (!EditorApplication.isPlaying) return;
        double now = EditorApplication.timeSinceStartup;
        if (now - start > 90) { Finish(false, "Timed out"); return; }
        if (now < next) return;
        next = now + 1;
        try
        {
            var pc = Object.FindAnyObjectByType<PlayerController>();
            switch (stage++)
            {
                case 0:
                    Check(Object.FindAnyObjectByType<MainMenuUI>() != null, "Main menu loads"); Capture("main-menu");
                    Object.FindAnyObjectByType<MainMenuUI>().startGame(); break;
                case 1:
                    Camera.main.aspect = 16f / 9f;
                    Check(pc && pc.dashUnlocked && Time.timeScale == 1, "Fresh run starts with dash and normal time");
                    Check(CombatFeedback.Instance && EnemyDirector.Instance.enemies.Length == 4, "Feedback and four enemy prefabs are wired");
                    Check(UpgradeManager.Instance.allUpgrades.All(u => u && u.icon && u.maxRanks > 0), "All 36 upgrades have images and rank limits");
                    Check(UpgradeManager.Instance.allUpgrades.Select(u => u.icon).Distinct().Count() == 22 && UpgradeManager.Instance.allUpgrades.All(u => AssetDatabase.GetAssetPath(u.icon).Contains("UpgradeIconsOriginal")), "All upgrades use the 22 original generated icon families");
                    Check(Object.FindObjectsByType<TMPro.TMP_Text>(FindObjectsInactive.Include).All(t => t.font.name.StartsWith("Times New Roman")), "Every scene text uses Times New Roman");
                    var xpPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Main/Prefab/ExperienceMote.prefab");
                    var droneBullet = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Main/Prefab/DroneBullet.prefab");
                    Check(Mathf.Approximately(xpPrefab.transform.localScale.x, 0.45f) && droneBullet.transform.localScale.y >= 0.4f, "XP pickups and drone projectiles have larger authored sizes");
                    Check(pc.GetComponentInChildren<ParticleSystem>().emission.rateOverTime.constant == 0, "Thruster exhaust is off outside dash");
                    pc.GetComponent<PlayerHealth>().GrantInvulnerability(60);
                    var rock = Object.Instantiate(AsteroidSpawner.Instance.asteroidPrefab, new Vector3(7, 5, 0), Quaternion.identity); rock.Init(AsteroidSize.Large, Vector2.left); int hp = rock.health; rock.TakeDamage(1);
                    Check(rock.health == hp - 1 && rock.health > 0, "Large asteroid survives a single hit");
                    foreach (var variant in rock.variants)
                    {
                        rock.GetComponent<SpriteRenderer>().sprite = variant; rock.FitColliderToSprite();
                        var poly = rock.GetComponent<PolygonCollider2D>(); var bounds = variant.bounds;
                        Check(poly.pathCount > 0 && Enumerable.Range(0, poly.pathCount).SelectMany(i => poly.GetPath(i)).All(point => point.x >= bounds.min.x - 0.001f && point.x <= bounds.max.x + 0.001f && point.y >= bounds.min.y - 0.001f && point.y <= bounds.max.y + 0.001f), "Collider stays within sprite bounds: " + variant.name);
                    }
                    rock.Init(AsteroidSize.Large, Vector2.left);
                    Check(Mathf.Abs(Mathf.Max(rock.GetComponent<SpriteRenderer>().bounds.size.x, rock.GetComponent<SpriteRenderer>().bounds.size.y) - 2.8f) < 0.05f, "Large asteroid normalizes to a larger 2.8-unit silhouette");
                    VerifyBuildChoices(pc);
                    var xpComponent = pc.GetComponent<PlayerExperience>(); xpComponent.level = 2;
                    Check(UpgradeManager.Instance.IsEligible(UpgradeManager.Instance.allUpgrades.First(u => u is DroneCompanionUpgrade)), "First drone can be acquired with zero drones"); xpComponent.level = 1;
                    var dm = pc.GetComponent<DroneManager>(); dm.fireBonus = 0.4f; dm.AddDrone(); dm.AddDrone();
                    Check(dm.GetDrones().All(d => Mathf.Approximately(d.fireRateBonus, 0.4f)), "New drones inherit prior bonuses");
                    for (int i = 0; i < 4; i++) Object.Instantiate(EnemyDirector.Instance.enemies[i], new Vector3(-10 + i * 6, 7, 0), Quaternion.identity);
                    pc.weapon.SetFiring(true);
                    hitTarget = Object.Instantiate(EnemyDirector.Instance.enemies[3], new Vector3(13, -7, 0), Quaternion.identity); hitTarget.enabled = false; targetHealth = hitTarget.health;
                    Object.Instantiate(pc.weapon.bulletPrefab, new Vector3(13, -10, 0), Quaternion.identity); break;
                case 2:
                    Check(hitTarget.health < targetHealth, "Player projectile damages an enemy through physics collision"); Capture("combat"); pc.weapon.SetFiring(false);
                    var hostilePrefab = AssetDatabase.LoadAssetAtPath<EnemyProjectile>("Assets/_Main/Prefab/HostileShot.prefab");
                    phaseNear = Object.Instantiate(hostilePrefab, pc.transform.position + Vector3.right, Quaternion.identity); phaseNear.Init(Vector2.up, 0);
                    phaseFar = Object.Instantiate(hostilePrefab, pc.transform.position + Vector3.right * 8, Quaternion.identity); phaseFar.Init(Vector2.up, 0);
                    pc.phaseDash = true; pc.Dash();
                    Check(pc.IsDashing && pc.DashCooldownNormalized() > 0.9f, "Dash starts and consumes cooldown");
                    pc.GetComponent<Rigidbody2D>().position = new Vector2(100, 100); break;
                case 3:
                    Check(!phaseNear && phaseFar, "Phase Dash clears nearby hostile shots while preserving distant threats");
                    Object.Destroy(phaseFar.gameObject); pc.phaseDash = false;
                    Check(Mathf.Abs(pc.transform.position.x) < Camera.main.orthographicSize * Camera.main.aspect && Mathf.Abs(pc.transform.position.y) < Camera.main.orthographicSize, "Player is constrained to the camera arena");
                    pc.GetComponent<Rigidbody2D>().position = Vector2.zero;
                    pc.GetComponent<PlayerExperience>().GainExp(40); pausedTimer = RunTimer.Instance.GetTime(); break;
                case 4:
                    Check(GameState.IsPaused && Time.timeScale == 0 && UpgradeUI.Instance.panel.activeSelf, "Upgrade draft pauses all simulation");
                    Check(Mathf.Approximately(pausedTimer, RunTimer.Instance.GetTime()), "Run timer stays frozen during draft"); var cards = UpgradeUI.Instance.buttons;
                    for (int rarityIndex = 0; rarityIndex < 3; rarityIndex++)
                    {
                        var data = UpgradeManager.Instance.allUpgrades.First(u => rarityIndex == 0 ? u is WideSpreadUpgrade && u.rarity == UpgradeData.UpgradeRarity.Common : rarityIndex == 1 ? u is FocusedShotUpgrade && u.rarity == UpgradeData.UpgradeRarity.Rare : u is SplitShotUpgrade);
                        cards[rarityIndex].Setup(data, pc.gameObject);
                    }
                    Check(cards.Select(c => c.button.colors.normalColor).Distinct().Count() == 3, "Common, Rare and Epic cards have distinct surface colors");
                    Capture("upgrade-selection");
                    UpgradeUI.Instance.buttons.First(b => b.gameObject.activeSelf).button.onClick.Invoke(); break;
                case 5:
                    Check(UpgradeUI.Instance.panel.activeSelf, "Multiple level gains queue a second choice");
                    UpgradeUI.Instance.buttons.First(b => b.gameObject.activeSelf).button.onClick.Invoke(); break;
                case 6:
                    Check(!GameState.IsPaused && Time.timeScale == 1, "Simulation resumes after queued choices");
                    var acquire = UpgradeManager.Instance.allUpgrades.First(u => u is DroneCompanionUpgrade);
                    Check(UpgradeManager.Instance.IsEligible(acquire) == (pc.GetComponent<DroneManager>().GetDrones().Count < 3), "Drone acquisition respects the cap");
                    var extra = UpgradeManager.Instance.allUpgrades.OfType<ExtraBulletUpgrade>().Max(u => u.bulletsToAdd);
                    Check(extra == 2, "Extra-shot tier II adds two bullets, not thirty");
                    var profile = Object.FindObjectsByType<Volume>().First(v => v.name == "Combat bloom").sharedProfile;
                    Check(profile.TryGet<Bloom>(out var bloom) && bloom.intensity.value > 0, "Bloom settings persist as assets");
                    EndScreenUI.Instance.Show(3, 30, 5); break;
                case 7:
                    Check(GameState.IsDead && Time.timeScale == 0, "Death pauses combat"); Capture("run-summary"); EndScreenUI.Instance.MainMenu(); break;
                case 8:
                    Check(!GameState.IsDead && Time.timeScale == 1, "Return to menu resets run state"); Object.FindAnyObjectByType<MainMenuUI>().startGame(); break;
                case 9:
                    Check(pc && Time.timeScale == 1 && Asteroid.totalDestroyed == 0 && EnemyShip.TotalDestroyed == 0, "Second run starts unfrozen with reset counters");
                    Check(Object.FindObjectsByType<SoundManager>().Length == 1, "Scene transitions preserve a single audio manager");
                    var mote = AssetDatabase.LoadAssetAtPath<ExperiencePickup>("Assets/_Main/Prefab/ExperienceMote.prefab");
                    ExperiencePickup.Drop(mote, pc.transform.position, 2);
                    var shot = Object.Instantiate(AssetDatabase.LoadAssetAtPath<EnemyProjectile>("Assets/_Main/Prefab/HostileShot.prefab"), pc.transform.position, Quaternion.identity); shot.Init(Vector2.right, 0); break;
                case 10:
                    Check(pc.GetComponent<PlayerExperience>().currentExp >= 2, "XP mote is collected and awards XP");
                    Check(pc.GetComponent<PlayerHealth>().Hearts == 2, "Hostile projectile collision damages the player");
                    Finish(true, "Automated play-mode smoke checks passed. Human playtesting is still needed for difficulty and feel."); break;
            }
        }
        catch (Exception ex) { Finish(false, ex.ToString()); }
    }
    static void VerifyBuildChoices(PlayerController pc)
    {
        var manager = UpgradeManager.Instance; var xp = pc.GetComponent<PlayerExperience>(); var w = pc.weapon;
        xp.level = 10;
        var first = manager.allUpgrades.OfType<BulletSpeedUpgrade>().OrderBy(u => u.rarity).ToArray();
        Check(manager.IsEligible(first[0]) && !manager.IsEligible(first[1]), "Higher tiers require the previous family tier");
        manager.Record(first[0]);
        Check(!manager.IsEligible(first[0]) && manager.IsEligible(first[1]), "Selected one-time tier is removed and next tier unlocks");
        bool validDrafts = true;
        for (int i = 0; i < 30; i++)
        {
            var draft = manager.GetRandomUpgrades(3);
            validDrafts &= draft.Count == 3 && draft.Select(u => u.GetType()).Distinct().Count() == 3
                && draft.Any(u => u.category == UpgradeData.UpgradeCategory.Weapon || u.category == UpgradeData.UpgradeCategory.Drone)
                && draft.Any(u => u.category != UpgradeData.UpgradeCategory.Weapon && u.category != UpgradeData.UpgradeCategory.Drone);
        }
        Check(validDrafts, "Thirty drafts offer combat, utility and distinct families");
        float baseRate = w.EffectiveFireRate;
        var lance = manager.allUpgrades.OfType<FocusedShotUpgrade>().OrderBy(u => u.rarity).First();
        var scatter = manager.allUpgrades.OfType<WideSpreadUpgrade>().OrderBy(u => u.rarity).First();
        lance.Apply(pc.gameObject);
        Check(w.EffectiveSpread == 2 && w.EffectiveFireRate < baseRate && !manager.IsEligible(scatter), "Lance tightens aim, slows fire and excludes Scatter");
        var before = Object.FindObjectsByType<Bullet>().ToHashSet();
        w.SendMessage("Shoot");
        var lanceShot = Object.FindObjectsByType<Bullet>().First(b => !before.Contains(b));
        Check(lanceShot.damage == 2, "Lance fires real double-damage projectiles"); Object.DestroyImmediate(lanceShot.gameObject);
        w.focusRank = 0; scatter.Apply(pc.gameObject);
        Check(w.PelletCount == 3 && w.EffectiveRange == 13 && w.EffectiveSpread == 28 && !manager.IsEligible(lance), "Scatter adds two pellets with short range and excludes Lance");
        w.sweepRank = 0; w.dashVolley = true;
        before = Object.FindObjectsByType<Bullet>().ToHashSet(); w.FireDashVolley();
        var dashShots = Object.FindObjectsByType<Bullet>().Where(b => !before.Contains(b)).ToArray();
        Check(dashShots.Length == 5 && dashShots.All(b => !b.canSplit && b.pierceCount == 0), "Dash volley fires five bounded projectiles");
        foreach(var b in dashShots) Object.DestroyImmediate(b.gameObject); w.dashVolley = false;
        w.chargeRank = 3; before = Object.FindObjectsByType<Bullet>().ToHashSet();
        for (int i = 0; i < 4; i++) w.SendMessage("Shoot");
        var chargedShots = Object.FindObjectsByType<Bullet>().Where(b => !before.Contains(b)).ToArray();
        Check(chargedShots.Length == 4 && chargedShots.Count(b => b.damage == 2) == 2, "Charged Rounds III empowers every second real volley");
        foreach (var b in chargedShots) Object.DestroyImmediate(b.gameObject); w.chargeRank = 0;
        w.extraBullets = 3;
        Check(w.PelletCount == 4 && Mathf.Approximately(w.EffectiveFireRate, baseRate / 2f), "Four-pellet volleys halve fire rate instead of quadrupling sustained damage");
        w.extraBullets = 0;
        xp.expMultiplier = 1.15f;
        for(int i=0;i<10;i++) xp.GainExp(1);
        Check(xp.currentExp == 11, "Fractional XP bonuses accumulate across small pickups without rounding inflation");
        xp.currentExp = 0; xp.expMultiplier = 1; xp.level = 1;
        var hud = Object.FindAnyObjectByType<FlightHUD>();
        Check(!hud.transform.Find("Title").gameObject.activeSelf && !hud.sectorText.gameObject.activeSelf
            && hud.pausePanel.transform.Find("Controls") != null, "HUD removes repeated labels and moves controls into pause");
    }
    public static void Build()
    {
        var report = UnityEditor.BuildPipeline.BuildPlayer(new UnityEditor.BuildPlayerOptions {
            scenes = new[] { "Assets/_Main/Scenes/MainMenu.unity", "Assets/_Main/Scenes/GameScene.unity" },
            locationPathName = Output + "SpaceShooter-Windows/SpaceShooter.exe",
            target = BuildTarget.StandaloneWindows64, options = BuildOptions.None
        });
        File.WriteAllText(Output + "build-result.txt", report.summary.result + "\nErrors: " + report.summary.totalErrors + "\nWarnings: " + report.summary.totalWarnings);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new Exception("Player build failed");
    }
}


