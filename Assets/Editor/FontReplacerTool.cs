using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

public class FontReplacerTool : EditorWindow
{
    TMP_FontAsset oldFont;
    TMP_FontAsset newFont;

    bool includeInactive = true;
    bool replaceInPrefabs = true;

    [MenuItem("Tools/Font Replacer")]
    static void Open()
    {
        GetWindow<FontReplacerTool>("Font Replacer");
    }

    void OnGUI()
    {
        GUILayout.Label("TextMeshPro Font Replacer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        oldFont = (TMP_FontAsset)EditorGUILayout.ObjectField(
            "Old Font (Optional)",
            oldFont,
            typeof(TMP_FontAsset),
            false
        );

        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField(
            "New Font",
            newFont,
            typeof(TMP_FontAsset),
            false
        );

        includeInactive = EditorGUILayout.Toggle(
            "Include Inactive Objects",
            includeInactive
        );

        replaceInPrefabs = EditorGUILayout.Toggle(
            "Replace In Prefabs",
            replaceInPrefabs
        );

        EditorGUILayout.Space();

        GUI.enabled = newFont != null;

        if (GUILayout.Button("Replace Fonts (Scene + Prefabs)"))
        {
            ReplaceFonts();
        }

        GUI.enabled = true;
    }

    // ================= CORE =================

    void ReplaceFonts()
    {
        int sceneCount = ReplaceFontsInScene();
        int prefabCount = replaceInPrefabs ? ReplaceFontsInPrefabs() : 0;

        Debug.Log(
            $"Font Replacer Done ✔\n" +
            $"- Scene objects updated: {sceneCount}\n" +
            $"- Prefab objects updated: {prefabCount}"
        );
    }

    // ================= SCENE =================

    int ReplaceFontsInScene()
    {
        TMP_Text[] texts = includeInactive
            ? Resources.FindObjectsOfTypeAll<TMP_Text>()
            : Object.FindObjectsOfType<TMP_Text>();

        int count = 0;

        foreach (TMP_Text t in texts)
        {
            if (t == null || EditorUtility.IsPersistent(t))
                continue;

            if (oldFont != null && t.font != oldFont)
                continue;

            Undo.RecordObject(t, "Replace TMP Font (Scene)");
            t.font = newFont;
            EditorUtility.SetDirty(t);
            count++;
        }

        return count;
    }

    // ================= PREFABS =================

    int ReplaceFontsInPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int count = 0;

        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
                continue;

            bool modified = false;

            TMP_Text[] texts = prefab.GetComponentsInChildren<TMP_Text>(true);

            foreach (TMP_Text t in texts)
            {
                if (oldFont != null && t.font != oldFont)
                    continue;

                Undo.RecordObject(t, "Replace TMP Font (Prefab)");
                t.font = newFont;
                EditorUtility.SetDirty(t);
                modified = true;
                count++;
            }

            if (modified)
            {
                PrefabUtility.SavePrefabAsset(prefab);
            }
        }

        return count;
    }
}
