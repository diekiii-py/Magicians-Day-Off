using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orchestrates the Beat-Saber-style cat spell battle: spawns notes from the beatmap timed to
/// arrive at the hit zone, checks the player's current hand gesture against each note's
/// requirement, and reports hits/misses. Reaching <see cref="maxMisses"/> misses loses the game;
/// finishing the beatmap without doing so wins it.
/// </summary>
public class RhythmGameManager : MonoBehaviour
{
    public BeatMap beatMap;
    public Transform spawnPoint;
    public Transform hitZonePoint;
    public GameObject noteOrbPrefab;

    public HandGestureRecognizer leftHand;
    public HandGestureRecognizer rightHand;

    [Tooltip("Game over once this many notes have been missed.")]
    public int maxMisses = 20;
    [Tooltip("How far past the hit zone a note travels before counting as missed, in meters.")]
    public float overshootDistance = 1.0f;

    public int Score { get; private set; }
    public int Misses { get; private set; }

    bool _running;
    bool _ended;
    readonly List<NoteController> _liveNotes = new();

    public void BeginSong()
    {
        if (_running) return;
        _running = true;
        _ended = false;
        Score = 0;
        Misses = 0;
        GameEvents.RaiseRhythmBattleStart();
        StartCoroutine(SongRoutine());
    }

    IEnumerator SongRoutine()
    {
        if (beatMap == null || beatMap.notes == null || beatMap.notes.Length == 0)
        {
            Debug.LogWarning("RhythmGameManager: no BeatMap assigned, nothing to play.");
            yield break;
        }

        float songStart = Time.time;
        foreach (var note in beatMap.notes)
        {
            float spawnAt = songStart + note.time - beatMap.travelTime;
            float waitFor = spawnAt - Time.time;
            if (waitFor > 0f) yield return new WaitForSeconds(waitFor);

            if (_ended) yield break;
            SpawnNote(note);
        }

        // Wait for the last note's travel time plus a buffer before declaring completion.
        yield return new WaitForSeconds(beatMap.travelTime + 0.5f);

        if (!_ended)
        {
            _ended = true;
            GameEvents.RaiseRhythmBattleComplete();
            GameEvents.RaisePlayerWin();
        }
    }

    void SpawnNote(NoteData data)
    {
        if (noteOrbPrefab == null || spawnPoint == null || hitZonePoint == null) return;

        var obj = Instantiate(noteOrbPrefab, spawnPoint.position, Quaternion.identity);
        var note = obj.GetComponent<NoteController>();
        if (note == null) note = obj.AddComponent<NoteController>();

        Vector3 direction = (hitZonePoint.position - spawnPoint.position).normalized;
        Vector3 end = hitZonePoint.position + direction * overshootDistance;

        note.Launch(spawnPoint.position, end, data.requiredGesture, beatMap.travelTime, this);
        _liveNotes.Add(note);
    }

    public bool PlayerIsPerforming(GestureType gesture)
    {
        if (leftHand != null && leftHand.IsTracked && leftHand.CurrentGesture == gesture) return true;
        if (rightHand != null && rightHand.IsTracked && rightHand.CurrentGesture == gesture) return true;
        return false;
    }

    public void NotifyHit(NoteController note)
    {
        _liveNotes.Remove(note);
        Score++;
        GameEvents.RaiseNoteHit();
    }

    public void NotifyMiss(NoteController note)
    {
        _liveNotes.Remove(note);
        Misses++;
        GameEvents.RaiseNoteMissed(Misses, maxMisses);

        if (!_ended && Misses >= maxMisses)
        {
            _ended = true;
            StopAllCoroutines();
            foreach (var n in _liveNotes) if (n != null) Destroy(n.gameObject);
            _liveNotes.Clear();
            GameEvents.RaisePlayerLose();
        }
    }
}
