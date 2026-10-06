using System;

/// <summary>
/// Central event bus for the Magic Battle sequence. Keeps the mirror, wall, cat and
/// rhythm-game systems loosely coupled: each system raises events, GameManager (and
/// anyone else) listens and reacts instead of holding direct references to each other.
/// </summary>
public static class GameEvents
{
    public static event Action OnMirrorCrackStart;
    public static event Action OnMirrorBroken;

    public static event Action OnWallCrackStart;
    public static event Action OnWallPunched;
    public static event Action OnWallBroken;

    public static event Action OnCatRevealed;
    public static event Action OnRhythmBattleStart;
    public static event Action OnRhythmBattleComplete;

    public static event Action OnNoteHit;
    /// <summary>misses so far, max misses allowed.</summary>
    public static event Action<int, int> OnNoteMissed;

    public static event Action OnPlayerLose;
    public static event Action OnPlayerWin;

    public static void RaiseMirrorCrackStart() => OnMirrorCrackStart?.Invoke();
    public static void RaiseMirrorBroken() => OnMirrorBroken?.Invoke();

    public static void RaiseWallCrackStart() => OnWallCrackStart?.Invoke();
    public static void RaiseWallPunched() => OnWallPunched?.Invoke();
    public static void RaiseWallBroken() => OnWallBroken?.Invoke();

    public static void RaiseCatRevealed() => OnCatRevealed?.Invoke();
    public static void RaiseRhythmBattleStart() => OnRhythmBattleStart?.Invoke();
    public static void RaiseRhythmBattleComplete() => OnRhythmBattleComplete?.Invoke();

    public static void RaiseNoteHit() => OnNoteHit?.Invoke();
    public static void RaiseNoteMissed(int misses, int maxMisses) => OnNoteMissed?.Invoke(misses, maxMisses);

    public static void RaisePlayerLose() => OnPlayerLose?.Invoke();
    public static void RaisePlayerWin() => OnPlayerWin?.Invoke();
}
