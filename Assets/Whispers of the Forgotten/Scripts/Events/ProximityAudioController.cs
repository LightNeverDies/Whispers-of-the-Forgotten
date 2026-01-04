using System.Collections;
using UnityEngine;

/// <summary>
/// Audio controller that triggers based on player proximity and line of sight
/// Uses the same sophisticated detection system as MirrorHandprintRevealer
/// Perfect for ambient sounds, whispers, and environmental audio
/// </summary>
public class ProximityAudioController : MonoBehaviour
{
    [Header("Audio Sequence")]
    public AudioSequence[] audioSequences;
    
    [Header("Proximity Settings")]
    public Transform playerCamera;
    public float triggerDistance = 3f;
    public float lookAngle = 25f;
    public LayerMask obstructionLayer = -1;
    
    [Header("Subtitle Integration")]
    public SubtitleManager subtitleManager;
    
    [Header("Playback Control")]
    public bool loopSequence = false;
    public bool canRetrigger = true;
    public float retriggerCooldown = 5f;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    public bool showGizmos = true;
    
    private bool isPlaying = false;
    private bool hasTriggered = false;
    private int currentSequenceIndex = 0;
    private Coroutine currentPlaybackCoroutine;
    private float lastTriggerTime = 0f;
    
    void Start()
    {
        // Auto-find components if not assigned
        if (subtitleManager == null)
            subtitleManager = FindObjectOfType<SubtitleManager>();
        
        if (playerCamera == null)
            playerCamera = Camera.main.transform;
        
        SetupAudioSources();
    }
    
    void Update()
    {
        if (isPlaying || (!canRetrigger && hasTriggered)) return;
        
        // Check cooldown
        if (Time.time - lastTriggerTime < retriggerCooldown) return;
        
        CheckForProximityTrigger();
    }
    
    void CheckForProximityTrigger()
    {
        if (playerCamera == null) return;
        
        // Check distance
        float distance = Vector3.Distance(playerCamera.position, transform.position);
        if (distance > triggerDistance) return;
        
        // Check if player is looking at the object
        Vector3 directionToObject = (transform.position - playerCamera.position).normalized;
        float angle = Vector3.Angle(playerCamera.forward, directionToObject);
        
        if (angle < lookAngle)
        {
            // Check for line of sight (no obstructions)
            if (HasLineOfSight())
            {
                TriggerAudioSequence();
            }
        }
    }
    
    bool HasLineOfSight()
    {
        if (obstructionLayer.value == 0) return true; // No obstruction layer set
        
        Vector3 rayDirection = (transform.position - playerCamera.position).normalized;
        float rayDistance = Vector3.Distance(playerCamera.position, transform.position);
        
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.position, rayDirection, out hit, rayDistance, obstructionLayer))
        {
            // If we hit an obstruction, check if it's the target object itself
            return hit.collider.transform == transform;
        }
        
        return true; // No obstruction found
    }
    
    void TriggerAudioSequence()
    {
        if (canRetrigger || !hasTriggered)
        {
            StartAudioSequence();
            hasTriggered = true;
            lastTriggerTime = Time.time;
        }
    }
    
    void SetupAudioSources()
    {
        for (int i = 0; i < audioSequences.Length; i++)
        {
            if (audioSequences[i].audioSource == null)
            {
                GameObject audioObj = new GameObject($"AudioSource_{i}");
                audioObj.transform.SetParent(transform);
                audioSequences[i].audioSource = audioObj.AddComponent<AudioSource>();
            }
            
            AudioSource source = audioSequences[i].audioSource;
            source.playOnAwake = false;
            source.gameObject.SetActive(true); // Activate audio source
        }
    }
    
    public void StartAudioSequence()
    {
        if (isPlaying)
        {
            return;
        }
        
        if (audioSequences == null || audioSequences.Length == 0)
        {
            return;
        }
        
        if (currentPlaybackCoroutine != null)
        {
            StopCoroutine(currentPlaybackCoroutine);
        }
        
        currentPlaybackCoroutine = StartCoroutine(PlayAudioSequence());
    }
    
    public void StopAudioSequence()
    {
        if (currentPlaybackCoroutine != null)
        {
            StopCoroutine(currentPlaybackCoroutine);
            currentPlaybackCoroutine = null;
        }
        
        foreach (var sequence in audioSequences)
        {
            if (sequence.audioSource != null && sequence.audioSource.isPlaying)
            {
                sequence.audioSource.Stop();
            }
        }
        
        isPlaying = false;
        currentSequenceIndex = 0;
    }
    
    public void ResetTrigger()
    {
        hasTriggered = false;
        StopAudioSequence();
    }
    
    private IEnumerator PlayAudioSequence()
    {
        isPlaying = true;
        
        do
        {
            for (int i = 0; i < audioSequences.Length; i++)
            {
                currentSequenceIndex = i;
                AudioSequence currentSequence = audioSequences[i];
                
                if (currentSequence.audioSource == null || currentSequence.audioSource.clip == null)
                {
                    continue;
                }
                
                // Activate AudioSource if deactivated
                if (!currentSequence.audioSource.enabled)
                {
                    currentSequence.audioSource.enabled = true;
                }
                if (!currentSequence.audioSource.gameObject.activeInHierarchy)
                {
                    currentSequence.audioSource.gameObject.SetActive(true);
                }
                
                // Play the audio
                currentSequence.audioSource.Play();
                
                // Show subtitle if needed
                if (currentSequence.needsSubtitle && !string.IsNullOrEmpty(currentSequence.subtitleText) && subtitleManager != null)
                {
                    subtitleManager.ShowSubtitle(currentSequence.subtitleText, currentSequence.subtitleDuration);
                }
                
                // Wait for audio to finish
                yield return new WaitUntil(() => !currentSequence.audioSource.isPlaying);
                
                // Wait additional delay before next sound
                if (currentSequence.delayBeforeNext > 0)
                {
                    yield return new WaitForSeconds(currentSequence.delayBeforeNext);
                }
            }
            
            if (loopSequence)
            {
            }
            
        } while (loopSequence);
        
        isPlaying = false;
        currentSequenceIndex = 0;
    }
    
    // Gizmos for visualization
    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        // Draw trigger distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
        
        // Draw look angle
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;
            Vector3 forward = playerCamera.forward;
            Vector3 left = Quaternion.AngleAxis(-lookAngle, Vector3.up) * forward;
            Vector3 right = Quaternion.AngleAxis(lookAngle, Vector3.up) * forward;
            
            Gizmos.DrawRay(transform.position, left * triggerDistance);
            Gizmos.DrawRay(transform.position, right * triggerDistance);
        }
    }
    
    // Public methods for external control
    public void PlayNextSound()
    {
        if (currentSequenceIndex < audioSequences.Length - 1)
        {
            currentSequenceIndex++;
            StartCoroutine(PlaySingleSound(currentSequenceIndex));
        }
    }
    
    public void PlayPreviousSound()
    {
        if (currentSequenceIndex > 0)
        {
            currentSequenceIndex--;
            StartCoroutine(PlaySingleSound(currentSequenceIndex));
        }
    }
    
    public void PlaySoundAtIndex(int index)
    {
        if (index >= 0 && index < audioSequences.Length)
        {
            currentSequenceIndex = index;
            StartCoroutine(PlaySingleSound(index));
        }
    }
    
    private IEnumerator PlaySingleSound(int index)
    {
        AudioSequence sequence = audioSequences[index];
        
        if (sequence.audioSource != null && sequence.audioSource.clip != null)
        {
            // Activate AudioSource if deactivated
            if (!sequence.audioSource.enabled)
            {
                sequence.audioSource.enabled = true;
            }
            if (!sequence.audioSource.gameObject.activeInHierarchy)
            {
                sequence.audioSource.gameObject.SetActive(true);
            }
            
            sequence.audioSource.Play();
            
            if (sequence.needsSubtitle && !string.IsNullOrEmpty(sequence.subtitleText) && subtitleManager != null)
            {
                subtitleManager.ShowSubtitle(sequence.subtitleText, sequence.subtitleDuration);
            }
            
            yield return new WaitUntil(() => !sequence.audioSource.isPlaying);
        }
    }
    
    // Public properties for external access
    public bool IsPlaying => isPlaying;
    public int CurrentSequenceIndex => currentSequenceIndex;
    public int TotalSequences => audioSequences.Length;
    public bool HasTriggered => hasTriggered;
}
