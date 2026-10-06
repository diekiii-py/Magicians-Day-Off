using UnityEngine;

/// <summary>
/// Default, always-available wall locator: simply uses a Transform you position in the
/// Editor (or calibrate at runtime, see <see cref="CalibrateInFrontOfPlayer"/>). This is what
/// the scene uses out of the box so the whole sequence works in Play Mode / on any device even
/// without a MR Utility Kit room scan.
/// </summary>
public class ManualWallLocator : MonoBehaviour, IWallLocator
{
    [Tooltip("Transform placed at the wall. Its blue axis (forward) must point INTO the room.")]
    public Transform wallMarker;
    public Vector2 wallSize = new Vector2(2.5f, 2.5f);

    public Vector3 WallCenter => wallMarker != null ? wallMarker.position : transform.position;
    public Vector3 WallNormal => wallMarker != null ? wallMarker.forward : transform.forward;
    public Vector2 WallSize => wallSize;
    public bool HasWall => wallMarker != null;

    [Header("Optional runtime calibration")]
    [Tooltip("Reference used to compute 'in front of the player', typically CenterEyeAnchor.")]
    public Transform playerHead;
    [Tooltip("Distance in front of the player to place the wall when calibrating.")]
    public float calibrationDistance = 1.0f;

    /// <summary>
    /// Moves the wall marker to a point directly in front of (and facing) the player, with
    /// pitch/roll removed so the wall stays vertical. Handy for quick on-device placement:
    /// stand facing the real wall and call this once (e.g. from a calibration button/gesture).
    /// </summary>
    [ContextMenu("Calibrate In Front Of Player")]
    public void CalibrateInFrontOfPlayer()
    {
        if (wallMarker == null || playerHead == null) return;

        Vector3 flatForward = playerHead.forward;
        flatForward.y = 0f;
        if (flatForward.sqrMagnitude < 0.0001f) flatForward = Vector3.forward;
        flatForward.Normalize();

        wallMarker.position = playerHead.position + flatForward * calibrationDistance;
        wallMarker.rotation = Quaternion.LookRotation(-flatForward, Vector3.up);
    }
}
