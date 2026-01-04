using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using System.Linq;

/// <summary>
/// Controller for VHS/Glitch screen effect that can be triggered on demand
/// </summary>
public class VHSEffectController : MonoBehaviour
{
    [Header("Post Processing Settings")]
    [Tooltip("Post Process Volume that contains the VHS/Glitch effects")]
    public PostProcessVolume vhsPostProcessVolume;
    
    [Tooltip("Main camera to apply effects to (auto-finds if not assigned)")]
    public Camera mainCamera;
    
    [Header("Effect Settings")]
    [Tooltip("Duration of the VHS/Glitch effect in seconds")]
    public float effectDuration = 2f;
    
    [Tooltip("Intensity curve for the effect (0 = start, 1 = peak, 2 = end)")]
    public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Auto Setup")]
    [Tooltip("Auto-find main camera if not assigned")]
    public bool autoFindCamera = true;
    
    [Header("Testing")]
    [Tooltip("Enable test mode - press T key to trigger effect")]
    public bool enableTestMode = true;
    
    [Tooltip("Key to press for testing (default: T)")]
    public KeyCode testKey = KeyCode.T;
    
    private float currentEffectTime = 0f;
    private bool isEffectActive = false;
    private float originalVolumeWeight = 0f;
    private DigitalGlitchPostProcess digitalGlitchEffect;
    private float originalIntensity = 0f;
    
    void Start()
    {
        // Auto-find camera if needed
        if (mainCamera == null && autoFindCamera)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
        }
        
        // Try to find Post Process Volume if not assigned
        if (vhsPostProcessVolume == null)
        {
            // Try to find in scene
            vhsPostProcessVolume = FindObjectOfType<PostProcessVolume>();
            
            // Try to find Post Process Volume on camera
            if (vhsPostProcessVolume == null && mainCamera != null)
            {
                vhsPostProcessVolume = mainCamera.GetComponent<PostProcessVolume>();
                if (vhsPostProcessVolume == null)
                {
                    // Try to find in children
                    vhsPostProcessVolume = mainCamera.GetComponentInChildren<PostProcessVolume>();
                }
            }
        }
        
        // Store original volume weight
        if (vhsPostProcessVolume != null)
        {
            originalVolumeWeight = vhsPostProcessVolume.weight;
            
            // Try to get Digital Glitch effect from profile
            if (vhsPostProcessVolume.profile != null)
            {
                if (vhsPostProcessVolume.profile.TryGetSettings<DigitalGlitchPostProcess>(out digitalGlitchEffect))
                {
                    originalIntensity = digitalGlitchEffect.intensity.value;
                }
            }
        }
        
        // Initialize curve if not set
        if (intensityCurve == null || intensityCurve.keys.Length == 0)
        {
            // Create a curve that goes from 0 to 1 and back to 0
            intensityCurve = new AnimationCurve(
                new Keyframe(0f, 0f),
                new Keyframe(0.3f, 1f),
                new Keyframe(1f, 0f)
            );
        }
    }
    
    void Update()
    {
        // Test mode - press key to trigger effect
        if (enableTestMode && Input.GetKeyDown(testKey))
        {
            TriggerEffect();
        }
        
        if (isEffectActive)
        {
            currentEffectTime += Time.deltaTime;
            float normalizedTime = currentEffectTime / effectDuration;
            
            // Clamp normalized time to 0-1 range
            normalizedTime = Mathf.Clamp01(normalizedTime);
            
            // Apply intensity curve
            float intensity = intensityCurve.Evaluate(normalizedTime);
            
            if (vhsPostProcessVolume != null)
            {
                // Ensure volume is enabled
                if (!vhsPostProcessVolume.enabled)
                {
                    vhsPostProcessVolume.enabled = true;
                }
                
                // Fade in and out the effect
                vhsPostProcessVolume.weight = intensity;
                
                // Update Digital Glitch intensity if available
                if (digitalGlitchEffect != null)
                {
                    digitalGlitchEffect.intensity.value = intensity;
                }
            }
            
            // Check if effect should end
            if (currentEffectTime >= effectDuration)
            {
                StopEffect();
            }
        }
    }
    
    /// <summary>
    /// Trigger the VHS/Glitch effect
    /// </summary>
    public void TriggerEffect()
    {
        if (vhsPostProcessVolume == null)
        {
            return;
        }
        
        // Check if volume has a profile
        if (vhsPostProcessVolume.profile == null)
        {
            return;
        }
        
        isEffectActive = true;
        currentEffectTime = 0f;
        
        // Ensure volume is enabled
        if (vhsPostProcessVolume != null)
        {
            vhsPostProcessVolume.enabled = true;
            // Set initial weight to 0 to start the fade in
            vhsPostProcessVolume.weight = 0f;
        }
    }
    
    /// <summary>
    /// Trigger the effect with custom duration
    /// </summary>
    public void TriggerEffect(float duration)
    {
        effectDuration = duration;
        TriggerEffect();
    }
    
    /// <summary>
    /// Stop the effect immediately
    /// </summary>
    public void StopEffect()
    {
        isEffectActive = false;
        currentEffectTime = 0f;
        
        if (vhsPostProcessVolume != null)
        {
            vhsPostProcessVolume.weight = originalVolumeWeight;
            
            // Reset Digital Glitch intensity if available
            if (digitalGlitchEffect != null)
            {
                digitalGlitchEffect.intensity.value = originalIntensity;
            }
        }
    }
    
    /// <summary>
    /// Check if effect is currently active
    /// </summary>
    public bool IsEffectActive()
    {
        return isEffectActive;
    }
    
    // All debug logging removed per project settings.
}

