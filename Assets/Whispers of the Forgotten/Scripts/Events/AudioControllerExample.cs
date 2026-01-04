using UnityEngine;

/// <summary>
/// Example script showing how to use the different audio controllers
/// This demonstrates various ways to trigger and control audio sequences
/// </summary>
public class AudioControllerExample : MonoBehaviour
{
    [Header("Audio Controller References")]
    public ReusableAudioController audioController;
    public InteractiveAudioController interactiveAudioController;
    public ProximityAudioController proximityAudioController;
    
    [Header("Example Settings")]
    public KeyCode triggerKey = KeyCode.Space;
    public KeyCode stopKey = KeyCode.Escape;
    public KeyCode nextSoundKey = KeyCode.N;
    public KeyCode previousSoundKey = KeyCode.P;
    
    void Start()
    {
        // Auto-find audio controllers if not assigned
        if (audioController == null)
            audioController = GetComponent<ReusableAudioController>();
        
        if (interactiveAudioController == null)
            interactiveAudioController = GetComponent<InteractiveAudioController>();
        
        if (proximityAudioController == null)
            proximityAudioController = GetComponent<ProximityAudioController>();
        
        if (audioController == null && interactiveAudioController == null && proximityAudioController == null)
        {
        }
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        // Trigger audio sequence
        if (Input.GetKeyDown(triggerKey))
        {
            if (audioController != null)
                audioController.StartAudioSequence();
            if (interactiveAudioController != null)
                interactiveAudioController.StartAudioSequence();
            if (proximityAudioController != null)
                proximityAudioController.StartAudioSequence();
        }
        
        // Stop audio sequence
        if (Input.GetKeyDown(stopKey))
        {
            if (audioController != null)
                audioController.StopAudioSequence();
            if (interactiveAudioController != null)
                interactiveAudioController.StopAudioSequence();
            if (proximityAudioController != null)
                proximityAudioController.StopAudioSequence();
        }
        
        // Play next sound in sequence
        if (Input.GetKeyDown(nextSoundKey))
        {
            if (audioController != null)
                audioController.PlayNextSound();
            if (interactiveAudioController != null)
                interactiveAudioController.PlayNextSound();
            if (proximityAudioController != null)
                proximityAudioController.PlayNextSound();
        }
        
        // Play previous sound in sequence
        if (Input.GetKeyDown(previousSoundKey))
        {
            if (audioController != null)
                audioController.PlayPreviousSound();
            if (interactiveAudioController != null)
                interactiveAudioController.PlayPreviousSound();
            if (proximityAudioController != null)
                proximityAudioController.PlayPreviousSound();
        }
    }
    
    // Example method to trigger audio from external events
    public void TriggerAudioFromEvent()
    {
        if (audioController != null)
        {
            audioController.StartAudioSequence();
        }
    }
    
    // Example method to play specific sound by index
    public void PlaySoundAtIndex(int index)
    {
        if (audioController != null)
        {
            audioController.PlaySoundAtIndex(index);
        }
    }
    
    // Example method to reset the trigger state
    public void ResetAudioTrigger()
    {
        if (audioController != null)
        {
            audioController.ResetTrigger();
        }
    }
    
    void OnGUI()
    {
        if (audioController == null) return;
        
        // Display current status
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Audio Controller Status:");
        GUILayout.Label($"Is Playing: {audioController.IsPlaying}");
        GUILayout.Label($"Current Index: {audioController.CurrentSequenceIndex}");
        GUILayout.Label($"Total Sequences: {audioController.TotalSequences}");
        GUILayout.Label($"Has Triggered: {audioController.HasTriggered}");
        
        GUILayout.Space(10);
        GUILayout.Label("Controls:");
        GUILayout.Label($"{triggerKey} - Start Audio");
        GUILayout.Label($"{stopKey} - Stop Audio");
        GUILayout.Label($"{nextSoundKey} - Next Sound");
        GUILayout.Label($"{previousSoundKey} - Previous Sound");
        
        GUILayout.EndArea();
    }
}
