using UnityEngine;

// Defines our available turning styles. We define this outside the
// class so it can be seen in the Inspector.
public enum TurnStyle
{
    Smooth,
    Snap
}
// --- END NEW ENUM ---

/// <summary>
///     This script manages player locomotion using a CharacterController.
///     It handles:
///     1. Gravity (to follow terrain downhill)
///     2. Thumbstick movement (which collides with terrain)
///     3. Jumping
///     4. Smooth Turning OR Snap Turning
///     5. Syncing the controller's horizontal position with the HMD
///     6. Rotation lock
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotion : MonoBehaviour
{
    [Header("Object References")] [Tooltip("The OVRCameraRig, which should be a child of this object.")]
    public OVRCameraRig ovrCameraRig;

    [Tooltip("The 'CenterEyeAnchor' Transform (HMD).")]
    public Transform centerEyeAnchor;

    [Header("Movement Settings")] public float moveSpeed = 3.0f;

    public float gravity = -9.81f;
    public float jumpForce = 5.0f;

    [Header("Rotation Settings")] [Tooltip("Forces the Player object to stay vertically upright.")]
    public bool forceUpright = true;

    // --- MODIFIED & NEW VARIABLES ---
    [Tooltip("Choose between Smooth (like a joystick) or Snap (teleporting rotation).")]
    public TurnStyle turnStyle = TurnStyle.Snap; // Default to Snap for comfort

    [Tooltip("The speed of smooth turning (if TurnStyle is Smooth).")]
    public float turnSpeed = 90.0f;

    [Tooltip("The fixed angle for each snap turn (if TurnStyle is Snap).")]
    public float snapTurnAngle = 45.0f;

    [Tooltip("The thumbstick threshold to trigger a snap turn (0 to 1).")] [Range(0.1f, 1.0f)]
    public float snapTurnThreshold = 0.9f;
    // --- END MODIFIED & NEW VARIABLES ---

    private CharacterController _controller;

    // Private variable to track our snap turn "cooldown"
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
        var totalMovement = CalculateMovement();
        _controller.Move(totalMovement * Time.deltaTime);
    }

    /// <summary>
    ///     Manages all player rig rotation: smooth/snap turning and upright enforcement.
    /// </summary>
    private void HandleRotation()
    {
        // --- MODIFIED: Now checks which turn style is active ---

        // Get the right thumbstick's horizontal input
        var turnInput = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick).x;

        if (turnStyle == TurnStyle.Smooth)
        {
            // --- SMOOTH TURN LOGIC (from before) ---
            if (Mathf.Abs(turnInput) > 0.1f)
            {
                var rotationAmount = turnInput * turnSpeed * Time.deltaTime;
                transform.Rotate(Vector3.up, rotationAmount);
            }
        }
        else if (turnStyle == TurnStyle.Snap)
        {
            // --- NEW SNAP TURN LOGIC ---

            // Check if we are NOT currently in the middle of a turn
            if (!_isSnapTurning)
            {
                // Flick right
                if (turnInput > snapTurnThreshold)
                {
                    transform.Rotate(Vector3.up, snapTurnAngle);
                    _isSnapTurning = true; // Set cooldown
                }
                // Flick left
                else if (turnInput < -snapTurnThreshold)
                {
                    transform.Rotate(Vector3.up, -snapTurnAngle);
                    _isSnapTurning = true; // Set cooldown
                }
            }

            // Check for stick release to reset the cooldown
            // We use a small deadzone (e.g., 0.1)
            if (Mathf.Abs(turnInput) < 0.1f) _isSnapTurning = false;
            // --- END NEW SNAP TURN LOGIC ---
        }

        // --- UPRIGHT LOCK (unchanged) ---
        if (forceUpright)
        {
            var angles = transform.rotation.eulerAngles;
            if (angles.x != 0 || angles.z != 0) transform.rotation = Quaternion.Euler(0, angles.y, 0);
        }
    }

    private void SyncControllerToHead()
    {
        var headLocalPos = ovrCameraRig.transform.InverseTransformPoint(centerEyeAnchor.position);
        _controller.center = new Vector3(headLocalPos.x, _controller.center.y, headLocalPos.z);
    }

    private Vector3 CalculateMovement()
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

        // --- Thumbstick Locomotion Logic (unchanged) ---
        var thumbstickInput = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
        var headForward = Vector3.ProjectOnPlane(centerEyeAnchor.forward, Vector3.up).normalized;
        var headRight = Vector3.ProjectOnPlane(centerEyeAnchor.right, Vector3.up).normalized;
        var moveDirection = headForward * thumbstickInput.y + headRight * thumbstickInput.x;
        totalMovement += moveDirection * moveSpeed;

        return totalMovement;
    }
}