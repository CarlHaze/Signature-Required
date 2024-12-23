using UnityEngine.InputSystem;
using UnityEngine;
using System.Linq;

public class CarControl : MonoBehaviour
{
    public float motorTorque = 2500;
    public float brakeTorque = 2700;
    public float maxSpeed = 20;
    public float steeringRange = 30;
    public float steeringRangeAtMaxSpeed = 10;
    public Vector3 centerOfMass;
    public float currentSpeed = 0f;

    private WheelControl[] wheels;
    private Rigidbody rigidBody;

    // Input actions for acceleration and steering
    private InputAction accelerateAction;
    private InputAction brakeAction;
    private InputAction steerAction;
    private InputAction reverseAction;

    public Light LeftBrakeLight;
    public Light RightBrakeLight;
    public Light LeftReverseLight;
    public Light RightReverseLight;

    // UI
    private UIManager uiManager;

    private void Awake()
    {
        // Bind input actions from Input System
        accelerateAction = InputSystem.actions.FindAction("Accelerate");
        brakeAction = InputSystem.actions.FindAction("Brake");
        steerAction = InputSystem.actions.FindAction("Move");
        reverseAction = InputSystem.actions.FindAction("Reverse");
    }

    private void OnEnable()
    {
        accelerateAction.Enable();
        brakeAction.Enable();
        steerAction.Enable();
        reverseAction.Enable();
    }

    private void OnDisable()
    {
        accelerateAction.Disable();
        brakeAction.Disable();
        steerAction.Disable();
        reverseAction.Disable();
    }

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.centerOfMass = centerOfMass;
        wheels = GetComponentsInChildren<WheelControl>();
        LeftBrakeLight.enabled = false;
        RightBrakeLight.enabled = false;
        LeftReverseLight.enabled = false;
        RightReverseLight.enabled = false;

        // Find the UIManager in the scene
        uiManager = FindFirstObjectByType<UIManager>();
    }

    public float GetCurrentSpeed()
    {
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
        return Mathf.Round(Mathf.Abs(forwardSpeed * 3.6f) * 10f) / 10f; // Convert to km/h and round to 1 decimal place
    }

    void FixedUpdate()
    {
        // Separate input handling for acceleration/braking and steering
        float vInput = accelerateAction.ReadValue<float>() - brakeAction.ReadValue<float>();
        float hInput = steerAction.ReadValue<Vector2>().x;
        bool isReversing = reverseAction.ReadValue<float>() > 0;

        // Keyboard input fallback
        if (vInput == 0 && !isReversing)
        {
            vInput = Input.GetKey(KeyCode.W) ? 1 : (Input.GetKey(KeyCode.S) ? -1 : 0);
        }

        if (hInput == 0)
        {
            hInput = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        }

        // Speed calculation and adjustment based on max speed
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, forwardSpeed);

        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

        bool isBraking = brakeAction.ReadValue<float>() > 0 || Input.GetKey(KeyCode.S);
        LeftBrakeLight.enabled = RightBrakeLight.enabled = isBraking;

        // Apply torque and steering
        foreach (var wheel in wheels)
        {
            if (wheel.steerable)
            {
                wheel.WheelCollider.steerAngle = hInput * currentSteerRange;
            }

            if (vInput > 0)
            {
                if (wheel.motorized)
                {
                    wheel.WheelCollider.motorTorque = vInput * currentMotorTorque;
                }
                wheel.WheelCollider.brakeTorque = 0;
            }
            else if (isReversing)
            {
                // Allow reversing without holding the brake
                if (wheel.motorized)
                {
                    wheel.WheelCollider.motorTorque = -motorTorque;
                }
                wheel.WheelCollider.brakeTorque = 0;
            }
            else if (isBraking)
            {
                // Apply brake torque immediately when braking
                wheel.WheelCollider.brakeTorque = brakeTorque;
                wheel.WheelCollider.motorTorque = 0;
            }
            else
            {
                wheel.WheelCollider.brakeTorque = 0;
                wheel.WheelCollider.motorTorque = 0;
            }
        }

        // Update current speed
        currentSpeed = GetCurrentSpeed();

        // Enable reverse lights when reversing
        LeftReverseLight.enabled = RightReverseLight.enabled = isReversing;
    }

}
