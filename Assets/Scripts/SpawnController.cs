using UnityEngine;
using Unity.Cinemachine;

public class SpawnController : MonoBehaviour
{
    public Transform startPos;
    public GameObject carPrefab;

    // Reference to the Cinemachine camera
    public CinemachineCamera freeLookCamera;

    void Start()
    {
        // Create a rotation that aligns with the x-axis of startPos
        Quaternion rotation = Quaternion.LookRotation(startPos.right, Vector3.up);

        // Instantiate the car prefab at the startPos position with the calculated rotation
        GameObject carInstance = Instantiate(carPrefab, startPos.position, rotation);

        // Set the FreeLook Camera's Follow and LookAt targets to the instantiated car's transform
        freeLookCamera.Follow = carInstance.transform;
        freeLookCamera.LookAt = carInstance.transform;
    }
}
