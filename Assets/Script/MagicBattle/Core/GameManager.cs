using System.Collections;
using UnityEngine;

/// <summary>
/// Top-level state machine for the whole "Magician's Day Off" sequence:
/// Intro -> mirror cracks -> mirror shatters / cat revealed -> wall cracks -> player punches
/// the wall -> wall breaks / forest revealed -> rhythm battle -> win or lose.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Timing")]
    public float introDuration = 3f;
    [Tooltip("Delay after the mirror breaks before the wall starts cracking.")]
    public float postMirrorDelay = 2.5f;
    [Tooltip("How long the wall creaks/cracks before the punch becomes possible.")]
    public float wallCrackWarmup = 4f;
    [Tooltip("Delay after the wall breaks before the rhythm battle begins.")]
    public float postWallDelay = 2f;

    [Header("Systems")]
    public MirrorBreakSequence mirror;
    public WallCrackWarning wallCrackWarning;
    public WallBreakController wallBreakController;
    public RhythmGameManager rhythmGame;
    public CatController cat;

    [Header("Wall crack warning placement")]
    public MonoBehaviour wallLocatorComponent; // must implement IWallLocator, used to position the crack decal/audio

    void OnEnable()
    {
        GameEvents.OnMirrorBroken += HandleMirrorBroken;
        GameEvents.OnWallBroken += HandleWallBroken;
        GameEvents.OnPlayerLose += HandleGameOver;
        GameEvents.OnPlayerWin += HandleGameOver;
    }

    void OnDisable()
    {
        GameEvents.OnMirrorBroken -= HandleMirrorBroken;
        GameEvents.OnWallBroken -= HandleWallBroken;
        GameEvents.OnPlayerLose -= HandleGameOver;
        GameEvents.OnPlayerWin -= HandleGameOver;
    }

    void Start()
    {
        StartCoroutine(IntroRoutine());
    }

    IEnumerator IntroRoutine()
    {
        yield return new WaitForSeconds(introDuration);
        if (mirror != null) mirror.BeginSequence();
    }

    void HandleMirrorBroken()
    {
        StartCoroutine(WallCrackRoutine());
    }

    IEnumerator WallCrackRoutine()
    {
        yield return new WaitForSeconds(postMirrorDelay);

        var locator = wallLocatorComponent as IWallLocator;
        Vector3 pos = locator != null && locator.HasWall ? locator.WallCenter : transform.position;
        Quaternion rot = locator != null && locator.HasWall ? Quaternion.LookRotation(locator.WallNormal) : Quaternion.identity;

        if (wallCrackWarning != null) wallCrackWarning.BeginWarning(pos, rot);

        yield return new WaitForSeconds(wallCrackWarmup);

        if (wallBreakController != null) wallBreakController.Arm();
    }

    void HandleWallBroken()
    {
        if (wallCrackWarning != null) wallCrackWarning.StopWarning();
        StartCoroutine(RhythmStartRoutine());
    }

    IEnumerator RhythmStartRoutine()
    {
        yield return new WaitForSeconds(postWallDelay);
        if (rhythmGame != null) rhythmGame.BeginSong();
        if (cat != null) cat.PlayCastAnimation();
    }

    void HandleGameOver()
    {
        // Hook for stopping ambience/music or disabling further interaction if desired.
    }
}
