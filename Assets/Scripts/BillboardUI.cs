using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Transform cameraTransform;

    private void LateUpdate()
    {
        if (cameraTransform == null)
        {
            FindCamera(); // Dynamically find the camera if it's not assigned
        }

        if (cameraTransform != null)
        {
            // Make the UI face the camera
            transform.LookAt(transform.position + cameraTransform.forward);
        }
    }

    private void FindCamera()
    {
        Camera[] allCameras = Camera.allCameras;
        if (allCameras.Length > 0)
        {
            cameraTransform = allCameras[0].transform; // Use the first available camera
        }
    }
}
