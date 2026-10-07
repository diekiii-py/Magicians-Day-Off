using UnityEngine;

/// <summary>
/// Teleport with hand gestures only, on the existing OVRCameraRig + Hand Tracking building blocks.
/// Hold a POINT gesture to aim (an arc and a ring show where you will land); stop pointing
/// (open hand or make a fist) to teleport. Aiming goes from the wrist through the index fingertip.
/// The floor needs a Collider on a layer included in <see cref="floorMask"/>.
/// </summary>
public class GestureTeleport : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Root of the OVRCameraRig. This is the object that gets moved.")]
    public Transform rigRoot;
    public Transform playerHead;
    public HandGestureRecognizer leftHand;
    public HandGestureRecognizer rightHand;

    [Header("Aiming")]
    public LayerMask floorMask = ~0;
    public float maxDistance = 8f;
    [Tooltip("Surfaces steeper than this (degrees from flat) are not valid floors.")]
    public float maxSlope = 30f;
    public float arcHeight = 1.0f;
    public int arcSegments = 24;

    [Header("Feel")]
    [Tooltip("Pointing must be held this long before the aim starts, so quick flicks don't teleport.")]
    public float aimHoldTime = 0.25f;
    public Color validColor = new Color(0.2f, 0.9f, 0.4f);
    public Color invalidColor = new Color(0.9f, 0.2f, 0.2f);
    public AudioSource teleportSound;

    [Tooltip("Turn off to block teleporting (for example during the rhythm game).")]
    public bool teleportEnabled = true;

    LineRenderer _line;
    Transform _reticle;
    HandGestureRecognizer _aimHand;
    float _pointTimer;
    bool _aiming;
    bool _hasTarget;
    Vector3 _target;

    void Awake()
    {
        if (playerHead == null && Camera.main != null) playerHead = Camera.main.transform;
        if (rigRoot == null)
        {
            var rig = FindFirstObjectByType<OVRCameraRig>();
            if (rig != null) rigRoot = rig.transform;
        }

        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        _line = new GameObject("TeleportArc").AddComponent<LineRenderer>();
        _line.transform.SetParent(transform, false);
        _line.material = new Material(shader);
        _line.widthMultiplier = 0.015f;
        _line.positionCount = arcSegments + 1;
        _line.useWorldSpace = true;
        _line.enabled = false;

        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "TeleportReticle";
        Destroy(ring.GetComponent<Collider>());
        ring.transform.SetParent(transform, false);
        ring.transform.localScale = new Vector3(0.5f, 0.005f, 0.5f);
        ring.GetComponent<Renderer>().material = new Material(shader);
        _reticle = ring.transform;
        _reticle.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!teleportEnabled || rigRoot == null)
        {
            StopAiming(false);
            return;
        }

        var hand = PointingHand();
        if (hand != null)
        {
            _pointTimer += Time.deltaTime;
            if (_pointTimer >= aimHoldTime)
            {
                _aimHand = hand;
                _aiming = true;
                UpdateAim(hand);
            }
        }
        else
        {
            bool teleport = _aiming && _hasTarget;
            StopAiming(teleport);
        }
    }

    HandGestureRecognizer PointingHand()
    {
        if (_aimHand != null && IsPointing(_aimHand)) return _aimHand;
        if (IsPointing(rightHand)) return rightHand;
        if (IsPointing(leftHand)) return leftHand;
        return null;
    }

    static bool IsPointing(HandGestureRecognizer h)
    {
        return h != null && h.IsTracked && h.CurrentGesture == GestureType.Point;
    }

    void UpdateAim(HandGestureRecognizer hand)
    {
        Vector3 origin = hand.IndexTipPosition;
        Vector3 dir = (hand.IndexTipPosition - hand.WristPosition).normalized;

        // Walk along a drooping arc and find where it first hits a valid floor.
        _hasTarget = false;
        Vector3 prev = origin;
        Vector3 hit = origin;
        for (int i = 0; i <= arcSegments; i++)
        {
            float t = i / (float)arcSegments;
            Vector3 p = origin + dir * (maxDistance * t) + Vector3.down * (arcHeight * 2f * t * t * maxDistance * 0.25f);
            _line.SetPosition(i, p);

            if (i > 0 && Physics.Linecast(prev, p, out var info, floorMask, QueryTriggerInteraction.Ignore))
            {
                hit = info.point;
                _hasTarget = Vector3.Angle(info.normal, Vector3.up) <= maxSlope;
                _target = info.point;
                for (int j = i; j <= arcSegments; j++) _line.SetPosition(j, info.point);
                break;
            }
            prev = p;
        }

        Color c = _hasTarget ? validColor : invalidColor;
        _line.startColor = _line.endColor = c;
        _line.enabled = true;

        _reticle.gameObject.SetActive(true);
        _reticle.position = hit + Vector3.up * 0.01f;
        _reticle.GetComponent<Renderer>().material.color = c;
    }

    void StopAiming(bool teleport)
    {
        if (teleport) TeleportTo(_target);
        _pointTimer = 0f;
        _aiming = false;
        _hasTarget = false;
        _aimHand = null;
        if (_line != null) _line.enabled = false;
        if (_reticle != null) _reticle.gameObject.SetActive(false);
    }

    void TeleportTo(Vector3 floorPoint)
    {
        // Move the rig so the player's head ends up above the target, keeping the real height.
        Vector3 head = playerHead != null ? playerHead.position : rigRoot.position;
        Vector3 offset = floorPoint - new Vector3(head.x, rigRoot.position.y, head.z);
        offset.y = floorPoint.y - rigRoot.position.y;
        rigRoot.position += offset;
        if (teleportSound != null) teleportSound.Play();
    }
}
