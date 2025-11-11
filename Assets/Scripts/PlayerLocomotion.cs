using System;
using UnityEngine;

public enum TurnStyle
{
    Smooth,
    Snap
}

/// <summary>
///     This script manages player locomotion using a CharacterController.
///     It handles:
///     1. Gravity (to follow terrain downhill)
///     2. Thumbstick movement (which collides with terrain)
///     3. Jumping
///     4. Smooth Turning OR Snap Turning
///     5. Sprinting
///     6. Syncing the controller's horizontal position with the HMD
///     7. Rotation lock
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotion : MonoBehaviour
{
    [Header("Object References")] public OVRCameraRig ovrCameraRig;
    public Transform centerEyeAnchor;

    [Header("Movement Settings")] public float moveSpeed = 3.0f;

    // --- NEW VARIABLES ---
    [Tooltip("The speed the player moves at when sprinting.")]
    public float sprintSpeed = 6.0f;

    [Tooltip("How far forward the stick must be pushed (0 to 1) to allow sprinting.")] [Range(0.1f, 1.0f)]
    public float sprintInputThreshold = 0.8f;
    // --- END NEW VARIABLES ---

    public float gravity = -9.81f;
    public float jumpForce = 5.0f;

    [Header("Rotation Settings")] public bool forceUpright = true;
    public TurnStyle turnStyle = TurnStyle.Snap;
    public float turnSpeed = 90.0f;
    public float snapTurnAngle = 45.0f;
    [Range(0.1f, 1.0f)] public float snapTurnThreshold = 0.9f;
    private CharacterController _controller;

    private bool _isSnapTurning;
    private float _verticalVelocity;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        if (ovrCameraRig == null || centerEyeAnchor == null)
            Debug.LogError("PlayerLocomotion: OVRCameraRig or CenterEyeAnchor not assigned.");
    }

    private void Update()
    {
        HandleRotation();
        SyncControllerToHead();

        // --- MODIFIED CALL ---
        // Pass the thumbstick input to CalculateMovement
        var thumbstickInput = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
        var totalMovement = CalculateMovement(thumbstickInput);
        // --- END MODIFIED CALL ---

        _controller.Move(totalMovement * Time.deltaTime);
    }

    private void HandleRotation()
    {
        var turnInput = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).x;

        switch (turnStyle)
        {
            case TurnStyle.Smooth:
            {
                if (Mathf.Abs(turnInput) > 0.1f)
                {
                    var rotationAmount = turnInput * turnSpeed * Time.deltaTime;
                    transform.Rotate(Vector3.up, rotationAmount);
                }

                break;
            }
            case TurnStyle.Snap:
            {
                if (!_isSnapTurning)
                {
                    if (turnInput > snapTurnThreshold)
                    {
                        transform.Rotate(Vector3.up, snapTurnAngle);
                        _isSnapTurning = true;
                    }
                    else if (turnInput < -snapTurnThreshold)
                    {
                        transform.Rotate(Vector3.up, -snapTurnAngle);
                        _isSnapTurning = true;
                    }
                }

                if (Mathf.Abs(turnInput) < 0.1f) _isSnapTurning = false;

                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (!forceUpright) return;
        var angles = transform.rotation.eulerAngles;
        if (angles.x != 0 || angles.z != 0) transform.rotation = Quaternion.Euler(0, angles.y, 0);
    }

    private void SyncControllerToHead()
    {
        var headLocalPos = ovrCameraRig.transform.InverseTransformPoint(centerEyeAnchor.position);
        _controller.center = new Vector3(headLocalPos.x, _controller.center.y, headLocalPos.z);
    }

    // --- MODIFIED FUNCTION ---
    /// <summary>
    ///     Calculates the final movement vector for this frame,
    ///     including gravity, jumping, and locomotion (walk/sprint).
    /// </summary>
    /// <param name="thumbstickInput">The raw input from the LThumbstick.</param>
    /// <returns>The combined movement vector.</returns>
    private Vector3 CalculateMovement(Vector2 thumbstickInput)
    {
        var totalMovement = Vector3.zero;

        // --- Grounding, Gravity, & Jump Logic (unchanged) ---
        if (_controller.isGrounded)
        {
            if (OVRInput.GetDown(OVRInput.Button.One)) // 'A' button
                _verticalVelocity = jumpForce;
            else
                _verticalVelocity = -1.0f; // "Stick" to ground
        }
        else
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }

        totalMovement += Vector3.up * _verticalVelocity;

        // --- NEW SPRINT LOGIC ---

        // 1. Check for sprint button hold
        // OVRInput.Button.PrimaryThumbstick is the left stick click
        var isSprintButtonDown = OVRInput.Get(OVRInput.Button.PrimaryThumbstick);

        // 2. Check if we are pushing the stick forward past the threshold
        var isMovingForward = thumbstickInput.y > sprintInputThreshold;

        // 3. Determine if we are sprinting
        // We can only sprint if we are grounded, holding the button, AND
        // pushing the stick sufficiently forward.
        var isSprinting = _controller.isGrounded && isSprintButtonDown && isMovingForward;

        // 4. Select the speed
        var currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // --- END NEW SPRINT LOGIC ---


        // --- Thumbstick Locomotion Logic (modified to use currentSpeed) ---
        var headForward = Vector3.ProjectOnPlane(centerEyeAnchor.forward, Vector3.up).normalized;
        var headRight = Vector3.ProjectOnPlane(centerEyeAnchor.right, Vector3.up).normalized;
        var moveDirection = headForward * thumbstickInput.y + headRight * thumbstickInput.x;

        // Apply the correct speed (walk or sprint)
        totalMovement += moveDirection * currentSpeed;

        return totalMovement;
    }
}