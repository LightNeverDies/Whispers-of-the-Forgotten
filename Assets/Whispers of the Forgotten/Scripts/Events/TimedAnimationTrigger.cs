using System.Collections;
using UnityEngine;

public class TimedAnimationTrigger : MonoBehaviour
{
    [Header("Timing Settings")]
    [Tooltip("Delay in seconds before triggering the animation and sound")]
    public float triggerDelay = 3f;

    [Tooltip("If true, automatically triggers after triggerDelay on Start(). Disable this if you want to trigger via code (e.g., look triggers).")]
    public bool triggerOnStart = true;
    
    [Header("Animation Settings")]
    [Tooltip("The GameObject with the Animator component (leave empty to use this GameObject)")]
    public GameObject targetObject;
    [Tooltip("Animation trigger name to set (leave empty if using bool parameter)")]
    public string animationTrigger = "";
    [Tooltip("Animation bool parameter name (only used if trigger is empty)")]
    public string animationBool = "";
    [Tooltip("Animation bool parameter value (only used if using bool parameter)")]
    public bool animationBoolValue = true;
    [Tooltip("If true, will automatically find Animator component on the target object if not assigned")]
    public bool autoFindAnimator = true;
    [Tooltip("The Animator component (leave empty if autoFindAnimator is true)")]
    public Animator targetAnimator;
    
    [Header("Audio Settings")]
    [Tooltip("Audio source to play when animation triggers")]
    public AudioSource soundEffect;
    [Tooltip("If true, applies random pitch variation (0.9 to 1.1) like glass hit")]
    public bool useRandomPitch = true;
    
    [Header("Subtitle Integration")]
    [Tooltip("SubtitleManager to use for displaying subtitles (will auto-find if not assigned)")]
    public SubtitleManager subtitleManager;
    
    [Header("Subtitle Settings")]
    [Tooltip("If true, show subtitle when animation triggers")]
    public bool needsSubtitle = false;
    [TextArea(2, 4)]
    [Tooltip("Subtitle text to display when animation triggers")]
    public string subtitleText = "";
    [Tooltip("Duration in seconds to show the subtitle")]
    public float subtitleDuration = 5f;
    
    [Header("Post-Animation Subtitle Settings")]
    [Tooltip("If true, show subtitle after animation completes (each line sequentially)")]
    public bool showSubtitleAfterAnimation = false;
    [TextArea(2, 4)]
    [Tooltip("Subtitle text to display after animation (each line will show sequentially)")]
    public string subtitleAfterAnimationText = "";
    [Tooltip("Duration in seconds to show each line of the post-animation subtitle")]
    public float subtitleAfterAnimationDuration = 5f;
    
    private bool hasTriggered = false;

    void Awake()
    {
        // Auto-find SubtitleManager if not assigned
        if (subtitleManager == null)
        {
            subtitleManager = FindObjectOfType<SubtitleManager>();
        }
    }

    void Start()
    {
        if (triggerOnStart)
        {
            // Start the coroutine to trigger after delay
            StartCoroutine(TriggerAfterDelay());
        }
    }
    
    private IEnumerator TriggerAfterDelay()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(triggerDelay);
        
        // Trigger the animation and sound
        yield return StartCoroutine(TriggerAnimationAndSoundCoroutine());
    }
    
    private IEnumerator TriggerAnimationAndSoundCoroutine()
    {
        if (hasTriggered) yield break;
        hasTriggered = true;
        
        // Play sound effect first (with optional random pitch)
        if (soundEffect != null)
        {
            soundEffect.gameObject.SetActive(true);
            
            if (useRandomPitch)
            {
                soundEffect.pitch = Random.Range(0.9f, 1.1f);
            }
            
            soundEffect.Play();
        }
        
        // Show subtitle if needed
        if (needsSubtitle && !string.IsNullOrEmpty(subtitleText) && subtitleManager != null)
        {
            subtitleManager.ShowSubtitle(subtitleText, subtitleDuration);
        }
        
        // Trigger animation
        Animator animator = GetAnimator();
        if (animator != null)
        {
            // Use trigger if specified
            if (!string.IsNullOrEmpty(animationTrigger))
            {
                animator.SetTrigger(animationTrigger);
            }
            // Otherwise use bool parameter
            else if (!string.IsNullOrEmpty(animationBool))
            {
                animator.SetBool(animationBool, animationBoolValue);
            }
            
            // Wait for animation to complete (check if animation is playing)
            if (!string.IsNullOrEmpty(animationTrigger) || !string.IsNullOrEmpty(animationBool))
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
        else
        {
        }
        
        // Show subtitle after animation if configured (each line sequentially)
        if (showSubtitleAfterAnimation && !string.IsNullOrEmpty(subtitleAfterAnimationText) && subtitleManager != null)
        {
            yield return StartCoroutine(ShowSequentialSubtitles(subtitleAfterAnimationText, subtitleAfterAnimationDuration));
        }
    }

    /// <summary>
    /// Returns a routine you can yield on to wait for the trigger to fully finish (audio kick + animation + optional post subtitles).
    /// Useful for sequencing multiple actions.
    /// </summary>
    public IEnumerator TriggerNowRoutine()
    {
        yield return StartCoroutine(TriggerAnimationAndSoundCoroutine());
    }

    /// <summary>
    /// Returns a routine that waits delay seconds and then triggers (and can be yielded on).
    /// </summary>
    public IEnumerator TriggerAfterDelayRoutine(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }
        yield return StartCoroutine(TriggerAnimationAndSoundCoroutine());
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
    
    /// <summary>
    /// Public method to manually trigger (useful for testing or other scripts)
    /// </summary>
    public void ManualTrigger()
    {
        if (!hasTriggered)
        {
            StartCoroutine(TriggerAnimationAndSoundCoroutine());
        }
    }
    
    private Animator GetAnimator()
    {
        // Use assigned animator if available
        if (targetAnimator != null)
            return targetAnimator;
        
        // Determine which GameObject to search
        GameObject objToSearch = targetObject != null ? targetObject : gameObject;
        
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
    /// Reset the trigger so it can be triggered again
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}

