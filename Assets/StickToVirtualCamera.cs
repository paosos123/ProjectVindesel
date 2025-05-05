using UnityEngine;
using Cinemachine;

public class StickToVirtualCamera : MonoBehaviour
{
    public CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float backgroundZOffset = 6f; // กำหนดค่า Z ที่ต้องการ

    private Transform backgroundTransform;
    private Transform cameraTransform;

    void Start()
    {
        backgroundTransform = transform;
        if (virtualCamera != null)
        {
            cameraTransform = virtualCamera.transform;
        }
        else
        {
            Debug.LogError("Virtual Camera is not assigned to StickToVirtualCamera on " + gameObject.name);
            enabled = false;
        }
    }

    void LateUpdate()
    {
        if (cameraTransform != null)
        {
            Vector3 cameraPosition = cameraTransform.position;
            backgroundTransform.position = new Vector3(cameraPosition.x, cameraPosition.y, backgroundZOffset);
        }
    }
}