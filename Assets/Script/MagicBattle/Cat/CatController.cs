using UnityEngine;

/// <summary>
/// Cat state machine: hidden -> revealed/angry (after the mirror breaks) -> casting spells ->
/// stepping closer on every missed note -> either reaching the player (lose) or being fought
/// off until the song ends (win). Animator parameters are optional (triggers "Reveal", "Cast",
/// "Step", bool "Angry") - if no Animator is present the cat still moves/functions correctly.
/// </summary>
public class CatController : MonoBehaviour
{
    [Tooltip("Where the cat starts, deep in the forest.")]
    public Transform forestPosition;
    [Tooltip("Where the cat ends up if it reaches the player (game over position).")]
    public Transform playerPosition;
    [Tooltip("Must match RhythmGameManager.maxMisses so each miss advances the cat by the same fraction.")]
    public int maxMisses = 20;

    public Animator animator;

    int _missesSoFar;

    void OnEnable()
    {
        GameEvents.OnCatRevealed += HandleRevealed;
        GameEvents.OnNoteMissed += HandleNoteMissed;
    }

    void OnDisable()
    {
        GameEvents.OnCatRevealed -= HandleRevealed;
        GameEvents.OnNoteMissed -= HandleNoteMissed;
    }

    void HandleRevealed()
    {
        if (animator != null) animator.SetTrigger("Reveal");
    }

    void HandleNoteMissed(int misses, int configuredMax)
    {
        maxMisses = configuredMax;
        _missesSoFar = misses;
        AdvanceTowardPlayer();
    }

    void AdvanceTowardPlayer()
    {
        if (forestPosition == null || playerPosition == null || maxMisses <= 0) return;

        float t = Mathf.Clamp01((float)_missesSoFar / maxMisses);
        transform.position = Vector3.Lerp(forestPosition.position, playerPosition.position, t);
        transform.LookAt(playerPosition.position + Vector3.up * (playerPosition.position.y - transform.position.y));

        if (animator != null) animator.SetTrigger("Step");
    }

    public void PlayCastAnimation()
    {
        if (animator != null) animator.SetTrigger("Cast");
    }

    /// <summary>Snaps the cat back to its forest starting point (used on scene reset).</summary>
    public void ResetCat()
    {
        _missesSoFar = 0;
        if (forestPosition != null) transform.position = forestPosition.position;
    }
}
