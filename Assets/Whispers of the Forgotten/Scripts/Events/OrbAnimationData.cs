using UnityEngine;

[System.Serializable]
public class OrbAnimationData
{
    [Header("Animation Settings")]
    [Tooltip("The Animator component to trigger animation on (leave empty to auto-find on orb)")]
    public Animator targetAnimator;
    
    [Tooltip("Animation trigger name to set (leave empty if using bool parameter)")]
    public string animationTrigger = "";
    
    [Tooltip("Animation bool parameter name (only used if trigger is empty)")]
    public string animationBool = "";
    
    [Tooltip("Animation bool parameter value (only used if using bool parameter)")]
    public bool animationBoolValue = true;
    
    [Header("Trigger Settings")]
    [Tooltip("If true, this animation will trigger when player looks at the orb (raycast detection)")]
    public bool triggerOnSight = false;
    
    [Tooltip("If true, the orb will be at a static position after this animation (waiting to be seen)")]
    public bool isStaticPosition = false;
    
    [Header("Subtitle Settings")]
    [Tooltip("If true, show subtitle when this animation triggers")]
    public bool showSubtitle = false;
    
    [TextArea(2, 4)]
    [Tooltip("Subtitle text to display when this animation triggers")]
    public string subtitleText = "";
    
    [Tooltip("Duration in seconds to show the subtitle")]
    public float subtitleDuration = 5f;
    
    [Header("Audio Settings")]
    [Tooltip("Audio source to play when animation triggers (optional)")]
    public AudioSource soundEffect;
    
    [Tooltip("If true, applies random pitch variation (0.9 to 1.1)")]
    public bool useRandomPitch = false;
    
    [Header("Object Visibility")]
    [Tooltip("GameObjects to show when this animation triggers (e.g., for last animation)")]
    public GameObject[] objectsToShow;
    
    [Tooltip("If true, these objects will be shown when animation triggers")]
    public bool showObjectsOnTrigger = false;
}

