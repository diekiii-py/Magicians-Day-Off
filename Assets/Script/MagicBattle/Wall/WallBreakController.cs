using UnityEngine;

/// <summary>
/// Listens to both hands and triggers the wall break once the player throws a fist punch
/// (closed fist + fast hand velocity) aimed at the wall. Enable <see cref="armed"/> once the
/// crack warning has played (GameManager does this automatically).
/// </summary>
public class WallBreakController : MonoBehaviour
{
    public HandGestureRecognizer leftHand;
    public HandGestureRecognizer rightHand;

    [Tooltip("Component describing where the wall is - ManualWallLocator by default, or a MRUKWallLocator on real hardware.")]
    public MonoBehaviour wallLocatorComponent; // must implement IWallLocator
    public WallBreakEffect breakEffect;

    [Header("Punch detection")]
    public float punchSpeedThreshold = 1.8f; // m/s
    public float punchRange = 0.9f; // meters, max distance from wall plane
    [Range(0f, 1f)] public float aimDotThreshold = 0.5f;

    public bool armed { get; private set; }
    bool _broken;

    IWallLocator Locator => wallLocatorComponent as IWallLocator;

    public void Arm()
    {
        armed = true;
    }

    public void Disarm()
    {
        armed = false;
    }

    void Update()
    {
        if (!armed || _broken) return;
        var locator = Locator;
        if (locator == null || !locator.HasWall) return;

        if (CheckHandPunch(leftHand, locator) || CheckHandPunch(rightHand, locator))
        {
            TriggerBreak(locator);
        }
    }

    bool CheckHandPunch(HandGestureRecognizer hand, IWallLocator locator)
    {
        if (hand == null || !hand.IsTracked) return false;
        if (hand.CurrentGesture != GestureType.Fist) return false;

        float speed = hand.WristVelocity.magnitude;
        if (speed < punchSpeedThreshold) return false;

        Vector3 toWall = locator.WallCenter - hand.WristPosition;
        float planeDistance = Vector3.Dot(toWall, locator.WallNormal);
        if (Mathf.Abs(planeDistance) > punchRange) return false;

        Vector3 velocityDir = hand.WristVelocity.normalized;
        float aimDot = Vector3.Dot(velocityDir, locator.WallNormal);
        return aimDot >= aimDotThreshold;
    }

    void TriggerBreak(IWallLocator locator)
    {
        _broken = true;
        armed = false;
        GameEvents.RaiseWallPunched();
        if (breakEffect != null) breakEffect.Break(locator);
    }
}
