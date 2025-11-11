using UnityEngine;

/// <summary>
///     This script manages player locomotion using a CharacterController.
///     It handles:
///     1. Gravity (to follow terrain downhill)
///     2. Thumbstick movement (which collides with terrain)
///     3. Jumping
///     4. Syncing the controller's horizontal position with the HMD
///     5. Rotation lock
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotion : MonoBehaviour
{
    [Header("Object References")] [Tooltip("The OVRCameraRig, which should be a child of this object.")]
    public OVRCameraRig ovrCameraRig;

    [Tooltip("The 'CenterEyeAnchor' Transform (HMD).")]
    public Transform centerEyeAnchor;

    [Header("Movement Settings")] [Tooltip("Movement speed in meters per second.")]
    public float moveSpeed = 3.0f;

    [Tooltip("The force of gravity to apply.")]
    public float gravity = -9.81f;

    // --- NEW VARIABLE ---
    [Tooltip("The initial upward velocity of the jump (in m/s).")]
    public float jumpForce = 5.0f;
    // --- END NEW VARIABLE ---

    [Header("Rotation Settings")] [Tooltip("Forces the Player object to stay vertically upright.")]
    public bool forceUpright = true;

    private CharacterController _controller;
    private float _verticalVelocity; // Stores our current gravity/jump speed

    private void Start()
    {
        _controller = GetComponent<CharacterController>();

        if (ovrCameraRig == null || centerEyeAnchor == null)
            Debug.LogError("PlayerLocomotion: OVRCameraRig or CenterEyeAnchor not assigned.");
    }

    private void Update()
    {
        // --- 1. Rotation ---
        if (forceUpright)
        {
            var angles = transform.rotation.eulerAngles;
            if (angles.x != 0 || angles.z != 0) transform.rotation = Quaternion.Euler(0, angles.y, 0);
        }

        // --- 2. Grounding, Gravity, & Jump ---
        SyncControllerToHead();

        var totalMovement = Vector3.zero;

        // Check if the controller is on the ground
        if (_controller.isGrounded)
        {
            // --- NEW JUMP LOGIC ---
            // Check for jump input (OVRInput.Button.One is 'A' on the right controller)
            if (OVRInput.GetDown(OVRInput.Button.One))
                // Apply the jump force as an immediate upward velocity
                _verticalVelocity = jumpForce;
            else
                // Not jumping, so "stick" to the ground
                _verticalVelocity = -1.0f;
            // --- END NEW JUMP LOGIC ---
        }
        else
        {
            // We are in the air, so apply gravity
            _verticalVelocity += gravity * Time.deltaTime;
        }

        // Add vertical (gravity/jump) movement to our total
        totalMovement += Vector3.up * _verticalVelocity;

        // --- 3. Thumbstick Locomotion ---
        var thumbstickInput = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
        var headForward = Vector3.ProjectOnPlane(centerEyeAnchor.forward, Vector3.up).normalized;
        var headRight = Vector3.ProjectOnPlane(centerEyeAnchor.right, Vector3.up).normalized;
        var moveDirection = headForward * thumbstickInput.y + headRight * thumbstickInput.x;
        totalMovement += moveDirection * moveSpeed;

        // --- 4. Apply All Movement ---
        // controller.Move() handles all collisions, gravity, and jumping
        _controller.Move(totalMovement * Time.deltaTime);
    }

    /// <summary>
    ///     Syncs the CharacterController's center to the HMD's X/Z position.
    /// </summary>
    private void SyncControllerToHead()
    {
        var headLocalPos = ovrCameraRig.transform.InverseTransformPoint(centerEyeAnchor.position);
        _controller.center = new Vector3(headLocalPos.x, _controller.center.y, headLocalPos.z);
    }
}