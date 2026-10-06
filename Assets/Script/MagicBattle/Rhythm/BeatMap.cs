using UnityEngine;

/// <summary>
/// Data asset describing the whole cat "spell attack" rhythm sequence. Create one via
/// Assets > Create > Magic Battle > Beat Map, or use the Tools > Magic Battle > Create Default
/// Beat Map menu to auto-generate a ready-to-play sequence.
/// </summary>
[CreateAssetMenu(fileName = "NewBeatMap", menuName = "Magic Battle/Beat Map")]
public class BeatMap : ScriptableObject
{
    public NoteData[] notes;
    public float travelTime = 1.6f;
}
