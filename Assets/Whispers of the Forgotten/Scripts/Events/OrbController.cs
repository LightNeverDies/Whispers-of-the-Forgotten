using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbController : MonoBehaviour, IPowerReactive
{
    [Header("Orb Settings")]
    [Tooltip("The collider used for raycast detection (if null, will try to find one)")]
    public Collider orbCollider;
    
    [Tooltip("Layer mask for raycast detection")]
    public LayerMask detectionLayer = -1;
    
    [Tooltip("Maximum distance at which the orb can be detected by the player")]
    public float maxDetectionDistance = 20f;
    
    [Tooltip("If true, the orb must be directly in front of the camera to be detected")]
    public bool requireDirectView = true;
    
    [Header("Power & Visibility")]
    [Tooltip("If true, orb will start invisible/disabled")]
    public bool startInvisible = true;
    
    [Header("Animations")]
    [Tooltip("List of animations this orb can perform. Animation 1 (index 0) triggers when power goes off.")]
    public List<OrbAnimationData> animations = new List<OrbAnimationData>();
    
    [Header("Subtitle Integration")]
    [Tooltip("SubtitleManager to use for displaying subtitles (will auto-find if not assigned)")]
    public SubtitleManager subtitleManager;
    
    [Header("Proximity Settings")]
    [Tooltip("Distance at which player proximity will hide all objects (orb and shown objects)")]
    public float hideDistance = 3f;
    
    [Tooltip("Player GameObject reference (will auto-find if not assigned)")]
    public GameObject player;
    
    [Header("VHS/Glitch Effect")]
    [Tooltip("VHS Effect Controller to trigger when hiding objects (optional)")]
    public VHSEffectController vhsEffectController;
    
    [Tooltip("Duration of VHS effect when hiding objects")]
    public float vhsEffectDuration = 0.5f;
    
    [Header("Debug")]
    [Tooltip("If true, draw debug rays in the scene view")]
    public bool showDebugRays = false;

    [Header("Animation Safety (Anti-Teleport)")]
    [Tooltip("If true, detects large animation-driven teleports (e.g., clip resetting object to origin) and restores position. Keep this ON, but use a sensible threshold so real movement isn't cancelled.")]
    public bool preventAnimationTeleports = true;

    [Tooltip("Only treat position changes larger than this as a teleport. Too small values will cancel legitimate movement.")]
    public float teleportRestoreDistance = 2f;
    
    // Private variables
    private Camera mainCamera;
    private bool isPlayerLookingAtOrb = false;
    private bool isAnimating = false;
    private int currentAnimationIndex = -1;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    
    // Power state
    private bool hasPower = true;
    private bool isVisible = false;
    
    // State tracking
    private bool isStatic = false; // Is orb at static position waiting to be seen
    private Vector3 savedPosition; // Saved position when power goes on (only if static)
    private Quaternion savedRotation;
    private int savedAnimationIndex = -1; // Saved animation index when power goes on (only if static)

    // Power-interrupt resume (when power is turned ON mid-animation and later turned OFF again)
    private bool wasInterruptedByPower = false;
    private int interruptedAnimationIndex = -1;
    private int interruptedStateHash = 0;
    private float interruptedNormalizedTime = 0f;
    private bool interruptedWasInTransition = false;
    private int interruptedNextStateHash = 0;
    private float interruptedNextNormalizedTime = 0f;
    
    // Objects shown by animations
    private List<GameObject> currentlyShownObjects = new List<GameObject>();
    private bool lastAnimationCompleted = false; // Track if last animation has completed
    private bool hasTriggeredFromSight = false; // Prevent multiple triggers from sight detection
    private bool wasLookingWhenAnimationCompleted = false; // Track if player was looking when animation completed
    // If we cut power while at the final "objects shown" state, we should restore that state when power goes off again
    // instead of restarting from animation 0.
    private bool restoreFinalShownStateAfterPowerToggle = false;
    
    // Coroutine tracking
    private Coroutine currentAnimationCoroutine;
    
    void Start()
    {
        InitializeComponents();
        StoreOriginalTransform();
        
        // Auto-find VHS effect controller if not assigned
        if (vhsEffectController == null)
        {
            vhsEffectController = FindObjectOfType<VHSEffectController>();
        }
        
        // Start invisible if configured
        if (startInvisible)
        {
            SetOrbVisibility(false);
        }
    }
    
    void InitializeComponents()
    {
        // Find main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        // Auto-find collider if not assigned
        if (orbCollider == null)
        {
            orbCollider = GetComponent<Collider>();
            if (orbCollider == null)
            {
                orbCollider = GetComponentInChildren<Collider>();
            }
        }
        
        // Auto-find SubtitleManager if not assigned
        if (subtitleManager == null)
        {
            subtitleManager = FindObjectOfType<SubtitleManager>();
        }
        
        // Auto-find player if not assigned
        if (player == null)
        {
            PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
            if (playerMovement != null)
            {
                player = playerMovement.gameObject;
            }
        }
        
        // Auto-find Animator if not assigned in any animation data
        Animator defaultAnimator = GetComponent<Animator>();
        if (defaultAnimator == null)
        {
            defaultAnimator = GetComponentInChildren<Animator>();
        }
        
        foreach (var anim in animations)
        {
            if (anim.targetAnimator == null)
            {
                anim.targetAnimator = defaultAnimator;
            }
        }
    }
    
    void StoreOriginalTransform()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        savedPosition = originalPosition;
        savedRotation = originalRotation;
    }
    
    void Update()
    {
        // Only check for player detection if orb is visible and has power off
        if (!isVisible || hasPower)
            return;
        
        CheckForPlayerDetection();
        CheckPlayerProximity();
    }
    
    /// <summary>
    /// IPowerReactive interface implementation
    /// </summary>
    public void SetPower(bool state)
    {
        hasPower = state;
        
        if (hasPower)
        {
            // Power ON - hide orb and pause
            OnPowerOn();
        }
        else
        {
            // Power OFF - show orb and start animation 1
            OnPowerOff();
        }
    }
    
    void OnPowerOff()
    {
        // Show the orb
        SetOrbVisibility(true);

        // If power was turned ON mid-animation, resume from the exact animator time/state.
        if (wasInterruptedByPower &&
            interruptedAnimationIndex >= 0 &&
            interruptedAnimationIndex < animations.Count)
        {
            // Restore transform from when power was turned on
            transform.position = savedPosition;
            transform.rotation = savedRotation;

            currentAnimationIndex = interruptedAnimationIndex;
            isStatic = false;

            // Ensure no stale coroutines are running.
            if (currentAnimationCoroutine != null)
            {
                StopCoroutine(currentAnimationCoroutine);
                currentAnimationCoroutine = null;
            }
            StopAllCoroutines();

            // Restore animator playback position (best-effort).
            OrbAnimationData animData = animations[currentAnimationIndex];
            Animator animator = animData.targetAnimator;
            if (animator != null)
            {
                int stateHashToPlay = interruptedStateHash;
                float normalizedTimeToPlay = interruptedNormalizedTime;

                // If we were in transition when interrupted, prefer the next state.
                if (interruptedWasInTransition && interruptedNextStateHash != 0)
                {
                    stateHashToPlay = interruptedNextStateHash;
                    normalizedTimeToPlay = interruptedNextNormalizedTime;
                }

                // normalizedTime can exceed 1.0 for looping; clamp to [0,1) for Play(...)
                if (normalizedTimeToPlay >= 1f)
                {
                    normalizedTimeToPlay = normalizedTimeToPlay % 1f;
                }
                else if (normalizedTimeToPlay < 0f)
                {
                    normalizedTimeToPlay = 0f;
                }

                animator.Play(stateHashToPlay, 0, normalizedTimeToPlay);
                animator.Update(0f);
            }

            wasInterruptedByPower = false;
            interruptedAnimationIndex = -1;

            // Continue the in-progress animation without re-triggering it from the start.
            currentAnimationCoroutine = StartCoroutine(ResumeAnimation(animData));
            return;
        }

        // If power was toggled ON while we were in the final state (last animation completed),
        // restore that state now instead of restarting from the beginning.
        // This must run BEFORE the "savedAnimationIndex" restore, because the last animation can also be marked static.
        if (restoreFinalShownStateAfterPowerToggle && animations.Count > 0)
        {
            // Restore transform from when power was turned on
            transform.position = savedPosition;
            transform.rotation = savedRotation;

            currentAnimationIndex = animations.Count - 1;
            lastAnimationCompleted = true;
            isAnimating = false;
            isStatic = animations[currentAnimationIndex].isStaticPosition;

            // Re-show any objects that were hidden when power was turned on (if any).
            foreach (GameObject obj in currentlyShownObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }

            // Consume the restore flag; it will be set again on the next power-on toggle if still in final state.
            restoreFinalShownStateAfterPowerToggle = false;
            return;
        }

        // If we have a saved animation index (from a static "waiting" position), restore and wait for sight trigger.
        // This supports the design where the orb can pause at a static step when power toggles.
        if (savedAnimationIndex >= 0 && savedAnimationIndex < animations.Count)
        {
            transform.position = savedPosition;
            transform.rotation = savedRotation;

            currentAnimationIndex = savedAnimationIndex;
            isStatic = animations[currentAnimationIndex].isStaticPosition;

            // Don't auto-trigger; player must look at it to advance.
            return;
        }

        // Otherwise, always restart from the first animation when power goes off.
        // (Fixes: after a power ON/OFF cycle, currentAnimationIndex may be >= 0 but no coroutine is running,
        // leaving the orb visible but "stuck".)
        if (animations.Count > 0)
        {
            // Hard reset per-cycle flags so sight triggering works reliably.
            lastAnimationCompleted = false;
            hasTriggeredFromSight = false;
            wasLookingWhenAnimationCompleted = false;

            // IMPORTANT: Do NOT reset to originalPosition/originalRotation here.
            // Power toggles should not "teleport/reset" the orb/ball. Instead, restart the sequence
            // from the last known transform captured when power was turned ON.
            transform.position = savedPosition;
            transform.rotation = savedRotation;

            // Ensure no stale coroutines are running.
            if (currentAnimationCoroutine != null)
            {
                StopCoroutine(currentAnimationCoroutine);
                currentAnimationCoroutine = null;
            }
            StopAllCoroutines();
            isAnimating = false;
            isStatic = false;

            currentAnimationIndex = 0;
            currentAnimationCoroutine = StartCoroutine(PlayAnimation(animations[0]));
        }
    }
    
    void OnPowerOn()
    {
        // Save transform always so we can resume seamlessly.
        savedPosition = transform.position;
        savedRotation = transform.rotation;

        // Detect if we are currently in the final "objects shown" state.
        // If so, remember that we should restore it when power goes off again.
        restoreFinalShownStateAfterPowerToggle =
            animations.Count > 0 &&
            currentAnimationIndex == animations.Count - 1 &&
            lastAnimationCompleted;

        // If power is turned on during an animation, snapshot animator state/time so we can resume later.
        if (isAnimating && currentAnimationIndex >= 0 && currentAnimationIndex < animations.Count)
        {
            OrbAnimationData animData = animations[currentAnimationIndex];
            Animator animator = animData.targetAnimator;
            if (animator != null)
            {
                interruptedWasInTransition = animator.IsInTransition(0);

                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                interruptedStateHash = stateInfo.fullPathHash;
                interruptedNormalizedTime = stateInfo.normalizedTime;

                if (interruptedWasInTransition)
                {
                    AnimatorStateInfo nextInfo = animator.GetNextAnimatorStateInfo(0);
                    interruptedNextStateHash = nextInfo.fullPathHash;
                    interruptedNextNormalizedTime = nextInfo.normalizedTime;
                }
                else
                {
                    interruptedNextStateHash = 0;
                    interruptedNextNormalizedTime = 0f;
                }

                wasInterruptedByPower = true;
                interruptedAnimationIndex = currentAnimationIndex;
            }
        }

        // Save position and animation index only if orb is at static position
        if (isStatic)
        {
            savedAnimationIndex = currentAnimationIndex; // Save which animation we're on
        }
        else
        {
            // If not static, reset saved animation index
            savedAnimationIndex = -1;
        }
        
        // Hide the orb
        SetOrbVisibility(false);
        
        // Hide all shown objects
        foreach (GameObject obj in currentlyShownObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
        // Only clear the list if we are NOT preserving the final shown state.
        // When preserving, we keep references so we can re-enable them on power-off.
        if (!restoreFinalShownStateAfterPowerToggle)
        {
            currentlyShownObjects.Clear();
        }
        
        // Stop all animations
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }
        
        StopAllCoroutines();
        isAnimating = false;

        // IMPORTANT: Don't reset currentAnimationIndex/Animator here.
        // If power was turned on mid-animation, we want to resume from the same time/state when power goes off again.
    }

    IEnumerator ResumeAnimation(OrbAnimationData animData)
    {
        if (isAnimating)
            yield break;

        isAnimating = true;
        isStatic = false;

        // Reset flags (same semantics as PlayAnimation)
        lastAnimationCompleted = false;
        wasLookingWhenAnimationCompleted = false;

        Animator animator = animData.targetAnimator;
        bool isLastAnimation = (currentAnimationIndex == animations.Count - 1);

        // Wait for the current animator state to finish playing (best-effort).
        if (animator != null)
        {
            float animationTime = 0f;
            float maxWaitTime = isLastAnimation ? 60f : 30f;

            while (!hasPower && animationTime < maxWaitTime)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

                if (!animator.IsInTransition(0) && stateInfo.normalizedTime >= 1.0f)
                {
                    yield return new WaitForSeconds(isLastAnimation ? 0.5f : 0.1f);
                    break;
                }

                animationTime += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        // Update static state after animation completes
        isStatic = animData.isStaticPosition;

        // Check if player was looking when animation completed
        wasLookingWhenAnimationCompleted = isPlayerLookingAtOrb;

        if (wasLookingWhenAnimationCompleted && isStatic)
        {
            hasTriggeredFromSight = true;
        }

        if (isLastAnimation)
        {
            yield return new WaitForSeconds(0.3f);
            lastAnimationCompleted = true;

            if (animData.showObjectsOnTrigger && animData.objectsToShow != null)
            {
                ShowObjects(animData.objectsToShow);
            }

            yield return new WaitForSeconds(0.2f);
        }

        isAnimating = false;
    }
    
    void SetOrbVisibility(bool visible)
    {
        // Don't hide orb if we're animating (unless power is on)
        if (!visible && isAnimating && !hasPower)
        {
            return;
        }
        
        // Don't hide orb if it's the last animation and objects are shown (wait for player proximity)
        if (!visible && !hasPower)
        {
            bool isLastAnimation = (currentAnimationIndex == animations.Count - 1);
            if (isLastAnimation && currentlyShownObjects.Count > 0 && lastAnimationCompleted)
            {
                // Only hide if player is close enough - this will be handled by CheckPlayerProximity
                return;
            }
        }
        
        isVisible = visible;
        gameObject.SetActive(visible);
    }
    
    void CheckForPlayerDetection()
    {
        if (mainCamera == null || orbCollider == null)
            return;
        
        bool wasLookingAtOrb = isPlayerLookingAtOrb;
        isPlayerLookingAtOrb = false;
        
        // Perform raycast from camera center
        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit hit;
        
        float rayDistance = maxDetectionDistance;
        
        if (Physics.Raycast(ray, out hit, rayDistance, detectionLayer))
        {
            // Check if we hit this orb
            if (hit.collider == orbCollider)
            {
                // Check distance
                float distance = Vector3.Distance(mainCamera.transform.position, transform.position);
                if (distance <= maxDetectionDistance)
                {
                    // Check for obstructions if required
                    if (requireDirectView)
                    {
                        Vector3 directionToOrb = transform.position - mainCamera.transform.position;
                        RaycastHit obstructionHit;
                        
                        if (Physics.Raycast(mainCamera.transform.position, directionToOrb.normalized, out obstructionHit, distance, detectionLayer))
                        {
                            if (obstructionHit.collider != orbCollider)
                            {
                                // There's an obstruction
                                if (showDebugRays)
                                {
                                    Debug.DrawRay(mainCamera.transform.position, directionToOrb.normalized * distance, Color.red);
                                }
                                return;
                            }
                        }
                    }
                    
                    // Player is looking at orb
                    isPlayerLookingAtOrb = true;
                    
                    // Check if we should trigger next animation
                    if (!wasLookingAtOrb && isPlayerLookingAtOrb)
                    {
                        OnOrbSeen();
                    }
                    
                    if (showDebugRays)
                    {
                        Debug.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * hit.distance, Color.green);
                    }
                }
            }
        }
        else
        {
            // Player is not looking at orb
            if (!isPlayerLookingAtOrb)
            {
                // Reset flags when player stops looking
                if (wasLookingWhenAnimationCompleted)
                {
                    wasLookingWhenAnimationCompleted = false;
                }
                
                // Reset hasTriggeredFromSight when player stops looking (allows re-trigger when they look again)
                // But ONLY if animation is not currently playing AND orb is static
                // This prevents re-triggering while animation is in progress
                if (!isAnimating && isStatic && hasTriggeredFromSight)
                {
                    hasTriggeredFromSight = false;
                }
            }
            
            if (showDebugRays)
            {
                Debug.DrawRay(mainCamera.transform.position, mainCamera.transform.forward * rayDistance, Color.yellow);
            }
        }
    }
    
    void OnOrbSeen()
    {
        // Prevent multiple triggers - only trigger once per animation state
        if (hasTriggeredFromSight)
        {
            return;
        }
        
        // Don't trigger if we're currently animating - wait for animation to complete
        if (isAnimating)
        {
            return;
        }
        
        // Don't trigger if player was already looking when animation completed
        // Player needs to look away and look back again
        if (wasLookingWhenAnimationCompleted)
        {
            return;
        }
        
        // Check if current animation has triggerOnSight enabled
        if (currentAnimationIndex >= 0 && currentAnimationIndex < animations.Count)
        {
            OrbAnimationData currentAnim = animations[currentAnimationIndex];
            
            // Only trigger if:
            // 1. triggerOnSight is enabled
            // 2. Orb is at static position (waiting to be seen)
            // 3. Not currently animating
            // 4. Player wasn't already looking when animation completed
            if (currentAnim.triggerOnSight && isStatic && !isAnimating && !wasLookingWhenAnimationCompleted)
            {
                // Mark that we've triggered from sight to prevent multiple triggers
                hasTriggeredFromSight = true;
                
                // Trigger next animation
                int nextIndex = currentAnimationIndex + 1;
                if (nextIndex < animations.Count)
                {
                    currentAnimationIndex = nextIndex;
                    currentAnimationCoroutine = StartCoroutine(PlayAnimation(animations[nextIndex]));
                }
            }
        }
    }
    
    IEnumerator PlayAnimation(OrbAnimationData animData)
    {
        if (isAnimating)
            yield break;
        
        isAnimating = true;
        // While an animation is playing, the orb should NOT be considered "static / waiting to be seen".
        // If we keep isStatic=true from a previous step, a look-away/look-back can incorrectly advance
        // to the next animation if the animator state detection finishes early.
        isStatic = false;
        
        // Store current position before animation starts (to prevent teleportation issues)
        Vector3 positionBeforeAnimation = transform.position;
        Quaternion rotationBeforeAnimation = transform.rotation;
        
        // Play sound effect if configured
        if (animData.soundEffect != null)
        {
            animData.soundEffect.gameObject.SetActive(true);
            
            if (animData.useRandomPitch)
            {
                animData.soundEffect.pitch = Random.Range(0.9f, 1.1f);
            }
            
            animData.soundEffect.Play();
        }
        
        // Show subtitle if configured
        if (animData.showSubtitle && !string.IsNullOrEmpty(animData.subtitleText) && subtitleManager != null)
        {
            subtitleManager.ShowSubtitle(animData.subtitleText, animData.subtitleDuration);
        }
        
        // Reset flags
        lastAnimationCompleted = false;
        // Don't reset hasTriggeredFromSight here - it will be reset when animation completes and orb becomes static
        wasLookingWhenAnimationCompleted = false; // Reset flag for new animation
        
        // Trigger animation
        Animator animator = animData.targetAnimator;
        if (animator != null)
        {
            // Store position right before triggering to prevent teleportation
            Vector3 posBeforeTrigger = transform.position;
            Quaternion rotBeforeTrigger = transform.rotation;
            
            // Use trigger if specified
            if (!string.IsNullOrEmpty(animData.animationTrigger))
            {
                animator.SetTrigger(animData.animationTrigger);
            }
            // Otherwise use bool parameter
            else if (!string.IsNullOrEmpty(animData.animationBool))
            {
                animator.SetBool(animData.animationBool, animData.animationBoolValue);
            }
            
            // Immediate check after triggering (before first frame update)
            yield return null; // Wait one frame for animator to update
            if (preventAnimationTeleports)
            {
                float immediateDistance = Vector3.Distance(transform.position, posBeforeTrigger);
                if (immediateDistance > teleportRestoreDistance)
                {
                    transform.position = posBeforeTrigger;
                    transform.rotation = rotBeforeTrigger;
                }
            }
            
            // Wait for animation to start and monitor for position resets
            // Check for position resets for the first 1.5 seconds (increased from 0.5)
            // This prevents teleportation when animation clips start from different positions
            float positionCheckTime = 0f;
            float maxPositionCheckTime = 1.5f;
            Vector3 lastValidPosition = posBeforeTrigger;
            Quaternion lastValidRotation = rotBeforeTrigger;
            int consecutiveTeleportCount = 0;
            
            while (preventAnimationTeleports && positionCheckTime < maxPositionCheckTime && !hasPower)
            {
                yield return null;
                positionCheckTime += Time.deltaTime;
                
                // Check if position was reset (teleported) - if so, restore it
                float positionResetDistance = Vector3.Distance(transform.position, posBeforeTrigger);
                
                // Only treat large jumps as teleports. Small deltas are normal movement.
                if (positionResetDistance > teleportRestoreDistance)
                {
                    consecutiveTeleportCount++;
                    
                    // Only restore if we detect multiple consecutive teleports (to avoid interfering with smooth animation)
                    if (consecutiveTeleportCount >= 2)
                    {
                        transform.position = posBeforeTrigger;
                        transform.rotation = rotBeforeTrigger;
                        lastValidPosition = posBeforeTrigger;
                        lastValidRotation = rotBeforeTrigger;
                        consecutiveTeleportCount = 0; // Reset counter after restoration
                    }
                }
                else
                {
                    // Position is valid, update last valid position and reset counter
                    lastValidPosition = transform.position;
                    lastValidRotation = transform.rotation;
                    consecutiveTeleportCount = 0;
                }
            }
            
            // Final check: if position was teleported at the end of check period, restore it
            if (preventAnimationTeleports)
            {
                float finalDistance = Vector3.Distance(transform.position, posBeforeTrigger);
                if (finalDistance > teleportRestoreDistance)
                {
                    transform.position = posBeforeTrigger;
                    transform.rotation = rotBeforeTrigger;
                }
            }
        }
        
        // Check if this is the last animation
        bool isLastAnimation = (currentAnimationIndex == animations.Count - 1);
        
        // Wait for animation to complete
        if (animator != null && (!string.IsNullOrEmpty(animData.animationTrigger) || !string.IsNullOrEmpty(animData.animationBool)))
        {
            yield return new WaitForSeconds(0.1f);
            
            // State-based completion wait (prevents premature completion while the visual animation is still playing).
            // 1) Capture current state hash.
            // 2) Wait until the animator actually changes state (or enters a transition).
            // 3) Then wait until that target state finishes (normalizedTime >= 1) and we're not in transition.
            int initialStateHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;

            float startWait = 0f;
            const float maxStartWait = 1.5f;
            while (!hasPower && startWait < maxStartWait)
            {
                if (animator.IsInTransition(0) || animator.GetCurrentAnimatorStateInfo(0).fullPathHash != initialStateHash)
                {
                    break;
                }

                startWait += Time.deltaTime;
                yield return null;
            }
            
            int targetStateHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;

            float animationTime = 0f;
            float maxWaitTime = isLastAnimation ? 60f : 30f;
            while (!hasPower && animationTime < maxWaitTime)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

                // Only consider completion for the state we transitioned into.
                if (stateInfo.fullPathHash == targetStateHash &&
                    !animator.IsInTransition(0) &&
                    stateInfo.normalizedTime >= 1.0f)
                    {
                    yield return new WaitForSeconds(isLastAnimation ? 0.5f : 0.1f);
                    break;
                }
                
                animationTime += Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            // If no animator, wait a small amount to ensure other effects complete
            yield return new WaitForSeconds(0.5f);
        }
        
        // Ensure position is correct after animation (prevent teleportation)
        Vector3 positionAfterAnimation = transform.position;
        float positionChange = Vector3.Distance(positionBeforeAnimation, positionAfterAnimation);
        
        // Update static state after animation completes
        isStatic = animData.isStaticPosition;
        
        // Don't reset hasTriggeredFromSight here - it will be reset when player stops looking
        // This prevents re-triggering if player is still looking when animation completes
        
        // Check if player was looking when animation completed
        wasLookingWhenAnimationCompleted = isPlayerLookingAtOrb;
        
        // If player was looking when animation completed, keep hasTriggeredFromSight true
        // This prevents immediate trigger of next animation if player is still looking
        if (wasLookingWhenAnimationCompleted && isStatic)
        {
            hasTriggeredFromSight = true;
        }
        
        // Mark last animation as completed and show objects if this was the last one
        if (isLastAnimation)
        {
            // Additional wait for last animation to fully settle
            yield return new WaitForSeconds(0.3f);
            
            lastAnimationCompleted = true;
            
            // Show objects after last animation completes
            if (animData.showObjectsOnTrigger && animData.objectsToShow != null)
            {
                ShowObjects(animData.objectsToShow);
            }
            
            // Wait a bit more to ensure everything is stable before allowing proximity check
            yield return new WaitForSeconds(0.2f);
        }
        
        // Mark as not animating AFTER everything is done (for both last and non-last animations)
        isAnimating = false;
    }
    
    /// <summary>
    /// Manually trigger a specific animation by index
    /// </summary>
    public void TriggerAnimation(int animationIndex)
    {
        if (animationIndex >= 0 && animationIndex < animations.Count && !hasPower)
        {
            currentAnimationIndex = animationIndex;
            currentAnimationCoroutine = StartCoroutine(PlayAnimation(animations[animationIndex]));
        }
    }
    
    /// <summary>
    /// Reset the orb to its original state
    /// </summary>
    public void ResetOrb()
    {
        isAnimating = false;
        isStatic = false;
        currentAnimationIndex = -1;
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        savedPosition = originalPosition;
        savedRotation = originalRotation;
        StopAllCoroutines();
        currentAnimationCoroutine = null;
        
        // Reset animators if present
        foreach (var anim in animations)
        {
            if (anim.targetAnimator != null)
            {
                anim.targetAnimator.Rebind();
            }
        }
    }
    
    /// <summary>
    /// Check if the orb is currently static (waiting to be seen)
    /// </summary>
    public bool IsStatic()
    {
        return isStatic;
    }
    
    void CheckPlayerProximity()
    {
        if (player == null)
            return;
        
        // Only check proximity for last animation after it completed and objects are shown
        bool isLastAnimation = (currentAnimationIndex == animations.Count - 1);
        if (!isLastAnimation || !lastAnimationCompleted || currentlyShownObjects.Count == 0)
            return;
        
        // Don't hide if we're currently animating
        if (isAnimating)
            return;
        
        // Calculate distance from player to orb
        float distance = Vector3.Distance(player.transform.position, transform.position);
        
        // If player is within hide distance, hide all objects (orb and shown objects)
        if (distance <= hideDistance)
        {
            HideAllObjects();
        }
    }
    
    void ShowObjects(GameObject[] objects)
    {
        // Clear previous list
        currentlyShownObjects.Clear();
        
        // Show all objects
        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                currentlyShownObjects.Add(obj);
            }
        }
    }
    
    void HideAllObjects()
    {
        // Simple check: only hide if last animation completed and objects are shown
        bool isLastAnimation = (currentAnimationIndex == animations.Count - 1);
        if (!isLastAnimation || !lastAnimationCompleted || currentlyShownObjects.Count == 0)
            return;
        
        // Trigger VHS/Glitch effect before hiding
        if (vhsEffectController != null)
        {
            vhsEffectController.TriggerEffect(vhsEffectDuration);
        }
        
        // Hide orb
        isVisible = false;
        gameObject.SetActive(false);
        
        // Hide all shown objects
        foreach (GameObject obj in currentlyShownObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
        
        // Clear the list
        currentlyShownObjects.Clear();
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, maxDetectionDistance);
        
        // Draw hide distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, hideDistance);
    }
}

