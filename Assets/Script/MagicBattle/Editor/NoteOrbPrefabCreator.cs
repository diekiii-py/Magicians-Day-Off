#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>Editor utility that builds the NoteOrb prefab used by the rhythm game (a simple sphere, no art needed).</summary>
public static class NoteOrbPrefabCreator
{
    [MenuItem("Tools/Magic Battle/Create Note Orb Prefab")]
    public static void CreateNoteOrbPrefab()
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "NoteOrb";
        go.transform.localScale = Vector3.one * 0.18f;

        // Non-trigger collider + kinematic rigidbody so HitZone's trigger reliably fires events.
        var rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        go.AddComponent<NoteController>();

        const string folder = "Assets/Script/MagicBattle/Rhythm";
        const string path = folder + "/NoteOrb.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);

        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        Debug.Log($"Created NoteOrb prefab at {path}");
    }
}
#endif
