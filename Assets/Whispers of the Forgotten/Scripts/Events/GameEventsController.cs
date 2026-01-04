using System.Collections;
using UnityEngine;

public class GameEventsController : MonoBehaviour
{
    [Header("Common Settings")]
    public GameObject triggerObject;
    public Camera mainCamera;
    [Tooltip("Layer mask that will trigger the event when hit by raycast")]
    public LayerMask interactionLayer;
    [Tooltip("Layer mask for objects that can be moved/thrown (find objects automatically)")]
    public LayerMask movableObjectLayer;
    [Tooltip("Layer mask for obstructions (walls, etc.) that block raycast. Leave as 'Nothing' to use everything except interactionLayer.")]
    public LayerMask obstructionLayer;
    public float interactDistance = 5f;
    
    public enum TriggerMode 
    { 
        LookBased,      // Trigger when looking at object
        DistanceBased   // Trigger when player gets within distance
    }
    [Header("Trigger Mode Settings")]
    [Tooltip("LookBased: Trigger when looking at object.\nDistanceBased: Trigger when player is within range of specified object/position.")]
    public TriggerMode triggerMode = TriggerMode.LookBased;
    
    [Header("Distance-Based Trigger Settings")]
    [Tooltip("Distance in units to trigger event (only used if TriggerMode is DistanceBased)")]
    public float triggerDistance = 3f;
    
    public enum DistanceReferenceType
    {
        TriggerObject,      // Use triggerObject position
        EventObject,         // Use event-specific object (fallObject, throwObject, etc.)
        CustomPosition,      // Use custom world position
        CustomObject         // Use custom GameObject position
    }
    [Tooltip("What position to measure distance from")]
    public DistanceReferenceType distanceReferenceType = DistanceReferenceType.TriggerObject;
    
    [Tooltip("Custom position to check distance from (only if distanceReferenceType is CustomPosition)")]
    public Vector3 customTriggerPosition;
    
    [Tooltip("Custom GameObject to check distance from (only if distanceReferenceType is CustomObject)")]
    public GameObject customReferenceObject;
    
    [Tooltip("If true, checks distance only on XZ plane (ignores Y/height). Useful for floor-based triggers.")]
    public bool use2DDistance = false;
    
    [Header("Performance Settings")]
    [Tooltip("How often to check for triggers (in seconds). Lower values = more responsive but more CPU usage. Recommended: 0.1-0.2")]
    [Range(0.01f, 0.5f)]
    public float checkInterval = 0.1f;
    
    private bool eventTriggered = false;
    private float lastCheckTime = 0f;
    private float triggerDistanceSquared; // Cached squared distance for faster comparison

    [Header("Fall Event Settings")]
    [Tooltip("Leave empty to use triggerObject")]
    public GameObject fallObject;
    public AudioSource fallObjectSoundEffect;
    public Vector3 fallTargetPosition;
    public Quaternion fallTargetRotation;
    public float fallDuration = 1.0f;

    [Header("Throw Event Settings")]
    [Tooltip("Leave empty to use triggerObject")]
    public GameObject throwObject;
    public Vector3 throwTargetPosition;
    public float throwDuration = 1.0f;
    public float throwHeight = 1.0f;
    public AudioSource throwSoundEffect;

    [Header("Move Event Settings")]
    [Tooltip("Leave empty to use triggerObject")]
    public GameObject moveObject;
    [Tooltip("If true, moveTargetPosition is an offset from current position. If false, it's an absolute world position.")]
    public bool useRelativePosition = false;
    [Tooltip("Target position - either absolute (if useRelativePosition=false) or offset from start (if useRelativePosition=true)")]
    public Vector3 moveTargetPosition;
    [Tooltip("Target rotation as Euler angles (X, Y, Z) - will preserve exact angle values")]
    public Vector3 moveTargetRotationEuler;
    [Tooltip("If true, uses moveTargetRotationEuler. If false, uses legacy moveTargetRotation Quaternion")]
    public bool useEulerAngles = true;
    [Tooltip("Legacy Quaternion rotation (use moveTargetRotationEuler instead for better control)")]
    public Quaternion moveTargetRotation;
    public float moveDuration = 2.0f;
    public AudioSource moveSoundEffect;

    [Header("Rocking Chair Event Settings")]
    [Tooltip("Leave empty to use triggerObject")]
    public GameObject rockingChairObject;
    [Tooltip("Axis to rotate around (typically Z for forward/back rocking or X for side-to-side)")]
    public Vector3 rockingAxis = Vector3.forward;
    [Tooltip("Maximum angle in degrees the chair rocks forward and backward")]
    public float rockingAngle = 15f;
    [Tooltip("Duration for one complete rock cycle (forward and back)")]
    public float rockingCycleDuration = 2.0f;
    [Tooltip("Number of times to rock back and forth")]
    public int rockingCycles = 3;
    [Tooltip("Easing curve for smooth rocking motion (0 = linear, higher = smoother)")]
    public AnimationCurve rockingCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AudioSource rockingChairSoundEffect;

    [Header("Wishiper Event")]
    public AudioSource ghostBreath;

    [Header("Animation Settings")]
    [Tooltip("Enable to trigger animations when events occur")]
    public bool enableAnimations = false;
    [Tooltip("If true, will automatically find Animator component on the event object if not assigned")]
    public bool autoFindAnimator = true;

    [Header("Fall Event Animation")]
    public Animator fallObjectAnimator;
    [Tooltip("Animation trigger name to set (leave empty if using bool parameter)")]
    public string fallAnimationTrigger = "";
    [Tooltip("Animation bool parameter name and value (only used if trigger is empty)")]
    public string fallAnimationBool = "";
    public bool fallAnimationBoolValue = true;

    [Header("Throw Event Animation")]
    public Animator throwObjectAnimator;
    public string throwAnimationTrigger = "";
    public string throwAnimationBool = "";
    public bool throwAnimationBoolValue = true;

    [Header("Move Event Animation")]
    public Animator moveObjectAnimator;
    public string moveAnimationTrigger = "";
    public string moveAnimationBool = "";
    public bool moveAnimationBoolValue = true;

    [Header("Rocking Chair Event Animation")]
    public Animator rockingChairAnimator;
    public string rockingChairAnimationTrigger = "";
    public string rockingChairAnimationBool = "";
    public bool rockingChairAnimationBoolValue = true;

    [Header("Whisper Event Animation")]
    public Animator whisperEventAnimator;
    public string whisperAnimationTrigger = "";
    public string whisperAnimationBool = "";
    public bool whisperAnimationBoolValue = true;

    public enum GameEventsState { FallObjects, ThrowObjects, MovingObjects, RockingChair, WhisperEvent }
    public GameEventsState currentEventState;

    void Start()
    {
        // Cache squared distance for faster comparison
        triggerDistanceSquared = triggerDistance * triggerDistance;
        
        // Validate trigger object is set (or specific event object for certain event types)
        if (triggerObject == null && currentEventState != GameEventsState.WhisperEvent)
        {
            // Check if event has its own object assigned
            bool hasEventObject = false;
            switch (currentEventState)
            {
                case GameEventsState.FallObjects:
                    hasEventObject = fallObject != null;
                    break;
                case GameEventsState.ThrowObjects:
                    hasEventObject = throwObject != null;
                    break;
                case GameEventsState.MovingObjects:
                    hasEventObject = moveObject != null;
                    break;
                case GameEventsState.RockingChair:
                    hasEventObject = rockingChairObject != null;
                    break;
            }
            
            if (!hasEventObject)
            {
            }
        }
        
    }

    void Update()
    {
        if (eventTriggered) return;
        
        // Throttle checks to improve performance
        if (Time.time - lastCheckTime < checkInterval) return;
        lastCheckTime = Time.time;

        switch (triggerMode)
        {
            case TriggerMode.LookBased:
                if (CheckLookBasedTrigger())
                {
                    TriggerEvent();
                }
                break;

            case TriggerMode.DistanceBased:
                if (CheckDistanceBasedTrigger())
                {
                    TriggerEvent();
                }
                break;
        }
    }
    
    /// <summary>
    /// Checks if distance-based trigger condition is met
    /// </summary>
    private bool CheckDistanceBasedTrigger()
    {
        if (mainCamera == null) return false;

        Vector3 referencePosition = GetDistanceReferencePosition();
        
        if (referencePosition == Vector3.zero && distanceReferenceType != DistanceReferenceType.CustomPosition)
        {
            // Could not determine reference position
            return false;
        }

        Vector3 playerPos = mainCamera.transform.position;
        Vector3 refPos = referencePosition;

        // Use 2D distance if enabled (ignore Y axis)
        if (use2DDistance)
        {
            playerPos.y = 0f;
            refPos.y = 0f;
        }

        // Use sqrMagnitude for faster comparison (avoids square root calculation)
        float sqrDistance = (playerPos - refPos).sqrMagnitude;
        return sqrDistance <= triggerDistanceSquared;
    }
    
    /// <summary>
    /// Gets the reference position based on distanceReferenceType
    /// </summary>
    private Vector3 GetDistanceReferencePosition()
    {
        switch (distanceReferenceType)
        {
            case DistanceReferenceType.TriggerObject:
                if (triggerObject != null)
                    return triggerObject.transform.position;
                break;

            case DistanceReferenceType.EventObject:
                // Get event-specific object based on current event state
                GameObject eventObj = GetEventObject();
                if (eventObj != null)
                    return eventObj.transform.position;
                break;

            case DistanceReferenceType.CustomPosition:
                return customTriggerPosition;

            case DistanceReferenceType.CustomObject:
                if (customReferenceObject != null)
                    return customReferenceObject.transform.position;
                break;
        }

        return Vector3.zero;
    }
    
    /// <summary>
    /// Gets the event-specific object based on current event state
    /// </summary>
    private GameObject GetEventObject()
    {
        switch (currentEventState)
        {
            case GameEventsState.FallObjects:
                return fallObject != null ? fallObject : triggerObject;
            case GameEventsState.ThrowObjects:
                return throwObject != null ? throwObject : triggerObject;
            case GameEventsState.MovingObjects:
                return moveObject != null ? moveObject : triggerObject;
            case GameEventsState.RockingChair:
                return rockingChairObject != null ? rockingChairObject : triggerObject;
            default:
                return triggerObject;
        }
    }
    
    /// <summary>
    /// Checks if look-based trigger condition is met
    /// </summary>
    private bool CheckLookBasedTrigger()
    {
        if (mainCamera == null) return false;

        Vector3 cameraPos = mainCamera.transform.position;
        Vector3 cameraForward = mainCamera.transform.forward;
        Ray ray = new Ray(cameraPos, cameraForward);
        
        if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactionLayer))
        {
            return false;
        }

        // Check for obstructions between camera and hit point
        // Use sqrMagnitude for faster distance calculation
        Vector3 directionToHit = hit.point - cameraPos;
        float sqrDistanceToHit = directionToHit.sqrMagnitude;
        float distanceToHit = Mathf.Sqrt(sqrDistanceToHit);
        RaycastHit obstructionHit;
        
        // Use obstructionLayer if set, otherwise use everything except interactionLayer
        LayerMask obstructionCheckLayer = obstructionLayer.value != 0 ? obstructionLayer : ~interactionLayer;
        
        if (Physics.Raycast(cameraPos, directionToHit.normalized, out obstructionHit, distanceToHit, obstructionCheckLayer))
        {
            // Check if the obstruction is the target object itself (e.g., if object is on multiple layers)
            if (obstructionHit.collider != hit.collider)
            {
                // There's an obstruction blocking the view
                return false;
            }
        }
        
        // No obstruction or obstruction is the target itself - check if this is the target
        Transform hitTransform = hit.collider.transform;
        
        // Handle WhisperEvent - check if hit object is the triggerObject for this specific controller
        if (currentEventState == GameEventsState.WhisperEvent)
        {
            // WhisperEvent should only trigger if the hit object is this controller's triggerObject
            if (triggerObject != null)
            {
                return hitTransform == triggerObject.transform || hitTransform.IsChildOf(triggerObject.transform);
            }
            // If no triggerObject is set, don't trigger (safer than triggering on everything)
            return false;
        }
        
        if (triggerObject != null)
        {
            return hitTransform == triggerObject.transform || hitTransform.IsChildOf(triggerObject.transform);
        }
        
        // For RockingChair event, check if the hit object is the rocking chair
        if (currentEventState == GameEventsState.RockingChair && rockingChairObject != null)
        {
            return hitTransform == rockingChairObject.transform || hitTransform.IsChildOf(rockingChairObject.transform);
        }
        
        // If no trigger object set and not a specific event object case, trigger on any hit in the interaction layer
        if (currentEventState != GameEventsState.RockingChair && currentEventState != GameEventsState.WhisperEvent)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Triggers the event based on current state
    /// </summary>
    private void TriggerEvent()
    {
        if (eventTriggered) return;
        
        eventTriggered = true;
        
        // Double-check: if state is WhisperEvent, make sure we trigger WhisperEvent, not something else
        if (currentEventState == GameEventsState.WhisperEvent)
        {
            // Trigger animation if enabled
            if (enableAnimations && whisperEventAnimator != null)
            {
                TriggerAnimation(whisperEventAnimator, whisperAnimationTrigger, whisperAnimationBool, whisperAnimationBoolValue);
            }
            
            PlaySoundEffect(ghostBreath);
        }
        else
        {
            TriggerRandomEvent();
        }
    }


    public void TriggerFallEvent()
    {
        currentEventState = GameEventsState.FallObjects;
        StartCoroutine(FallEvent());
    }

    public void TriggerThrowEvent()
    {
        currentEventState = GameEventsState.ThrowObjects;
        StartCoroutine(ThrowEvent());
    }

    public void TriggerMoveEvent()
    {
        currentEventState = GameEventsState.MovingObjects;
        StartCoroutine(MoveEvent());
    }

    public void TriggerRockingChairEvent()
    {
        currentEventState = GameEventsState.RockingChair;
        StartCoroutine(RockingChairEvent());
    }

    public void TriggerWhisperEvent()
    {
        currentEventState = GameEventsState.WhisperEvent;
        
        // Trigger animation if enabled
        if (enableAnimations && whisperEventAnimator != null)
        {
            TriggerAnimation(whisperEventAnimator, whisperAnimationTrigger, whisperAnimationBool, whisperAnimationBoolValue);
        }
        
        PlaySoundEffect(ghostBreath);
    }

    public void TriggerRandomEvent()
    {
        switch (currentEventState)
        {
            case GameEventsState.FallObjects:
                StartCoroutine(FallEvent());
                break;
            case GameEventsState.ThrowObjects:
                StartCoroutine(ThrowEvent());
                break;
            case GameEventsState.MovingObjects:
                StartCoroutine(MoveEvent());
                break;
            case GameEventsState.RockingChair:
                StartCoroutine(RockingChairEvent());
                break;
            case GameEventsState.WhisperEvent:
                // Trigger animation if enabled
                if (enableAnimations && whisperEventAnimator != null)
                {
                    TriggerAnimation(whisperEventAnimator, whisperAnimationTrigger, whisperAnimationBool, whisperAnimationBoolValue);
                }
                PlaySoundEffect(ghostBreath);
                break;
            default:
                break;
        }
    }
    private IEnumerator FallEvent()
    {
        GameObject objToFall = fallObject != null ? fallObject : triggerObject;
        
        if (objToFall == null)
        {
            yield break;
        }

        // If animations are enabled, trigger ONLY the animation and skip transform manipulation
        if (enableAnimations)
        {
            Animator fallAnimator = GetOrFindAnimator(objToFall, fallObjectAnimator);
            if (fallAnimator != null)
            {
                TriggerAnimation(fallAnimator, fallAnimationTrigger, fallAnimationBool, fallAnimationBoolValue);
                PlaySoundEffect(fallObjectSoundEffect);
                yield break; // Exit early - only animation will play
            }
        }

        // Transform-based movement (only if animations are disabled or no animator found)
        PlaySoundEffect(fallObjectSoundEffect);

        Vector3 initialPosition = objToFall.transform.position;
        Quaternion initialRotation = objToFall.transform.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < fallDuration)
        {
            float t = elapsedTime / fallDuration;

            objToFall.transform.position = Vector3.Lerp(initialPosition, fallTargetPosition, t);
            objToFall.transform.rotation = Quaternion.Lerp(initialRotation, fallTargetRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        objToFall.transform.position = fallTargetPosition;
        objToFall.transform.rotation = fallTargetRotation;
    }


    private IEnumerator ThrowEvent()
    {
        GameObject objToThrow = throwObject != null ? throwObject : triggerObject;
        
        if (objToThrow == null)
        {
            yield break;
        }

        // If animations are enabled, trigger ONLY the animation and skip transform manipulation
        if (enableAnimations)
        {
            Animator throwAnimator = GetOrFindAnimator(objToThrow, throwObjectAnimator);
            if (throwAnimator != null)
            {
                TriggerAnimation(throwAnimator, throwAnimationTrigger, throwAnimationBool, throwAnimationBoolValue);
                
                yield return new WaitForSeconds(0.05f);
                PlaySoundEffect(throwSoundEffect);
                yield return new WaitForSeconds(0.05f);
                
                ghostBreath.enabled = true;
                ghostBreath.Play();
                yield break; // Exit early - only animation will play
            }
        }

        // Transform-based movement (only if animations are disabled or no animator found)
        Vector3 startPosition = objToThrow.transform.position;
        Vector3 endPosition = throwTargetPosition;

        float elapsedTime = 0f;

        Quaternion impactRotation = Quaternion.Euler(74f, -90f, 0f);

        float speed = Vector3.Distance(startPosition, endPosition) / throwDuration;

        while (elapsedTime < throwDuration)
        {
            float t = elapsedTime / throwDuration;

            Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, t);
            currentPosition.y += throwHeight * Mathf.Sin(Mathf.PI * t);
            objToThrow.transform.position = currentPosition;

            objToThrow.transform.Rotate(Vector3.forward * 600f * Time.deltaTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        objToThrow.transform.position = endPosition;
        objToThrow.transform.rotation = impactRotation;

        yield return new WaitForSeconds(0.05f);

        PlaySoundEffect(throwSoundEffect);

        yield return new WaitForSeconds(0.05f);

        ghostBreath.enabled = true;
        ghostBreath.Play();
    }

    private IEnumerator MoveEvent()
    {
        GameObject objToMove = moveObject != null ? moveObject : triggerObject;
        
        if (objToMove == null)
        {
            yield break;
        }

        // If animations are enabled, trigger ONLY the animation and skip transform manipulation
        if (enableAnimations)
        {
            Animator moveAnimator = GetOrFindAnimator(objToMove, moveObjectAnimator);
            if (moveAnimator != null)
            {
                TriggerAnimation(moveAnimator, moveAnimationTrigger, moveAnimationBool, moveAnimationBoolValue);
                PlaySoundEffect(moveSoundEffect);
                yield break; // Exit early - only animation will play
            }
        }

        // Transform-based movement (only if animations are disabled or no animator found)
        PlaySoundEffect(moveSoundEffect);

        Vector3 initialPosition = objToMove.transform.position;
        Vector3 initialRotationEuler = objToMove.transform.eulerAngles;
        Vector3 targetPosition;
        Vector3 targetRotationEuler;

        // Calculate target position (absolute or relative)
        if (useRelativePosition)
        {
            // moveTargetPosition is an offset from the initial position
            targetPosition = initialPosition + moveTargetPosition;
        }
        else
        {
            // moveTargetPosition is an absolute world position
            targetPosition = moveTargetPosition;
        }

        // Determine target rotation
        if (useEulerAngles)
        {
            // Use Euler angles directly - preserves exact values like 270
            targetRotationEuler = moveTargetRotationEuler;
        }
        else
        {
            // Convert from Quaternion to Euler (legacy support)
            targetRotationEuler = moveTargetRotation.eulerAngles;
        }

        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            
            // Interpolate position
            objToMove.transform.position = Vector3.Lerp(initialPosition, targetPosition, t);
            
            // Interpolate Euler angles directly to preserve exact values
            Vector3 currentRotationEuler = new Vector3(
                Mathf.LerpAngle(initialRotationEuler.x, targetRotationEuler.x, t),
                Mathf.LerpAngle(initialRotationEuler.y, targetRotationEuler.y, t),
                Mathf.LerpAngle(initialRotationEuler.z, targetRotationEuler.z, t)
            );
            
            objToMove.transform.rotation = Quaternion.Euler(currentRotationEuler);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Set final values exactly
        objToMove.transform.position = targetPosition;
        objToMove.transform.rotation = Quaternion.Euler(targetRotationEuler);
    }

    private IEnumerator RockingChairEvent()
    {
        GameObject chairObject = rockingChairObject != null ? rockingChairObject : triggerObject;
        
        if (chairObject == null)
        {
            yield break;
        }

        // If animations are enabled, trigger ONLY the animation and skip transform manipulation
        if (enableAnimations)
        {
            Animator chairAnimator = GetOrFindAnimator(chairObject, rockingChairAnimator);
            if (chairAnimator != null)
            {
                TriggerAnimation(chairAnimator, rockingChairAnimationTrigger, rockingChairAnimationBool, rockingChairAnimationBoolValue);
                PlaySoundEffect(rockingChairSoundEffect);
                yield break; // Exit early - only animation will play
            }
        }

        // Transform-based movement (only if animations are disabled or no animator found)
        PlaySoundEffect(rockingChairSoundEffect);

        // Store the initial rotation as the rest position
        Quaternion initialRotation = chairObject.transform.rotation;
        
        // Normalize the rocking axis
        Vector3 normalizedAxis = rockingAxis.normalized;

        // Calculate total duration for all cycles
        float totalDuration = rockingCycleDuration * rockingCycles;
        float elapsedTime = 0f;

        // Use sine wave for smooth, natural rocking motion
        while (elapsedTime < totalDuration)
        {
            // Calculate which cycle we're in (0 to rockingCycles)
            float cycleProgress = (elapsedTime / totalDuration) * rockingCycles;
            
            // Use sine wave to create smooth back-and-forth rocking motion
            // Sin gives us values from -1 to 1, which we multiply by the rocking angle
            float sineValue = Mathf.Sin(cycleProgress * 2f * Mathf.PI);
            float currentAngle = sineValue * rockingAngle;
            
            // Apply easing curve if available for smoother deceleration at the end
            if (rockingCurve != null && rockingCurve.length > 0)
            {
                float normalizedTime = elapsedTime / totalDuration;
                float curveValue = rockingCurve.Evaluate(normalizedTime);
                // Blend between sine motion and eased motion for smoother end
                if (normalizedTime > 0.7f) // Only apply easing in the last 30%
                {
                    float easeFactor = (normalizedTime - 0.7f) / 0.3f;
                    currentAngle = Mathf.Lerp(currentAngle, 0f, easeFactor * curveValue);
                }
            }
            
            // Apply rotation around the specified axis
            chairObject.transform.rotation = initialRotation * Quaternion.AngleAxis(currentAngle, normalizedAxis);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure we're back at the exact initial rotation
        chairObject.transform.rotation = initialRotation;
    }

    private void PlaySoundEffect(AudioSource audioSource)
    {
        if (audioSource != null)
        {
            audioSource.enabled = true;
            
            // Make sure the GameObject is active
            if (!audioSource.gameObject.activeInHierarchy)
            {
                audioSource.gameObject.SetActive(true);
            }
            
            audioSource.Play();
            
        }
    }

    /// <summary>
    /// Triggers an animation on the specified Animator. Supports both triggers and bool parameters.
    /// </summary>
    /// <param name="animator">The Animator component to trigger animation on</param>
    /// <param name="triggerName">Trigger parameter name (if using trigger)</param>
    /// <param name="boolName">Bool parameter name (if using bool)</param>
    /// <param name="boolValue">Bool parameter value (if using bool)</param>
    private void TriggerAnimation(Animator animator, string triggerName, string boolName, bool boolValue)
    {
        if (!enableAnimations) return;
        if (animator == null) return;

        // Use trigger if specified
        if (!string.IsNullOrEmpty(triggerName))
        {
            animator.SetTrigger(triggerName);
        }
        // Otherwise use bool parameter
        else if (!string.IsNullOrEmpty(boolName))
        {
            animator.SetBool(boolName, boolValue);
        }
    }

    /// <summary>
    /// Gets or finds the Animator component for a given GameObject
    /// </summary>
    private Animator GetOrFindAnimator(GameObject targetObject, Animator assignedAnimator)
    {
        if (!enableAnimations) return null;
        if (targetObject == null) return null;

        // Use assigned animator if available
        if (assignedAnimator != null) return assignedAnimator;

        // Auto-find if enabled
        if (autoFindAnimator)
        {
            Animator foundAnimator = targetObject.GetComponent<Animator>();
            if (foundAnimator == null)
            {
                // Try to find in children
                foundAnimator = targetObject.GetComponentInChildren<Animator>();
            }
            return foundAnimator;
        }

        return null;
    }

}