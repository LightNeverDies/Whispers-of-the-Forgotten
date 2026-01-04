using UnityEngine;

public class MirrorHandprintRevealer : MonoBehaviour
{
    [Header("Trigger Settings (Look at Mirror)")]
    [Tooltip("Usually the player's camera transform.")]
    [SerializeField] private Transform playerCamera;

    [Tooltip("The mirror transform that the player must look at.")]
    [SerializeField] private Transform mirrorTransform;

    [SerializeField] private float triggerDistance = 3f;

    [Tooltip("Max angle (degrees) between camera forward and direction to mirror.")]
    [SerializeField] private float triggerAngle = 25f;

    [Tooltip("If enabled, raycast must hit the mirror (prevents triggering through walls).")]
    [SerializeField] private bool requireLineOfSight = true;

    [Header("Animation")]
    [Tooltip("Animator that plays the mirror reveal animation. Can be on the same GameObject or elsewhere.")]
    [SerializeField] private Animator animator;

    [Tooltip("Animator trigger parameter name to fire when reveal should happen.")]
    [SerializeField] private string revealTrigger = "Reveal";

    [Tooltip("If true, will only trigger once until ResetSequence() is called.")]
    [SerializeField] private bool playOnce = true;

    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource revealAudioSource;

    [Tooltip("If true, enable the AudioSource GameObject before playing (handy if it's disabled by default).")]
    [SerializeField] private bool enableAudioGameObject = true;

    [SerializeField] private Vector2 randomPitchRange = new Vector2(0.9f, 1.1f);

    private bool hasTriggered = false;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        if (playOnce && hasTriggered) return;
        if (playerCamera == null || mirrorTransform == null) return;

        // Check if player is near and looking at mirror
        float distance = Vector3.Distance(playerCamera.position, mirrorTransform.position);
        Vector3 directionToMirror = (mirrorTransform.position - playerCamera.position).normalized;
        float angle = Vector3.Angle(playerCamera.forward, directionToMirror);

        if (distance <= triggerDistance && angle < triggerAngle)
        {
            if (!requireLineOfSight)
            {
                TriggerReveal();
                return;
            }

            // Check if there's a clear line of sight to the mirror (no walls blocking)
            RaycastHit hit;
            float rayDistance = Vector3.Distance(playerCamera.position, mirrorTransform.position);

            if (!Physics.Raycast(playerCamera.position, directionToMirror, out hit, rayDistance))
            {
                // If no hit, assume it's clear
                TriggerReveal();
                return;
            }

            // If the raycast hits the mirror, it's clear
            if (hit.collider != null && hit.collider.transform == mirrorTransform)
            {
                TriggerReveal();
            }
        }
    }

    /// <summary>
    /// Triggers the reveal animation and optional audio.
    /// Call this from Unity Events / Timeline if you want manual control.
    /// </summary>
    public void TriggerReveal()
    {
        if (playOnce) hasTriggered = true;

        if (animator != null && !string.IsNullOrWhiteSpace(revealTrigger))
        {
            animator.SetTrigger(revealTrigger);
        }

        if (revealAudioSource != null)
        {
            if (enableAudioGameObject && !revealAudioSource.gameObject.activeSelf)
            {
                revealAudioSource.gameObject.SetActive(true);
            }

            revealAudioSource.pitch = Random.Range(randomPitchRange.x, randomPitchRange.y);
            revealAudioSource.Play();
        }
    }

    public void ResetSequence()
    {
        hasTriggered = false;

        // Optional: reset animator to its default state.
        // (Useful while testing; safe to leave as-is if you handle resets in Animator/Timeline.)
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }

}

