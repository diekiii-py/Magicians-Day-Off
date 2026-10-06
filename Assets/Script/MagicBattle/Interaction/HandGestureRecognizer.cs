using UnityEngine;

/// <summary>
/// Lightweight, dependency-free gesture classifier built directly on top of Meta's core
/// hand-tracking data (<see cref="OVRHand"/> + <see cref="OVRSkeleton"/>). No Interaction SDK
/// pose assets required: every finger's "curl" is measured as the tip-to-wrist distance
/// normalised by the hand's own knuckle span, so it scales with hand size automatically.
///
/// Attach this to the LeftHandAnchor/RightHandAnchor objects created by the Meta "Hand
/// Tracking" Building Block (the same object that already has OVRHand + OVRSkeleton).
/// </summary>
[RequireComponent(typeof(OVRHand))]
public class HandGestureRecognizer : MonoBehaviour
{
    [Tooltip("Normalised tip-to-wrist distance above which a finger counts as 'extended'.")]
    [Range(0.4f, 1.2f)]
    public float extendedThreshold = 0.75f;

    public GestureType CurrentGesture { get; private set; } = GestureType.None;
    public bool IsTracked { get; private set; }

    /// <summary>World-space position of the wrist, useful for aiming/punch checks.</summary>
    public Vector3 WristPosition { get; private set; }
    public Vector3 WristVelocity { get; private set; }

    OVRHand _hand;
    OVRSkeleton _skeleton;
    Vector3 _previousWristPosition;
    bool _hasPreviousPosition;

    bool _thumbExtended, _indexExtended, _middleExtended, _ringExtended, _pinkyExtended;

    void Awake()
    {
        _hand = GetComponent<OVRHand>();
        _skeleton = GetComponent<OVRSkeleton>();
    }

    void Update()
    {
        IsTracked = _hand != null && _hand.IsTracked && _skeleton != null && _skeleton.IsDataValid;
        if (!IsTracked)
        {
            CurrentGesture = GestureType.None;
            return;
        }

        if (!TryGetBones(out var wrist, out var knuckle, out var thumbTip, out var indexTip,
                out var middleTip, out var ringTip, out var pinkyTip))
        {
            CurrentGesture = GestureType.None;
            return;
        }

        WristPosition = wrist;

        float dt = Time.deltaTime;
        if (_hasPreviousPosition && dt > 0f)
        {
            WristVelocity = (wrist - _previousWristPosition) / dt;
        }
        _previousWristPosition = wrist;
        _hasPreviousPosition = true;

        // Knuckle span gives us a per-user, per-hand scale reference so the threshold
        // above works regardless of hand size or tracking distance.
        float handScale = Mathf.Max(Vector3.Distance(wrist, knuckle), 0.01f);

        _thumbExtended = Extended(wrist, thumbTip, handScale);
        _indexExtended = Extended(wrist, indexTip, handScale);
        _middleExtended = Extended(wrist, middleTip, handScale);
        _ringExtended = Extended(wrist, ringTip, handScale);
        _pinkyExtended = Extended(wrist, pinkyTip, handScale);

        CurrentGesture = Classify();
    }

    bool Extended(Vector3 wrist, Vector3 tip, float handScale)
    {
        float normalised = Vector3.Distance(wrist, tip) / handScale;
        return normalised >= extendedThreshold;
    }

    GestureType Classify()
    {
        int extendedCount = (_thumbExtended ? 1 : 0) + (_indexExtended ? 1 : 0) + (_middleExtended ? 1 : 0) +
                            (_ringExtended ? 1 : 0) + (_pinkyExtended ? 1 : 0);

        if (extendedCount == 0) return GestureType.Fist;
        if (extendedCount >= 4) return GestureType.OpenPalm;

        if (_indexExtended && !_middleExtended && !_ringExtended && !_pinkyExtended && !_thumbExtended)
            return GestureType.Point;

        if (_indexExtended && _middleExtended && !_ringExtended && !_pinkyExtended)
            return GestureType.Peace;

        if (_thumbExtended && !_indexExtended && !_middleExtended && !_ringExtended && !_pinkyExtended)
            return GestureType.ThumbsUp;

        return GestureType.None;
    }

    bool TryGetBones(out Vector3 wrist, out Vector3 knuckle, out Vector3 thumbTip, out Vector3 indexTip,
        out Vector3 middleTip, out Vector3 ringTip, out Vector3 pinkyTip)
    {
        wrist = knuckle = thumbTip = indexTip = middleTip = ringTip = pinkyTip = Vector3.zero;
        bool gotWrist = false, gotKnuckle = false, gotThumb = false, gotIndex = false, gotMiddle = false,
            gotRing = false, gotPinky = false;

        var bones = _skeleton.Bones;
        for (int i = 0; i < bones.Count; i++)
        {
            var bone = bones[i];
            if (bone?.Transform == null) continue;

            switch (bone.Id)
            {
                case OVRSkeleton.BoneId.Hand_WristRoot:
                    wrist = bone.Transform.position; gotWrist = true; break;
                case OVRSkeleton.BoneId.Hand_Middle1:
                    knuckle = bone.Transform.position; gotKnuckle = true; break;
                case OVRSkeleton.BoneId.Hand_ThumbTip:
                    thumbTip = bone.Transform.position; gotThumb = true; break;
                case OVRSkeleton.BoneId.Hand_IndexTip:
                    indexTip = bone.Transform.position; gotIndex = true; break;
                case OVRSkeleton.BoneId.Hand_MiddleTip:
                    middleTip = bone.Transform.position; gotMiddle = true; break;
                case OVRSkeleton.BoneId.Hand_RingTip:
                    ringTip = bone.Transform.position; gotRing = true; break;
                case OVRSkeleton.BoneId.Hand_PinkyTip:
                    pinkyTip = bone.Transform.position; gotPinky = true; break;
            }
        }

        return gotWrist && gotKnuckle && gotThumb && gotIndex && gotMiddle && gotRing && gotPinky;
    }

    /// <summary>Returns the colour associated with a gesture, used to tint note orbs.</summary>
    public static Color ColorForGesture(GestureType gesture)
    {
        switch (gesture)
        {
            case GestureType.Fist: return new Color(0.85f, 0.15f, 0.15f);
            case GestureType.OpenPalm: return new Color(0.15f, 0.45f, 0.95f);
            case GestureType.Point: return new Color(0.2f, 0.85f, 0.3f);
            case GestureType.ThumbsUp: return new Color(0.95f, 0.8f, 0.1f);
            case GestureType.Peace: return new Color(0.65f, 0.25f, 0.9f);
            default: return Color.white;
        }
    }
}
