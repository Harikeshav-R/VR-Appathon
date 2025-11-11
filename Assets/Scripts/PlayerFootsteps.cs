using UnityEngine;
using System.Collections.Generic; // We'll use a List for flexibility

/// <summary>
/// This script manages playing footstep sounds based on the player's movement.
/// It should be placed on the same GameObject as the CharacterController.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("Footstep Settings")] [Tooltip("The audio clips to play for footsteps. One will be chosen randomly.")]
    public List<AudioClip> footstepSounds;

    [Tooltip("The distance the player must travel to trigger one footstep sound.")]
    public float distancePerStep = 2.0f;

    [Tooltip("An optional volume multiplier for the footstep sounds.")] [Range(0.1f, 1.0f)]
    public float footstepVolume = 0.7f;

    // --- Private Variables ---
    private CharacterController _controller;
    private AudioSource _audioSource;

    // This tracks the distance we've "built up" since the last step
    private float _distanceCovered = 0.0f;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _audioSource = GetComponent<AudioSource>();

        if (footstepSounds.Count == 0)
        {
            Debug.LogWarning("PlayerFootsteps: No footstep sounds assigned.");
        }
    }

    private void Update()
    {
        // --- 1. Check if we are on the ground ---
        // We only want footsteps if the player is grounded.
        if (!_controller.isGrounded)
        {
            // Reset distance if we jump, so we don't get a "land" step
            // immediately followed by a "walk" step.
            _distanceCovered = 0.0f;
            return;
        }

        // --- 2. Get horizontal velocity ---
        // We only care about movement on the X and Z axes (walking),
        // not the Y axis (falling or jumping).
        var horizontalVelocity = new Vector3(_controller.velocity.x, 0, _controller.velocity.z);

        // --- 3. Check if we are actually moving ---
        // We use a small threshold to avoid "foot shuffling" sounds
        // when the controller velocity is near-zero.
        if (horizontalVelocity.magnitude > 0.1f)
        {
            // --- 4. Add to our distance tracker ---
            // We add the distance covered this frame (Velocity * Time).
            _distanceCovered += horizontalVelocity.magnitude * Time.deltaTime;

            // --- 5. Time to play a step? ---
            if (!(_distanceCovered >= distancePerStep)) return;
            PlayFootstepSound();

            // Subtract the step distance, but don't reset to 0.
            // This makes it more accurate if we "over-step" in one frame.
            _distanceCovered -= distancePerStep;
        }
        else
        {
            // Standing still, reset our distance tracker.
            _distanceCovered = 0.0f;
        }
    }

    /// <summary>
    /// Selects a random footstep sound and plays it.
    /// </summary>
    private void PlayFootstepSound()
    {
        if (footstepSounds.Count == 0)
        {
            return; // No sounds to play
        }

        // --- 1. Pick a random clip ---
        var clip = footstepSounds[Random.Range(0, footstepSounds.Count)];

        // --- 2. Play it ---
        // We use PlayOneShot, which is perfect for overlapping sound effects.
        // It plays the clip at the specified volume without stopping other sounds.
        _audioSource.PlayOneShot(clip, footstepVolume);
    }
}