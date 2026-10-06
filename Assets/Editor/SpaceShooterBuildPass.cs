using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Object = UnityEngine.Object;

public static class SpaceShooterBuildPass
{
    const string Root = "Assets/_Main/";
    [MenuItem("Tools/Space Shooter/Apply meaningful builds and clean UI")]
    public static void Apply()
    {
        var all = AssetDatabase.FindAssets("t:UpgradeData").Select(g => AssetDatabase.LoadAssetAtPath<UpgradeData>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
        foreach (var group in all.GroupBy(u => u.GetType()))
        {
            var tiers = group.OrderBy(u => u.rarity).ThenBy(u => u.name).ToArray();
            for (int i = 0; i < tiers.Length; i++)
            {
                var u = tiers[i]; int tier = i + 1;
                u.maxRanks = 1;
                u.minLevel = i == 0 ? 2 : i == 1 ? 5 : 9;
                string suffix = tiers.Length > 1 ? " " + new[] {"I", "II", "III"}[i] : "";
                if (u is BulletSpeedUpgrade speed) { speed.speedBonus = 4; u.upgradeName = "Charged Rounds" + suffix; u.description = "Every " + new[] {"fourth", "third", "second"}[i] + " volley deals +1 damage.\n+4 shot speed."; }
                else if (u is RapidFireUpgrade rapid) { rapid.fireRateBonus = 1.25f; u.upgradeName = "Rapid Fire" + suffix; u.description = "+1.25 base volleys per second.\nFaster fire for your current weapon."; }
                else if (u is ExtraBulletUpgrade extra) { extra.bulletsToAdd = tier; u.upgradeName = "Multishot" + suffix; u.description = "+" + tier + " shot" + (tier > 1 ? "s" : "") + " per volley.\nMore coverage; slower volleys."; }
                else if (u is FocusedShotUpgrade) { u.upgradeName = "Lance" + suffix; u.description = i == 0 ? "Double damage. Tight aim.\n35% slower fire.\nLocks out Scatter." : "Lance shots pierce +1 target.\nKeep enemies lined up."; }
                else if (u is WideSpreadUpgrade) { u.upgradeName = "Scatter" + suffix; u.description = i == 0 ? "+2 shots in a wide cone.\nShort range; slower volleys.\nLocks out Lance." : "+2 range for Scatter.\nReach further while clearing crowds."; }
                else if (u is PiercingUpgrade pierce) { pierce.amount = 1; u.upgradeName = "Piercing" + suffix; u.description = "Shots pass through +1 target.\nLine up enemies for more damage."; }
                else if (u is SplitShotUpgrade) { u.minLevel = 6; u.upgradeName = "Shrapnel"; u.description = "Hits release two short-range shards.\nEach shard deals 1 damage."; }
                else if (u is DashForceUpgrade force) { force.bonus = 3; u.upgradeName = "Dash Volley" + suffix; u.description = i == 0 ? "Dash fires five short-range shots.\n+3 dash speed. Aim before dashing." : "+3 dash speed.\nReach safety with each dash volley."; }
                else if (u is DashCooldownUpgrade dash) { dash.reduction = 0.5f; u.upgradeName = "Quick Dash" + suffix; u.description = "Dash recharges 0.5s sooner.\nMore escapes and dash volleys."; }
                else if (u is PhaseDashUpgrade) { u.minLevel = 4; u.upgradeName = "Phase Dash"; u.description = "Dash clears nearby hostile shots.\nProtection lasts 0.45s."; }
                else if (u is ExpBoosterUpgrade magnet) { magnet.bonus = 0.15f; u.upgradeName = "Salvage Magnet"; u.description = "+2.5 pickup range and +15% XP.\nCollect safely while fighting."; }
                else if (u is GreedyCoreUpgrade greedy) { greedy.expBonus = 0.30f; greedy.asteroidSpeedIncrease = 0.20f; u.upgradeName = "Greedy Core"; u.description = "+30% XP.\nAsteroids move 20% of base faster."; }
                else if (u is VeteranPilotUpgrade veteran) { veteran.reduction = 0.15f; u.upgradeName = "Fast Learner"; u.description = "Future level thresholds cost 15% less.\nInvest now for more choices later."; }
                else if (u is ThrusterBoostUpgrade thrust) { thrust.forceIncrease = 9; u.upgradeName = "Agile Thrusters" + suffix; u.description = "+9 acceleration.\nChange direction more quickly."; }
                else if (u is MaxVelocityUpgrade velocity) { velocity.speedIncrease = 1.5f; u.upgradeName = "Cruise Engines" + suffix; u.description = "+1.5 top speed.\nOutrun danger and collect distant XP."; }
                else if (u is InertiaControlUpgrade brakes) { brakes.dampingIncrease = 0.025f; u.upgradeName = "Precision Brakes" + suffix; u.description = "Brake harder when movement is released.\nHold a firing position more easily."; }
                else if (u is AddHeartUpgrade repair) { repair.heartsToAdd = 2; u.maxRanks = 3; u.upgradeName = "Hull Repair"; u.description = "Restore 2 hearts, up to 8.\nRecover from a dangerous encounter."; }
                else if (u is DroneCompanionUpgrade) { u.maxRanks = 3; u.upgradeName = "Wingman"; u.description = "Add an auto-targeting drone.\nPrioritizes enemy ships. Up to 3."; }
                else if (u is DroneFireRateUpgrade droneRapid) { droneRapid.bonus = 0.5f; u.maxRanks = 2; u.upgradeName = "Drone Overclock"; u.description = "+0.5 base shots per second\nfor every current and future drone."; }
                else if (u is DroneBulletSpeedUpgrade droneSpeed) { droneSpeed.bonus = 8; u.upgradeName = "Drone Railshots"; u.description = "Drone shots pierce one target.\n+8 shot speed for reliable support."; }
                else if (u is DroneRangeUpgrade droneRange) { droneRange.bonus = 4; u.upgradeName = "Drone Radar"; u.description = "+4 drone targeting range.\nAttack ships before they close in."; }
                else if (u is DroneMultishotUpgrade) { u.upgradeName = "Drone Twin Fire"; u.description = "Drones fire one extra shot.\nWider coverage; slower volleys."; }
                EditorUtility.SetDirty(u);
            }
        }
        foreach (string scene in new[] {"MainMenu", "GameScene"})
        {
            EditorSceneManager.OpenScene(Root + "Scenes/" + scene + ".unity");
            var hud = Object.FindAnyObjectByType<FlightHUD>();
            if (hud)
            {
                Transform parent = hud.transform;
                Hide(parent, "Title"); Hide(parent, "Sector");
                var controls = parent.Find("Controls");
                if (controls) { controls.SetParent(hud.pausePanel.transform, false); controls.GetComponent<TMP_Text>().text = "WASD move   /   Mouse aim   /   LMB fire   /   RMB dash";
                    var cr = controls.GetComponent<RectTransform>(); cr.anchorMin = cr.anchorMax = new Vector2(0.5f,0.5f); cr.anchoredPosition = new Vector2(0,-220); cr.sizeDelta = new Vector2(900,40); controls.gameObject.SetActive(true); }
                Layout(parent, "Top bar", new Vector2(0,-36), new Vector2(0,72));
                Layout(parent, "Hull", new Vector2(150,-36), new Vector2(260,28));
                Layout(parent, "Level", new Vector2(0,-24), new Vector2(130,30));
                Layout(parent, "XP track", new Vector2(0,-49), new Vector2(280,4));
                Layout(parent, "XP fill", new Vector2(0,-49), new Vector2(280,4));
                Layout(parent, "Timer", new Vector2(-90,-35), new Vector2(150,40));
                Layout(parent, "Dash", new Vector2(-105,38), new Vector2(190,28));
                Layout(parent, "Dash track", new Vector2(-105,19), new Vector2(160,3));
                Layout(parent, "Dash fill", new Vector2(-105,19), new Vector2(160,3));
                var draft = parent.Find("Upgrade selection");
                draft.GetComponent<Image>().color = new Color(0.015f, 0.02f, 0.035f, 0.94f);
                Hide(draft,"Eyebrow"); Hide(draft,"Hint"); Hide(draft,"Footer");
                var heading = draft.Find("Title").GetComponent<TMP_Text>(); heading.text = "Choose an upgrade"; heading.fontSize = 36; heading.enableAutoSizing = false;
                Layout(draft,"Title", new Vector2(0,280),new Vector2(900,65));
                foreach (var card in draft.GetComponentsInChildren<UpgradeButtonUI>(true))
                {
                    var rect = card.GetComponent<RectTransform>(); rect.sizeDelta = new Vector2(340,390);
                    card.icon.rectTransform.anchoredPosition = new Vector2(0,112); card.icon.rectTransform.sizeDelta = new Vector2(82,82);
                    card.title.rectTransform.anchoredPosition = new Vector2(0,34); card.title.rectTransform.sizeDelta = new Vector2(310,58);
                    card.description.rectTransform.anchoredPosition = new Vector2(0,-55); card.description.rectTransform.sizeDelta = new Vector2(302,114);
                    card.description.fontSize = 21; card.description.enableAutoSizing = true; card.description.fontSizeMin = 19; card.description.fontSizeMax = 21;
                    card.description.color = new Color(0.80f, 0.85f, 0.91f);
                    card.rarityText.rectTransform.anchoredPosition = new Vector2(0,-158);
                }
                Hide(parent.Find("Run summary"),"Subheading");
                var spawner = Object.FindAnyObjectByType<AsteroidSpawner>(); spawner.spawnInterval = 1.4f; spawner.globalSpeedMultiplier = 1.05f;
            }
            else
            {
                foreach (var t in Object.FindObjectsByType<TMP_Text>(FindObjectsInactive.Include))
                    if (t.name == "Eyebrow" || t.name == "Subtitle") t.gameObject.SetActive(false);
            }
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
        AssetDatabase.SaveAssets();
        Debug.Log("MEANINGFUL_BUILDS_APPLIED");
    }
    static void Hide(Transform parent, string name) { var child = parent.Find(name); if (child) child.gameObject.SetActive(false); }
    static void Layout(Transform parent, string name, Vector2 position, Vector2 size)
    { var r = parent.Find(name).GetComponent<RectTransform>(); r.anchoredPosition = position; r.sizeDelta = size; }
    public static void ApplyAndVerify() { Apply(); SpaceShooterVerification.Run(); }
}
