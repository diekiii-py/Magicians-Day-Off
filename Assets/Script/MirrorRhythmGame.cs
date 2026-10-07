using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Self-contained gesture rhythm game. Coloured orbs fly from the cat toward the player; when an orb
/// reaches the hit point the player must be making the gesture shown on it (colour + label).
/// No prefabs, colliders, BeatMap asset or Interaction SDK needed. Orbs are built in code and hits are
/// judged by timing, so it only needs the HandGestureRecognizer components on your hand-tracking objects.
/// If the notes list is empty a default pattern is generated from bpm / noteCount.
/// </summary>
public class MirrorRhythmGame : MonoBehaviour
{
    [Serializable]
    public struct Note
    {
        [Tooltip("Seconds after the song starts at which the orb should reach the hit point.")]
        public float time;
        public GestureType gesture;
    }

    [Header("Hands")]
    public HandGestureRecognizer leftHand;
    public HandGestureRecognizer rightHand;

    [Header("Placement")]
    [Tooltip("Where orbs start (put it at the cat). If empty, 6 m in front of the player.")]
    public Transform spawnPoint;
    [Tooltip("Where orbs must be hit (about chest height in front of the player). If empty, 1 m in front of the player.")]
    public Transform hitPoint;
    public Transform playerHead;
    public float orbSize = 0.25f;

    [Header("Timing")]
    public float travelTime = 1.8f;
    [Tooltip("Seconds either side of the beat in which the gesture counts.")]
    public float hitWindow = 0.3f;
    public int maxMisses = 10;

    [Header("Notes")]
    public List<Note> notes = new List<Note>();
    public float bpm = 100f;
    public int noteCount = 32;
    public float firstNoteDelay = 3f;
    public GestureType[] defaultGestures = { GestureType.Fist, GestureType.OpenPalm, GestureType.Point };

    [Header("Feedback (optional)")]
    public AudioSource music;
    public AudioSource hitSound;
    public AudioSource missSound;
    public TMP_Text hudText;

    [Header("Events")]
    public UnityEvent onWin;
    public UnityEvent onLose;

    public int Score { get; private set; }
    public int Misses { get; private set; }

    class Orb
    {
        public GameObject go;
        public Note note;
        public Vector3 start, end;
    }

    bool _running;
    float _songStart;
    int _nextToSpawn;
    int _resolved;
    readonly List<Orb> _orbs = new List<Orb>();
    Vector3 _spawnPos, _hitPos;

    public void BeginSong()
    {
        if (_running) return;
        if (playerHead == null && Camera.main != null) playerHead = Camera.main.transform;

        if (notes == null || notes.Count == 0) GenerateDefaultNotes();
        notes.Sort((a, b) => a.time.CompareTo(b.time));

        ResolvePositions();
        Score = 0;
        Misses = 0;
        _nextToSpawn = 0;
        _resolved = 0;
        _songStart = Time.time;
        _running = true;

        if (music != null) music.Play();
        GameEvents.RaiseRhythmBattleStart();
        UpdateHud();
    }

    void GenerateDefaultNotes()
    {
        notes = new List<Note>();
        if (defaultGestures == null || defaultGestures.Length == 0) defaultGestures = new[] { GestureType.Fist };
        float beat = 60f / Mathf.Max(bpm, 1f);
        for (int i = 0; i < noteCount; i++)
        {
            notes.Add(new Note
            {
                time = firstNoteDelay + i * beat * 2f,
                gesture = defaultGestures[UnityEngine.Random.Range(0, defaultGestures.Length)]
            });
        }
    }

    void ResolvePositions()
    {
        Vector3 head = playerHead != null ? playerHead.position : transform.position;
        Vector3 forward = playerHead != null ? Vector3.ProjectOnPlane(playerHead.forward, Vector3.up).normalized : Vector3.forward;
        if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;

        _hitPos = hitPoint != null ? hitPoint.position : head + forward * 1f - Vector3.up * 0.2f;
        _spawnPos = spawnPoint != null ? spawnPoint.position : head + forward * 6f - Vector3.up * 0.2f;
    }

    void Update()
    {
        if (!_running) return;
        float now = Time.time - _songStart;

        while (_nextToSpawn < notes.Count && now >= notes[_nextToSpawn].time - travelTime)
        {
            SpawnOrb(notes[_nextToSpawn]);
            _nextToSpawn++;
        }

        for (int i = _orbs.Count - 1; i >= 0; i--)
        {
            var orb = _orbs[i];
            float t = (now - (orb.note.time - travelTime)) / travelTime;
            orb.go.transform.position = Vector3.LerpUnclamped(orb.start, orb.end, t);

            float delta = now - orb.note.time;
            if (Mathf.Abs(delta) <= hitWindow && PlayerPerforming(orb.note.gesture))
            {
                Resolve(i, true);
            }
            else if (delta > hitWindow)
            {
                Resolve(i, false);
            }

            if (!_running) return;
        }

        if (_nextToSpawn >= notes.Count && _orbs.Count == 0) Finish(true);
    }

    bool PlayerPerforming(GestureType gesture)
    {
        return (leftHand != null && leftHand.IsTracked && leftHand.CurrentGesture == gesture) ||
               (rightHand != null && rightHand.IsTracked && rightHand.CurrentGesture == gesture);
    }

    void SpawnOrb(Note note)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "RhythmOrb_" + note.gesture;
        go.transform.localScale = Vector3.one * orbSize;
        Destroy(go.GetComponent<Collider>());

        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        var mat = new Material(shader);
        Color c = HandGestureRecognizer.ColorForGesture(note.gesture);
        mat.color = c;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
        go.GetComponent<Renderer>().material = mat;

        var labelObj = new GameObject("Label");
        labelObj.transform.SetParent(go.transform, false);
        labelObj.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        labelObj.transform.localScale = Vector3.one * 0.35f;
        var label = labelObj.AddComponent<TextMesh>();
        label.text = note.gesture.ToString();
        label.anchor = TextAnchor.MiddleCenter;
        label.fontSize = 48;
        label.characterSize = 0.1f;
        label.color = Color.white;
        labelObj.AddComponent<LabelBillboard>().target = playerHead;

        _orbs.Add(new Orb { go = go, note = note, start = _spawnPos, end = _hitPos + (_hitPos - _spawnPos).normalized * 0.8f });
        go.transform.position = _spawnPos;
    }

    void Resolve(int index, bool hit)
    {
        var orb = _orbs[index];
        _orbs.RemoveAt(index);
        Destroy(orb.go);
        _resolved++;

        if (hit)
        {
            Score++;
            if (hitSound != null) hitSound.Play();
            GameEvents.RaiseNoteHit();
        }
        else
        {
            Misses++;
            if (missSound != null) missSound.Play();
            GameEvents.RaiseNoteMissed(Misses, maxMisses);
        }

        UpdateHud();
        if (Misses >= maxMisses) Finish(false);
    }

    void Finish(bool won)
    {
        if (!_running) return;
        _running = false;
        foreach (var o in _orbs) if (o.go != null) Destroy(o.go);
        _orbs.Clear();
        if (music != null) music.Stop();

        if (hudText != null) hudText.text = won ? "You win!  " + Score + " hits" : "Game over";
        if (won)
        {
            GameEvents.RaiseRhythmBattleComplete();
            GameEvents.RaisePlayerWin();
            onWin?.Invoke();
        }
        else
        {
            GameEvents.RaisePlayerLose();
            onLose?.Invoke();
        }
    }

    void UpdateHud()
    {
        if (hudText != null) hudText.text = "Hits " + Score + "   Misses " + Misses + "/" + maxMisses;
    }

    /// <summary>Keeps the gesture label readable by turning it toward the player.</summary>
    class LabelBillboard : MonoBehaviour
    {
        public Transform target;

        void LateUpdate()
        {
            if (target == null) return;
            transform.rotation = Quaternion.LookRotation(transform.position - target.position);
        }
    }
}
