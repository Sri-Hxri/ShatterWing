using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using UnityEngine.TextCore.LowLevel;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public static class SpaceShooterArtPass
{
    const string Root = "Assets/_Main/";
    public static readonly Dictionary<string, string> Icons = new Dictionary<string, string> {
        {"BulletSpeedUpgrade", "faster_bullets"}, {"RapidFireUpgrade", "rapid_fire"},
        {"ExtraBulletUpgrade", "extra_shot"}, {"PiercingUpgrade", "piercing"},
        {"FocusedShotUpgrade", "concentrated_shot"}, {"WideSpreadUpgrade", "wide_spread"},
        {"SplitShotUpgrade", "split_shot"}, {"ThrusterBoostUpgrade", "thruster_booster"},
        {"MaxVelocityUpgrade", "max_speed"}, {"InertiaControlUpgrade", "more_control"},
        {"DashCooldownUpgrade", "dash_cooldown"}, {"DashForceUpgrade", "dash_force"},
        {"PhaseDashUpgrade", "phase_dash"}, {"DroneCompanionUpgrade", "brother_in_arms"},
        {"DroneFireRateUpgrade", "drone_rapid_fire"}, {"DroneBulletSpeedUpgrade", "drone_bullet_speed"},
        {"DroneRangeUpgrade", "drone_range"}, {"DroneMultishotUpgrade", "drone_twin_fire"},
        {"ExpBoosterUpgrade", "exp_booster"}, {"GreedyCoreUpgrade", "greedy_core"},
        {"VeteranPilotUpgrade", "veteran_pilot"}, {"AddHeartUpgrade", "long_live"}
    };

    [MenuItem("Tools/Space Shooter/Apply original icons and Times New Roman")]
    public static void Apply()
    {
        foreach (var key in Icons.Values)
        {
            string path = Root + "Art/UpgradeIconsOriginal/" + key + ".png";
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!importer) throw new Exception("Missing generated icon: " + key);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 128;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 256;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
        foreach (var guid in AssetDatabase.FindAssets("t:UpgradeData"))
        {
            var upgrade = AssetDatabase.LoadAssetAtPath<UpgradeData>(AssetDatabase.GUIDToAssetPath(guid));
            upgrade.icon = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "Art/UpgradeIconsOriginal/" + Icons[upgrade.GetType().Name] + ".png");
            EditorUtility.SetDirty(upgrade);
        }
        var regular = FontAsset("Times New Roman", "times.ttf");
        var bold = FontAsset("Times New Roman Bold", "timesbd.ttf");
        EditPrefab("ExperienceMote", go => go.transform.localScale = Vector3.one * 0.45f);
        EditPrefab("DroneBullet", go => go.transform.localScale = new Vector3(0.17f, 0.40f, 0.17f));
        EditPrefab("Asteroid", go => {
            foreach (var c in go.GetComponents<Collider2D>()) if (!(c is PolygonCollider2D)) Object.DestroyImmediate(c);
            if (!go.GetComponent<PolygonCollider2D>()) go.AddComponent<PolygonCollider2D>();
            go.GetComponent<Asteroid>().FitColliderToSprite();
        });
        float[] speed = {4.4f, 3.1f, 5.7f, 2f};
        float[] interval = {3.2f, 2.5f, 1.8f, 4f};
        for (int i = 0; i < 4; i++)
        {
            int index = i;
            EditPrefab(((EnemyRole)i).ToString(), go => { var e = go.GetComponent<EnemyShip>(); e.speed = speed[index]; e.attackInterval = interval[index]; });
        }
        EditPrefab("ExpPopUP", go => { foreach (var text in go.GetComponentsInChildren<TMP_Text>(true)) text.font = regular; });
        foreach (string scene in new[] {"MainMenu", "GameScene"})
        {
            EditorSceneManager.OpenScene(Root + "Scenes/" + scene + ".unity");
            foreach (var text in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include))
            {
                text.font = text.name == "Title" || text.name == "Header" || text.name == "Name" ? bold : regular;
                text.enableAutoSizing = true;
                text.fontSizeMin = text.fontSize * 0.9f;
                text.fontSizeMax = text.fontSize * 1.1f;
                text.margin = new Vector4(4, 2, 4, 2);
            }
            var spawner = Object.FindAnyObjectByType<AsteroidSpawner>();
            if (spawner) { spawner.spawnInterval = 1.15f; spawner.globalSpeedMultiplier = 1.10f; }
            foreach (var card in Object.FindObjectsByType<UpgradeButtonUI>(FindObjectsInactive.Include))
                card.icon.rectTransform.sizeDelta = new Vector2(104, 104);
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
        AssetDatabase.SaveAssets();
        SpaceShooterBuildPass.Apply();
        Debug.Log("ORIGINAL_ART_PASS_COMPLETE: 22 original icon families across 36 upgrades");
    }
    static TMP_FontAsset FontAsset(string label, string source)
    {
        string path = Root + "Fonts/" + label + " SDF.asset";
        var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        if (asset) return asset;
        var font = AssetDatabase.LoadAssetAtPath<Font>(Root + "Fonts/" + source);
        if (!font) throw new Exception("Font source missing: " + source);
        asset = TMP_FontAsset.CreateFontAsset(font, 64, 6, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, false);
        asset.name = label + " SDF";
        asset.TryAddCharacters(string.Concat(Enumerable.Range(32, 95).Select(c => (char)c)) + "\u00b0\u00d7\u2013\u2014", out string missing);
        if (!string.IsNullOrEmpty(missing)) throw new Exception("Font lacks required characters: " + missing);
        asset.atlasPopulationMode = AtlasPopulationMode.Static;
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.AddObjectToAsset(asset.material, asset);
        foreach (var texture in asset.atlasTextures) AssetDatabase.AddObjectToAsset(texture, asset);
        EditorUtility.SetDirty(asset);
        return asset;
    }
    static void EditPrefab(string name, Action<GameObject> edit)
    {
        string path = Root + "Prefab/" + name + ".prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try { edit(root); PrefabUtility.SaveAsPrefabAsset(root, path); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }
    public static void ApplyAndVerify() { Apply(); SpaceShooterVerification.Run(); }
}
