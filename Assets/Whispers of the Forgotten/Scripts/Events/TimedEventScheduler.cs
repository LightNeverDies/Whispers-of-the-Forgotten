using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum TimedEventType
{
    AnimationAndSound,  // Trigger animation and sound
    PowerOff            // Turn off power (triggers all PowerReactiveObjects) + optional sound effect
}

[System.Serializable]
public class TimedEvent
{
    [Tooltip("Delay in seconds from game start before this event triggers")]
    public float triggerDelay = 5f;
    
    [Header("Event Type")]
    [Tooltip("What type of event to trigger")]
    public TimedEventType eventType = TimedEventType.AnimationAndSound;
    
    [Header("Power Off Settings")]
    [Tooltip("PowerButtonController to use for power off (will auto-find if not set)")]
    public PowerButtonController powerButtonController;
    
    [Header("Target Object (for Animation events only)")]
    [Tooltip("The GameObject with the Animator component (leave empty to use the scheduler's GameObject)")]
    public GameObject targetObject;
    
    [Header("Animation Settings")]
    [Tooltip("Animation trigger name to set (leave empty if using bool parameter)")]
    public string animationTrigger = "";
    [Tooltip("Animation bool parameter name (only used if trigger is empty)")]
    public string animationBool = "";
    [Tooltip("Animation bool parameter value (only used if using bool parameter)")]
    public bool animationBoolValue = true;
    [Tooltip("The Animator component (optional, will auto-find if not set)")]
    public Animator targetAnimator;
    
    [Header("Audio Settings")]
    [Tooltip("Audio source to play when event triggers")]
    public AudioSource soundEffect;
    [Tooltip("Second audio source to play when event triggers (optional)")]
    public AudioSource soundEffect2;
    [Tooltip("If true, applies random pitch variation (0.9 to 1.1) like glass hit")]
    public bool useRandomPitch = true;
    
    [Header("Subtitle Settings")]
    [Tooltip("If true, show subtitle when event triggers")]
    public bool needsSubtitle = false;
    [TextArea(2, 4)]
    [Tooltip("Subtitle text to display when event triggers (first time)")]
    public string subtitleText = "";
    [Tooltip("Duration in seconds to show the subtitle")]
    public float subtitleDuration = 5f;
    
    [Header("Repeat Subtitle Settings")]
    [Tooltip("If true, use different subtitle text when event repeats (only used if canRepeat is true)")]
    public bool useDifferentSubtitleOnRepeat = false;
    [TextArea(2, 4)]
    [Tooltip("Subtitle text to display when event repeats (leave empty to use subtitleText)")]
    public string repeatSubtitleText = "";
    [Tooltip("Duration in seconds to show the repeat subtitle (uses subtitleDuration if 0)")]
    public float repeatSubtitleDuration = 0f;
    
    [Header("Post-Animation Subtitle Settings")]
    [Tooltip("If true, show subtitle after animation completes (each line sequentially)")]
    public bool showSubtitleAfterAnimation = false;
    [TextArea(2, 4)]
    [Tooltip("Subtitle text to display after animation (each line will show sequentially)")]
    public string subtitleAfterAnimationText = "";
    [Tooltip("Duration in seconds to show each line of the post-animation subtitle")]
    public float subtitleAfterAnimationDuration = 5f;
    
    [Header("Repeating")]
    [Tooltip("If true, this event can trigger multiple times (resets after triggerDelay)")]
    public bool canRepeat = false;
    [Tooltip("Delay between repeats (only used if canRepeat is true)")]
    public float repeatDelay = 10f;

    // Internal state
    [HideInInspector]
    public bool hasTriggered = false;
}

public class TimedEventScheduler : MonoBehaviour
{
    [Header("Event List")]
    [Tooltip("List of timed events to trigger")]
    public List<TimedEvent> timedEvents = new List<TimedEvent>();
    
    [Header("Settings")]
    [Tooltip("If true, will automatically find Animator component on target objects if not assigned")]
    public bool autoFindAnimator = true;
    [Tooltip("Start triggering events immediately on Start()")]
    public bool startOnStart = true;
    [Tooltip("If true, will automatically find PowerButtonController if not assigned in events")]
    public bool autoFindPowerController = true;
    [Tooltip("SubtitleManager to use for displaying subtitles (will auto-find if not assigned)")]
    public SubtitleManager subtitleManager;
    
    private float gameStartTime;
    private PowerButtonController defaultPowerController;

    void Start()
    {
        gameStartTime = Time.time;
        
        // Auto-find PowerButtonController if enabled
        if (autoFindPowerController)
        {
            defaultPowerController = FindObjectOfType<PowerButtonController>();
        }
        
        // Auto-find SubtitleManager if not assigned
        if (subtitleManager == null)
        {
            subtitleManager = FindObjectOfType<SubtitleManager>();
        }
        
        if (startOnStart)
        {
            StartAllEvents();
        }
    }
    
    /// <summary>
    /// Start all timed events
    /// </summary>
    public void StartAllEvents()
    {
        foreach (var timedEvent in timedEvents)
        {
            if (timedEvent.canRepeat)
            {
                // For repeating events, start a repeating coroutine
                StartCoroutine(TriggerRepeatingEvent(timedEvent));
            }
            else
            {
                // For one-time events, start a single coroutine
                StartCoroutine(TriggerSingleEvent(timedEvent));
            }
        }
    }
    
    /// <summary>
    /// Stop all timed events
    /// </summary>
    public void StopAllEvents()
    {
        StopAllCoroutines();
    }
    
    private IEnumerator TriggerSingleEvent(TimedEvent timedEvent)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(timedEvent.triggerDelay);
        
        // Trigger the event (which may be a coroutine for animations)
        yield return StartCoroutine(ExecuteEventCoroutine(timedEvent, false));
    }
    
    private IEnumerator TriggerRepeatingEvent(TimedEvent timedEvent)
    {
        // Wait for initial delay
        yield return new WaitForSeconds(timedEvent.triggerDelay);
        
        // Trigger the first event (which may be a coroutine for animations)
        yield return StartCoroutine(ExecuteEventCoroutine(timedEvent, false));
        timedEvent.hasTriggered = true;
        
        // For repeating events, check conditions before each repeat
        while (true)
        {
            // Wait for repeat delay before checking again
            yield return new WaitForSeconds(timedEvent.repeatDelay);
            
            // For PowerOff events, check if power is ON before repeating
            if (timedEvent.eventType == TimedEventType.PowerOff)
            {
                PowerButtonController powerController = GetPowerController(timedEvent);
                if (powerController != null && powerController.hasPower)
                {
                    // Power is ON, trigger the event again (turn power off) - this is a repeat
                    yield return StartCoroutine(ExecuteEventCoroutine(timedEvent, true));
                }
                // If power is OFF, don't trigger - just wait and check again next cycle
            }
            else
            {
                // For non-PowerOff events, always trigger - this is a repeat
                yield return StartCoroutine(ExecuteEventCoroutine(timedEvent, true));
            }
        }
    }
    
    private IEnumerator ExecuteEventCoroutine(TimedEvent timedEvent, bool isRepeat = false)
    {
        // Determine which subtitle text and duration to use
        string subtitleTextToUse = timedEvent.subtitleText;
        float subtitleDurationToUse = timedEvent.subtitleDuration;
        
        // If this is a repeat and we're using different subtitle text for repeats
        if (isRepeat && timedEvent.useDifferentSubtitleOnRepeat)
        {
            if (!string.IsNullOrEmpty(timedEvent.repeatSubtitleText))
            {
                subtitleTextToUse = timedEvent.repeatSubtitleText;
            }
            if (timedEvent.repeatSubtitleDuration > 0f)
            {
                subtitleDurationToUse = timedEvent.repeatSubtitleDuration;
            }
        }
        
        // Handle power off events
        if (timedEvent.eventType == TimedEventType.PowerOff)
        {
            TriggerPowerOff(timedEvent);
            
            // Show subtitle if needed
            if (timedEvent.needsSubtitle && !string.IsNullOrEmpty(subtitleTextToUse) && subtitleManager != null)
            {
                subtitleManager.ShowSubtitle(subtitleTextToUse, subtitleDurationToUse);
            }
            
            // Play optional sound effects (in addition to the button click sound from PowerButtonController)
            PlaySoundEffect(timedEvent.soundEffect, timedEvent.useRandomPitch);
            PlaySoundEffect(timedEvent.soundEffect2, timedEvent.useRandomPitch);
        }
        // Handle animation and sound events
        else if (timedEvent.eventType == TimedEventType.AnimationAndSound)
        {
            // Play sound effects first (with optional random pitch)
            PlaySoundEffect(timedEvent.soundEffect, timedEvent.useRandomPitch);
            PlaySoundEffect(timedEvent.soundEffect2, timedEvent.useRandomPitch);
            
            // Show subtitle if needed
            if (timedEvent.needsSubtitle && !string.IsNullOrEmpty(subtitleTextToUse) && subtitleManager != null)
            {
                subtitleManager.ShowSubtitle(subtitleTextToUse, subtitleDurationToUse);
            }
            
            // Trigger animation
            Animator animator = GetAnimator(timedEvent);
            if (animator != null)
            {
                // Use trigger if specified
                if (!string.IsNullOrEmpty(timedEvent.animationTrigger))
                {
                    animator.SetTrigger(timedEvent.animationTrigger);
                }
                // Otherwise use bool parameter
                else if (!string.IsNullOrEmpty(timedEvent.animationBool))
                {
                    animator.SetBool(timedEvent.animationBool, timedEvent.animationBoolValue);
                }
                
                // Wait for animation to complete if we have an animation
                if (!string.IsNullOrEmpty(timedEvent.animationTrigger) || !string.IsNullOrEmpty(timedEvent.animationBool))
                {
                    // Wait a bit for animation to start, then check if it's still playing
                    yield return new WaitForSeconds(0.1f);
                    
                    // Wait for animation to finish (approximate - check animator state)
                    float animationTime = 0f;
                    float maxWaitTime = 10f; // Safety timeout
                    
                    while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f && animationTime < maxWaitTime)
                    {
                        animationTime += Time.deltaTime;
                        yield return null;
                    }
                }
            }
            else if (!string.IsNullOrEmpty(timedEvent.animationTrigger) || !string.IsNullOrEmpty(timedEvent.animationBool))
            {
                GameObject targetObj = timedEvent.targetObject != null ? timedEvent.targetObject : gameObject;
            }
            
            // Show subtitle after animation if configured (each line sequentially)
            if (timedEvent.showSubtitleAfterAnimation && !string.IsNullOrEmpty(timedEvent.subtitleAfterAnimationText) && subtitleManager != null)
            {
                yield return StartCoroutine(ShowSequentialSubtitles(timedEvent.subtitleAfterAnimationText, timedEvent.subtitleAfterAnimationDuration));
            }
        }
    }

    private void PlaySoundEffect(AudioSource source, bool useRandomPitch)
    {
        if (source == null)
            return;

        source.gameObject.SetActive(true);

        if (useRandomPitch)
        {
            source.pitch = Random.Range(0.9f, 1.1f);
        }

        source.Play();
    }
    
    private IEnumerator ShowSequentialSubtitles(string subtitleText, float durationPerLine)
    {
        // Split text by newlines and filter out empty lines
        string[] lines = subtitleText.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string line in lines)
        {
            // Trim whitespace from each line
            string trimmedLine = line.Trim();
            
            // Skip empty lines
            if (string.IsNullOrEmpty(trimmedLine))
                continue;
            
            // Show this line
            subtitleManager.ShowSubtitle(trimmedLine, durationPerLine);
            
            // Wait for the subtitle duration before showing the next line
            yield return new WaitForSeconds(durationPerLine);
        }
    }
    
    private void TriggerPowerOff(TimedEvent timedEvent)
    {
        PowerButtonController powerController = GetPowerController(timedEvent);
        
        if (powerController != null)
        {
            powerController.CutPowerFromEvent();
        }
    }
    
    private PowerButtonController GetPowerController(TimedEvent timedEvent)
    {
        PowerButtonController powerController = timedEvent.powerButtonController;
        
        // Auto-find if not assigned and auto-find is enabled
        if (powerController == null && autoFindPowerController)
        {
            powerController = defaultPowerController;
        }
        
        return powerController;
    }
    
    private Animator GetAnimator(TimedEvent timedEvent)
    {
        // Use assigned animator if available
        if (timedEvent.targetAnimator != null)
            return timedEvent.targetAnimator;
        
        // Determine which GameObject to search
        GameObject objToSearch = timedEvent.targetObject != null ? timedEvent.targetObject : gameObject;
        
        // Auto-find if enabled
        if (autoFindAnimator)
        {
            Animator foundAnimator = objToSearch.GetComponent<Animator>();
            if (foundAnimator == null)
            {
                // Try to find in children
                foundAnimator = objToSearch.GetComponentInChildren<Animator>();
            }
            return foundAnimator;
        }
        
        return null;
    }
    
    /// <summary>
    /// Manually trigger a specific event by index
    /// </summary>
    public void TriggerEvent(int eventIndex)
    {
        if (eventIndex >= 0 && eventIndex < timedEvents.Count)
        {
            StartCoroutine(ExecuteEventCoroutine(timedEvents[eventIndex], false));
        }
        else
        {
        }
    }
    
    /// <summary>
    /// Reset all events (useful for testing or restarting)
    /// </summary>
    public void ResetAllEvents()
    {
        foreach (var timedEvent in timedEvents)
        {
            timedEvent.hasTriggered = false;
        }
    }
}

