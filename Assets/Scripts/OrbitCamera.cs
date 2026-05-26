using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionReference rotate;
    public InputActionReference zoom;
    public InputActionReference unlock;

    [Header("Camera Settings")]
    public Transform target;
    public float distance;

    [Header("Orbit Settings")]
    public float rotationSpeed;
    public float zoomStep;
    public float minDistance;

    // Internal state variables
    private Vector3 target_pos; // World Coordinates
    private Vector3 target_dir; // From Camera to Target
    private float yaw;
    private float pitch;


    void OnEnable()
    {
        ValidateInputActions();

        rotate.action.Enable();
        zoom.action.Enable();
        unlock.action.Enable();
    }

    void OnDisable()
    {
        rotate.action.Disable();
        zoom.action.Disable();
        unlock.action.Disable();
    }

    void Start()
    {
        ValidateTarget();
        InitializeOrbitState();
    }
    
    void ValidateTarget()
    {
        if (target == null)
            throw new InvalidOperationException("OrbitCamera requires a target Transform to be assigned.");
    }

    void ValidateInputActions()
    {
        if (rotate == null || rotate.action == null)
            throw new InvalidOperationException("OrbitCamera requires a rotate InputActionReference with a valid action.");

        if (!IsVector2Action(rotate.action))
            throw new InvalidOperationException("OrbitCamera requires rotate to be a Vector2 action.");

        if (zoom == null || zoom.action == null)
            throw new InvalidOperationException("OrbitCamera requires a zoom InputActionReference with a valid action.");

        if (!IsVector2Action(zoom.action))
            throw new InvalidOperationException("OrbitCamera requires zoom to be a Vector2 action.");

        if (unlock == null || unlock.action == null)
            throw new InvalidOperationException("OrbitCamera requires an unlock InputActionReference with a valid action.");

        if (!IsButtonAction(unlock.action))
            throw new InvalidOperationException("OrbitCamera requires unlock to be a Button action.");
    }

    bool IsVector2Action(InputAction action)
    {
        return action.type == InputActionType.Value && action.expectedControlType == "Vector2";
    }

    bool IsButtonAction(InputAction action)
    {
        return action.type == InputActionType.Button;
    }
    
    void InitializeOrbitState()
    {
        target_pos = target.position;
        target_dir = transform.position - target_pos;

        if (target_dir.sqrMagnitude < 0.0001f)
        {
            target_dir = -transform.forward;
        }

        distance = target_dir.magnitude;

        Vector3 direction = target_dir.normalized;
        yaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        pitch = Mathf.Asin(Mathf.Clamp(direction.y, -1f, 1f)) * Mathf.Rad2Deg;
    }

    void Update()
    {
        if (IsCameraUnlocked()) {
            Vector2 rotationInput = GetRotationInput();
            UpdateYaw(rotationInput.x, Time.deltaTime);
            UpdatePitch(rotationInput.y, Time.deltaTime);
        }
        
        UpdateZoom();
        ApplyOrbitTransform();
    }

    void UpdateZoom()
    {
        float zoomInput = zoom.action.ReadValue<Vector2>().y;
        distance -= zoomInput * zoomStep;
        distance = Mathf.Max(minDistance, distance);
    }

    void ApplyOrbitTransform()
    {
        target_pos = target.position;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        target_dir = rotation * Vector3.forward;
        transform.position = target_pos - target_dir * distance;
        transform.LookAt(target_pos);
    }

    Vector2 GetRotationInput()
    {
        return rotate.action.ReadValue<Vector2>();
    }

    void UpdateYaw(float x, float dt) {
        yaw += x * rotationSpeed * dt;
    }

    void UpdatePitch(float y, float dt) {
        pitch -= y * rotationSpeed * dt;
        pitch = Mathf.Clamp(pitch, -89f, 89f);
    }

    

    bool IsCameraUnlocked()
    {
        return unlock.action.IsPressed();
    }
}