using UnityEngine;
using UnityEngine.InputSystem;

public class FollowCamera : MonoBehaviour
{
    public Transform target;                   // Reference to the car
    public Vector3 offset = new Vector3(0, 2, -8); // Offset behind the car
    public float followSpeed = 10f;            // Speed of the camera movement
    public float rotationSpeed = 5f;           // Speed of the camera rotation
    public float mouseSensitivity = 100f;      // Sensitivity for mouse input
    public float gamepadSensitivity = 100f;    // Sensitivity for gamepad input

    public InputAction lookAction;             // Input action for looking around

    private Transform camFocus;                // Reference to the CamFocus object
    private float yaw = 0f;                    // Yaw rotation angle
    private bool isStickInUse = false;         // Flag to track if the stick is being used
    private bool isMouseInUse = false;         // Flag to track if the mouse is being used

    private void Awake()
    {
        // Initialize the look action
        lookAction = new InputAction(type: InputActionType.Value, binding: "<Gamepad>/rightStick");
    }

    private void OnEnable()
    {
        // Enable the look action
        lookAction.Enable();
    }

    private void OnDisable()
    {
        // Disable the look action
        lookAction.Disable();
    }

    private void Start()
    {
        // Find the CamFocus object by tag
        GameObject camFocusObject = GameObject.FindWithTag("CameraFocusPoint");
        if (camFocusObject != null)
        {
            camFocus = camFocusObject.transform;
        }
        else
        {
            Debug.LogError("CamFocus object not found with tag 'CameraFocusPoint'.");
        }

        // Set initial yaw to align with the car's forward direction
        if (target != null)
        {
            yaw = target.eulerAngles.y;
        }
    }

    void FixedUpdate()
    {
        if (target == null || camFocus == null) return;

        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.fixedDeltaTime;

        // Get gamepad input from the look action
        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        float gamepadX = lookInput.x * gamepadSensitivity * Time.fixedDeltaTime;

        // Check if the stick or mouse is being used
        isStickInUse = lookInput != Vector2.zero;
        isMouseInUse = Mathf.Abs(mouseX) > 0.01f;

        if (isStickInUse)
        {
            // Combine mouse and gamepad input for horizontal rotation
            yaw += gamepadX;
        }
        else if (isMouseInUse)
        {
            // Apply mouse input to yaw
            yaw += mouseX;
        }
        else
        {
            // Smoothly reset yaw to align with the car's forward direction
            float targetYaw = target.eulerAngles.y;
            yaw = Mathf.LerpAngle(yaw, targetYaw, rotationSpeed * Time.fixedDeltaTime);
        }

        // Calculate rotation based on input (only yaw)
        Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);

        // Target position with the offset
        Vector3 desiredPosition = camFocus.position + rotation * offset;

        // Smoothly move camera position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.fixedDeltaTime);

        // Smoothly rotate the camera to look at the CamFocus
        Quaternion targetRotation = Quaternion.LookRotation(camFocus.position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }
}




