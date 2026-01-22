using System;
using UnityEngine;
using Unity.XR.CoreUtils; // Needed for XROrigin

public class RecenterYawOnly : MonoBehaviour
{
    public Transform rigTransform;      // The parent of the camera (your XR rig root)
    public Transform cameraTransform; // Reference to your XR Camera (usually the "Main Camera" under XR Origin)

    private void OnEnable()
    {
        SettingsGUI.RecenterPose += RecenterYaw;
        OscManager.ReceiveRecenterPose += RecenterYaw;
    }

    private void OnDisable()
    {
        SettingsGUI.RecenterPose -= RecenterYaw;
        OscManager.ReceiveRecenterPose -= RecenterYaw;
    }

    private void Update()
    {
        if (Input.GetKeyDown("n")) RecenterYaw();
    }

    public void RecenterYaw()
    {
        // Get the headset's forward direction, flattened on the horizontal plane
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        // Calculate the yaw angle between the headset's forward and world forward
        float yawOffset = Vector3.SignedAngle(cameraForward, Vector3.forward, Vector3.up);

        // Rotate the rig around the camera's position by the negative yaw offset
        rigTransform.RotateAround(cameraTransform.position, Vector3.up, yawOffset);
    }
}