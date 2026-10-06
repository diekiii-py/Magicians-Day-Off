using System;

/// <summary>A single note in the beatmap: when it should be hit, and which gesture it requires.</summary>
[Serializable]
public class NoteData
{
    [UnityEngine.Tooltip("Seconds from the start of the rhythm battle at which the note should reach the hit zone.")]
    public float time;
    public GestureType requiredGesture = GestureType.Fist;
}
