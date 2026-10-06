using UnityEngine;

/// <summary>
/// Classic planar-mirror-camera trick: positions <see cref="mirrorCamera"/> as the reflection
/// of the player's head across the mirror plane every frame and renders it into a RenderTexture
/// that is applied to the mirror's material (see Assets/mirror-frame/MIrrorMat.mat +
/// Mirror.renderTexture, already set up in this project).
///
/// Note: this reflects the *virtual* scene (hands, cat, UI, etc.), not the real passthrough
/// camera feed - Quest does not expose a simple "selfie" camera API. This is the same technique
/// used by most Quest mirror props and reads correctly through passthrough because anything
/// rendered opaque on the quad occludes the passthrough underlay behind it.
/// </summary>
public class MirrorReflectionCamera : MonoBehaviour
{
    [Tooltip("The camera that represents the player's view (OVRCameraRig's CenterEyeAnchor camera).")]
    public Camera playerCamera;

    [Tooltip("The camera that renders the reflection into the mirror RenderTexture.")]
    public Camera mirrorCamera;

    [Tooltip("Plane the mirror lives on. Its blue axis (forward) must point out of the mirror towards the room.")]
    public Transform mirrorPlane;

    void LateUpdate()
    {
        if (playerCamera == null || mirrorCamera == null || mirrorPlane == null) return;

        Vector3 planeNormal = mirrorPlane.forward;
        Vector3 planePoint = mirrorPlane.position;

        // Reflect the player camera's position across the mirror plane.
        Vector3 toCamera = playerCamera.transform.position - planePoint;
        float distance = Vector3.Dot(toCamera, planeNormal);
        Vector3 reflectedPosition = playerCamera.transform.position - 2f * distance * planeNormal;

        // Reflect the forward/up directions the same way.
        Vector3 forward = playerCamera.transform.forward;
        Vector3 up = playerCamera.transform.up;
        Vector3 reflectedForward = forward - 2f * Vector3.Dot(forward, planeNormal) * planeNormal;
        Vector3 reflectedUp = up - 2f * Vector3.Dot(up, planeNormal) * planeNormal;

        mirrorCamera.transform.position = reflectedPosition;
        mirrorCamera.transform.rotation = Quaternion.LookRotation(-reflectedForward, reflectedUp);
        mirrorCamera.fieldOfView = playerCamera.fieldOfView;
        mirrorCamera.nearClipPlane = playerCamera.nearClipPlane;
        mirrorCamera.farClipPlane = playerCamera.farClipPlane;
    }
}
