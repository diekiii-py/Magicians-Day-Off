using UnityEngine;

/// <summary>Makes a world-space UI canvas/text always face the player's head.</summary>
public class FaceCamera : MonoBehaviour
{
    public Transform cameraToFace;
    public bool lockUpright = true;

    void LateUpdate()
    {
        var cam = cameraToFace != null ? cameraToFace : Camera.main != null ? Camera.main.transform : null;
        if (cam == null) return;

        Vector3 dir = transform.position - cam.position;
        if (lockUpright) dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;
        transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }
}
