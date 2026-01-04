using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 2.0f;
    public float gravity = -9.81f;
    [Tooltip("Caps falling speed to avoid tunneling through thin colliders when squeezed between objects.")]
    public float maxFallSpeed = 25f;

    public Transform groundCheck;
    [Tooltip("Ground probe radius. Will be clamped to the CharacterController radius at runtime.")]
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    [Tooltip("Minimum Y component of the ground normal required to be considered walkable (e.g. 0.6 ~ 53 degrees).")]
    [Range(0f, 1f)]
    public float minGroundNormalY = 0.6f;

    [Header("Ground Snap")]
    [Tooltip("If enabled, snaps the CharacterController down to nearby ground to prevent small gaps causing falls.")]
    public bool enableGroundSnap = true;
    [Tooltip("Maximum distance to snap down to ground when falling/stepping off tiny ledges.")]
    public float groundSnapDistance = 0.6f;

    [Header("Fall Recovery (Failsafe)")]
    [Tooltip("If enabled, restores the player to the last safe grounded position when falling below Fall Kill Y.")]
    public bool enableFallRecovery = true;
    [Tooltip("If the player's Y position goes below this value, they will be recovered to the last safe position.")]
    public float fallKillY = -25f;
    [Tooltip("Extra height added when recovering to avoid spawning inside the floor.")]
    public float recoveryUpOffset = 0.2f;
    [Tooltip("Optional: a dedicated respawn point. Used if last safe position is invalid.")]
    public Transform respawnPoint;

    private CharacterController controller;
    private float verticalVelocity;
    private bool isGrounded;
    private Vector3 lastSafePosition;
    private Vector3 initialPosition;

    public bool inputEnabled = true;

    // Footstep sound variables
    public AudioSource audioSource;              // Assign your AudioSource component here in Inspector
    public AudioClip leftFootstepSound;          // Assign your left footstep AudioClip in Inspector
    public AudioClip rightFootstepSound;         // Assign your right footstep AudioClip in Inspector

    private float footstepTimer = 0f;
    public float footstepInterval = 0.5f;        // Time between footsteps, adjust to fit walking speed
    private bool isLeftFoot = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        initialPosition = transform.position;
        lastSafePosition = initialPosition;

        if (audioSource == null)
        {
            // Try to get AudioSource from same GameObject if not assigned
            audioSource = GetComponent<AudioSource>();
        }

        Camera.main.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    void Update()
    {
        // Ensure we can always recover even if input is disabled during cutscenes/events.
        if (enableFallRecovery && transform.position.y < fallKillY)
        {
            RecoverFromFall();
            return;
        }

        if (!inputEnabled) { return; }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Grounding: prefer CharacterController's own collision result (most reliable for CC),
        // plus a downward sphere cast (avoids treating nearby walls as "ground").
        bool castGrounded = false;
        RaycastHit groundHit = default;
        if (groundCheck != null)
        {
            Vector3 origin = groundCheck.position + Vector3.up * 0.05f;
            float maxDistance = 0.15f + controller.skinWidth;
            float probeRadius = Mathf.Min(groundDistance, controller.radius * 0.95f);
            if (Physics.SphereCast(
                origin,
                probeRadius,
                Vector3.down,
                out groundHit,
                maxDistance,
                groundMask,
                QueryTriggerInteraction.Ignore))
            {
                castGrounded = IsWalkableHit(groundHit);
            }
        }
        isGrounded = controller.isGrounded || castGrounded;

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f; // small downward force to keep grounded
        }

        // Horizontal movement (units: m/s)
        Vector3 horizontal = (transform.right * x + transform.forward * z) * speed;

        // Gravity (units: m/s)
        verticalVelocity += gravity * Time.deltaTime;
        if (maxFallSpeed > 0f)
        {
            verticalVelocity = Mathf.Max(verticalVelocity, -maxFallSpeed);
        }

        // Apply movement once per frame (units: meters)
        Vector3 motion = (horizontal + Vector3.up * verticalVelocity) * Time.deltaTime;
        CollisionFlags flags = controller.Move(motion);

        // If we hit the ground or ceiling, clamp vertical velocity to avoid building large values.
        if ((flags & CollisionFlags.Below) != 0)
        {
            isGrounded = true;
            if (verticalVelocity < 0f) verticalVelocity = -2f;
        }
        if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
        {
            verticalVelocity = 0f;
        }

        // Extra safety: snap to ground if we're very close but CC didn't report grounded (common when pushing between colliders).
        if (enableGroundSnap && !isGrounded && verticalVelocity <= 0f)
        {
            TrySnapToGround();
        }

        // Track last safe position while grounded.
        if (isGrounded)
        {
            // Only accept as "safe" if we're grounded on a walkable surface (prevents storing a "safe" spot on a wall).
            if (castGrounded)
            {
                lastSafePosition = transform.position;
            }
            else
            {
                // If we didn't cast this frame, validate with a short ray down from the controller center.
                Vector3 centerWorld = transform.TransformPoint(controller.center);
                if (Physics.Raycast(
                    centerWorld,
                    Vector3.down,
                    out RaycastHit hit,
                    controller.height * 0.6f,
                    groundMask,
                    QueryTriggerInteraction.Ignore) && IsWalkableHit(hit))
                {
                    lastSafePosition = transform.position;
                }
            }
        }

        // FOOTSTEP SOUND LOGIC:
        // Play footsteps only if grounded and player is moving (input magnitude check)
        if (isGrounded && (Mathf.Abs(x) > 0.1f || Mathf.Abs(z) > 0.1f))
        {
            footstepTimer += Time.deltaTime;

            if (footstepTimer >= footstepInterval)
            {
                // Play left or right footstep sound alternately
                audioSource.PlayOneShot(isLeftFoot ? leftFootstepSound : rightFootstepSound);

                isLeftFoot = !isLeftFoot;
                footstepTimer = 0f;
            }
        }
        else
        {
            // Reset timer if not moving or not grounded
            footstepTimer = footstepInterval;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }

    private void TrySnapToGround()
    {
        // Bottom sphere center of the CharacterController capsule in world space.
        Vector3 centerWorld = transform.TransformPoint(controller.center);
        Vector3 bottomSphereCenter = centerWorld + Vector3.down * (controller.height * 0.5f - controller.radius);
        float castRadius = controller.radius * 0.95f;

        if (Physics.SphereCast(
            bottomSphereCenter,
            castRadius,
            Vector3.down,
            out RaycastHit hit,
            groundSnapDistance,
            groundMask,
            QueryTriggerInteraction.Ignore))
        {
            if (!IsWalkableHit(hit)) return;

            float moveDown = hit.distance - controller.skinWidth;
            if (moveDown > 0f)
            {
                controller.Move(Vector3.down * moveDown);
            }

            isGrounded = true;
            if (verticalVelocity < 0f) verticalVelocity = -2f;
        }
    }

    private void RecoverFromFall()
    {
        // Teleport safely with CharacterController.
        if (controller == null) controller = GetComponent<CharacterController>();

        Vector3 target = lastSafePosition;
        // If lastSafePosition is invalid (e.g., never set correctly), fallback to respawn/start.
        if (target.y < fallKillY || float.IsNaN(target.y) || float.IsInfinity(target.y))
        {
            target = respawnPoint != null ? respawnPoint.position : initialPosition;
        }

        bool wasEnabled = controller != null && controller.enabled;
        if (controller != null) controller.enabled = false;
        transform.position = target + Vector3.up * Mathf.Max(0f, recoveryUpOffset);
        if (controller != null) controller.enabled = wasEnabled;
        verticalVelocity = 0f;
        isGrounded = false;
    }

    private bool IsWalkableHit(RaycastHit hit)
    {
        return hit.collider != null && hit.normal.y >= minGroundNormalY;
    }
}
