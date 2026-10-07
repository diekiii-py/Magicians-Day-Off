using System.Collections;
using TMPro;
using UnityEngine;
using Oculus.Interaction;

/// <summary>
/// Scene flow for "Magician's Day Off":
/// look in mirror -> Realcam hides, BrokenMirror + Fakecam appear -> cat angrily says the hat was
/// stolen -> a sound plays from the wall and "BREAK" is shown on it -> player makes a fist toward the
/// wall -> wall swaps for the broken wall -> rhythm game starts (same RhythmGameManager as Magic Battle).
///
/// Everything is assigned by dragging objects into the inspector.
/// Fist detection uses the Meta Interaction SDK: drag an IActiveState (e.g. a ShapeRecognizerActiveState
/// built from a HandPose/FistShape recipe, see "Building a Hand Pose Recognizer") into each fist slot.
/// If no ISDK state is assigned for a hand, the project's HandGestureRecognizer is used as a fallback.
/// </summary>
public class MirrorWallSequence : MonoBehaviour
{
    [Header("Mirror")]
    [Tooltip("Mirror object the player must look at (should have a Collider or Renderer).")]
    public Transform mirror;
    public Transform playerHead;
    [Range(1f, 45f)] public float lookAngle = 15f;
    public float lookDuration = 1.5f;
    public float maxLookDistance = 4f;

    [Header("Spot trigger (replaces looking at the mirror)")]
    [Tooltip("Empty object marking the teleport spot. When assigned, standing here starts the sequence instead of looking at the mirror.")]
    public Transform triggerSpot;
    public float spotRadius = 0.75f;
    public float spotDuration = 5f;

    [Header("Mirror swap")]
    public GameObject realCam;
    public GameObject brokenMirror;
    public GameObject fakeCam;
    public AudioSource mirrorBreakAudio;

    [Header("Cat")]
    public GameObject catObject; // Neko Cat02
    public CatController cat; // optional, for the cast animation
    public AudioSource catVoice;
    public TMP_Text catSubtitle; // optional, world-space or HUD text
    [TextArea] public string catLine = "You stole my hat!";
    public float catLineDuration = 4f;
    public float catDelay = 1.0f;

    [Header("Wall")]
    public GameObject wall;
    public GameObject brokenWall;
    [Tooltip("Text object on the wall that reads 'BREAK'.")]
    public GameObject breakText;
    public AudioSource wallSound; // place on/near the wall so it plays from there
    public float wallSoundDelay = 0.5f;
    public AudioSource wallBreakAudio;

    [Header("Fist detection (Meta ISDK)")]
    [Tooltip("Must implement IActiveState (e.g. ShapeRecognizerActiveState for Fist).")]
    public MonoBehaviour leftFistState;
    public MonoBehaviour rightFistState;
    public HandGestureRecognizer leftHandFallback;
    public HandGestureRecognizer rightHandFallback;
    public Transform leftHand;
    public Transform rightHand;
    [Tooltip("Hand must be within this distance of the wall for the fist to count.")]
    public float fistRange = 1.0f;
    public float fistHoldTime = 0.2f;

    [Header("Rhythm game")]
    public MirrorRhythmGame rhythmGame;
    [Tooltip("Optional: disabled when the rhythm game starts so Point can't teleport mid-song.")]
    public GestureTeleport teleport;
    public float rhythmStartDelay = 2f;

    enum Stage { WaitingForLook, CatSpeaking, WaitingForFist, Done }
    Stage _stage = Stage.WaitingForLook;
    float _lookTimer;
    float _fistTimer;

    void Awake()
    {
        // Mirror/cat cameras that render to a RenderTexture must not target the headset,
        // otherwise they hijack the stereo output and the VR view gets stuck.
        foreach (var c in FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (c.targetTexture != null) c.stereoTargetEye = StereoTargetEyeMask.None;
        }
    }

    void Start()
    {
        // The inspector's Player Head may point at a non-tracked object, so prefer the real headset camera.
        var ovrRig = FindFirstObjectByType<OVRCameraRig>();
        if (ovrRig != null && ovrRig.centerEyeAnchor != null) playerHead = ovrRig.centerEyeAnchor;
        else if (playerHead == null && Camera.main != null) playerHead = Camera.main.transform;
        if (triggerSpot == null)
            Debug.LogWarning("MirrorWallSequence: Trigger Spot is not assigned (or the scene wasn't saved); falling back to looking at the mirror.", this);
        // Initial active states are left exactly as set in the scene (disable BrokenMirror,
        // Fakecam, BrokenWall and BreakText in the Hierarchy yourself).
    }

    void Update()
    {
        switch (_stage)
        {
            case Stage.WaitingForLook:
                UpdateLook();
                break;
            case Stage.WaitingForFist:
                UpdateFist();
                break;
        }
    }

    void UpdateLook()
    {
        if (playerHead == null) return;

        bool triggered;
        if (triggerSpot != null)
        {
            Vector3 flat = playerHead.position - triggerSpot.position;
            flat.y = 0f;
            bool onSpot = flat.magnitude <= spotRadius;
            _lookTimer = onSpot ? _lookTimer + Time.deltaTime : 0f;
            triggered = _lookTimer >= spotDuration;
        }
        else
        {
            if (mirror == null) return;
            Vector3 toMirror = mirror.position - playerHead.position;
            bool looking = toMirror.magnitude <= maxLookDistance &&
                           Vector3.Angle(playerHead.forward, toMirror) <= lookAngle;
            _lookTimer = looking ? _lookTimer + Time.deltaTime : 0f;
            triggered = _lookTimer >= lookDuration;
        }

        if (triggered)
        {
            _stage = Stage.CatSpeaking;
            StartCoroutine(MirrorRoutine());
        }
    }

    IEnumerator MirrorRoutine()
    {
        if (realCam != null) realCam.SetActive(false);
        if (brokenMirror != null) brokenMirror.SetActive(true);
        if (fakeCam != null) fakeCam.SetActive(true);
        if (mirrorBreakAudio != null) mirrorBreakAudio.Play();

        yield return new WaitForSeconds(catDelay);

        if (catObject != null) catObject.SetActive(true);
        if (catVoice != null) catVoice.Play();
        if (catSubtitle != null)
        {
            catSubtitle.text = catLine;
            catSubtitle.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(catLineDuration);
        if (catSubtitle != null) catSubtitle.gameObject.SetActive(false);

        yield return new WaitForSeconds(wallSoundDelay);
        if (wallSound != null) wallSound.Play();
        if (breakText != null) breakText.SetActive(true);

        _fistTimer = 0f;
        _stage = Stage.WaitingForFist;
    }

    void UpdateFist()
    {
        bool fist = HandIsFist(leftFistState, leftHandFallback, leftHand) ||
                    HandIsFist(rightFistState, rightHandFallback, rightHand);

        _fistTimer = fist ? _fistTimer + Time.deltaTime : 0f;
        if (_fistTimer >= fistHoldTime)
        {
            _stage = Stage.Done;
            StartCoroutine(WallBreakRoutine());
        }
    }

    bool HandIsFist(MonoBehaviour isdkState, HandGestureRecognizer fallback, Transform hand)
    {
        bool fist;
        if (isdkState is IActiveState active) fist = active.Active;
        else if (fallback != null) fist = fallback.IsTracked && fallback.CurrentGesture == GestureType.Fist;
        else return false;

        return fist && IsNearWall(hand);
    }

    bool IsNearWall(Transform hand)
    {
        if (wall == null || hand == null) return true; // no range check possible
        Vector3 closest = wall.transform.position;
        var col = wall.GetComponentInChildren<Collider>();
        if (col != null) closest = col.ClosestPoint(hand.position);
        else
        {
            var rend = wall.GetComponentInChildren<Renderer>();
            if (rend != null) closest = rend.bounds.ClosestPoint(hand.position);
        }
        return Vector3.Distance(hand.position, closest) <= fistRange;
    }

    IEnumerator WallBreakRoutine()
    {
        if (breakText != null) breakText.SetActive(false);
        if (wall != null) wall.SetActive(false);
        if (brokenWall != null) brokenWall.SetActive(true);
        if (wallBreakAudio != null) wallBreakAudio.Play();
        GameEvents.RaiseWallBroken();

        yield return new WaitForSeconds(rhythmStartDelay);

        if (teleport != null) teleport.teleportEnabled = false;
        if (rhythmGame != null) rhythmGame.BeginSong();
        if (cat != null) cat.PlayCastAnimation();
    }
}
