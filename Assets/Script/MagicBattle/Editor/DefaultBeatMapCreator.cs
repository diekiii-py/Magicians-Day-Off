#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>Editor utility to generate a ready-to-play default beat map so you don't have to hand-author 40 notes.</summary>
public static class DefaultBeatMapCreator
{
    [MenuItem("Tools/Magic Battle/Create Default Beat Map")]
    public static void CreateDefaultBeatMap()
    {
        var beatMap = ScriptableObject.CreateInstance<BeatMap>();
        beatMap.travelTime = 1.6f;

        var gestures = new[]
        {
            GestureType.Fist, GestureType.OpenPalm, GestureType.Point, GestureType.ThumbsUp, GestureType.Peace
        };

        const int noteCount = 40;
        var notes = new NoteData[noteCount];
        float t = 3f;
        var rng = new System.Random(12345);

        for (int i = 0; i < noteCount; i++)
        {
            // Spacing shrinks from ~2.2s down to ~1.0s as the song progresses, ramping difficulty.
            float progress = (float)i / noteCount;
            float spacing = Mathf.Lerp(2.2f, 1.0f, progress);

            notes[i] = new NoteData
            {
                time = t,
                requiredGesture = gestures[rng.Next(gestures.Length)]
            };
            t += spacing;
        }

        beatMap.notes = notes;

        const string path = "Assets/Script/MagicBattle/Rhythm/DefaultBeatMap.asset";
        AssetDatabase.CreateAsset(beatMap, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = beatMap;
        Debug.Log($"Created default beat map with {noteCount} notes at {path}");
    }
}
#endif
