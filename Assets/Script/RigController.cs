using UnityEngine;

public class RigController : MonoBehaviour
{
    public Transform vrCamera;

    public Transform headIKTarget;
    public Transform kyleRig;
    public Transform headBone;

    public Transform leftIKTarget;
    public Transform leftHand;

    public Transform rightIKTarget;
    public Transform rightHand;

    void LateUpdate()
    {
        if (vrCamera == null)
        {
            var rig = FindFirstObjectByType<OVRCameraRig>();
            if (rig != null) vrCamera = rig.centerEyeAnchor;
            if (vrCamera == null) return;
        }

        // headtracking
        kyleRig.position += (headIKTarget.position - headBone.position);
        kyleRig.rotation = Quaternion.Euler(0f, vrCamera.rotation.eulerAngles.y, 0f);

        // follow hands
        leftIKTarget.SetPositionAndRotation(leftHand.position, leftHand.rotation);
        rightIKTarget.SetPositionAndRotation(rightHand.position, rightHand.rotation);
    }
}
