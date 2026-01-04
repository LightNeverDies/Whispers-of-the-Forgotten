using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TriggerType
{
    LookAt,         // Player must look at the object (like MirrorHandprintRevealer)
    Proximity,      // Player must be near the object
    Interaction,    // Player must interact with the object (like BookInteractionController)
    Raycast,        // Player must look at and be close enough
    Manual          // Only triggered manually via code
}

[System.Serializable]
public class AudioSequence
{
    [Header("Audio Source")]
    public AudioSource audioSource;
    
    [Header("Subtitle Settings")]
    public bool needsSubtitle = false;
    [TextArea(2, 4)]
    public string subtitleText = "";
    public float subtitleDuration = 5f;
    
    [Header("Post-Audio Subtitle Settings")]
    public bool showSubtitleAfterAudio = false;
    [TextArea(2, 4)]
    public string subtitleAfterAudioText = "";
    public float subtitleAfterAudioDuration = 5f;
    
    [Header("Timing")]
    public float delayBeforeNext = 0.5f;
}

public class ReusableAudioController : MonoBehaviour
{
    [Header("Audio Sequence")]
    public AudioSequence[] audioSequences;
    
    [Header("Trigger Settings")]
    public TriggerType triggerType = TriggerType.LookAt;
    public float triggerDistance = 3f;
    public float lookAngle = 25f;
    public LayerMask interactionLayer = -1;
    public LayerMask obstructionLayer = -1;
    public string playerTag = "Player";
    
    [Header("Camera Reference")]
    public Camera playerCamera;
    
    [Header("Playback Control")]
    public bool playOnStart = false;
    public bool loopSequence = false;
    public bool canRetrigger = true;
    
    [Header("Subtitle Integration")]
    public SubtitleManager subtitleManager;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    private bool isPlaying = false;
    private bool hasTriggered = false;
    private int currentSequenceIndex = 0;
    private Coroutine currentPlaybackCoroutine;
    
    void Start()
    {
        // Auto-find SubtitleManager if not assigned
        if (subtitleManager == null)
        {
            subtitleManager = FindObjectOfType<SubtitleManager>();
        }
        
        // Auto-find player camera if not assigned
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        // Setup audio sources if not assigned
        SetupAudioSources();
        
        if (playOnStart)
        {
            StartAudioSequence();
        }
    }
    
    void SetupAudioSources()
    {
        for (int i = 0; i < audioSequences.Length; i++)
        {
            if (audioSequences[i].audioSource == null)
            {
                // Create AudioSource if not assigned
                GameObject audioObj = new GameObject($"AudioSource_{i}");
                audioObj.transform.SetParent(transform);
                audioSequences[i].audioSource = audioObj.AddComponent<AudioSource>();
            }
            
            // Activate AudioSource
            AudioSource source = audioSequences[i].audioSource;
            source.playOnAwake = false;
            source.gameObject.SetActive(true);
        }
    }
    
    void Update()
    {
        if (triggerType == TriggerType.Manual) return;
        
        CheckForTrigger();
    }
    
    void CheckForTrigger()
    {
        if (isPlaying || (!canRetrigger && hasTriggered)) return;
        
        switch (triggerType)
        {
            case TriggerType.LookAt:
                CheckLookAtTrigger();
                break;
            case TriggerType.Proximity:
                CheckProximityTrigger();
                break;
            case TriggerType.Interaction:
                CheckInteractionTrigger();
                break;
            case TriggerType.Raycast:
                CheckRaycastTrigger();
                break;
        }
    }
    
    void CheckLookAtTrigger()
    {
        if (playerCamera == null) return;
        
        float distance = Vector3.Distance(playerCamera.transform.position, transform.position);
        if (distance > triggerDistance) return;
        
        Vector3 directionToObject = (transform.position - playerCamera.transform.position).normalized;
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToObject);
        
        if (angle < lookAngle)
        {
            // Check for line of sight (no obstructions)
            if (HasLineOfSight())
            {
                TriggerAudioSequence();
            }
        }
    }
    
    void CheckProximityTrigger()
    {
        if (playerCamera == null) return;
        
        float distance = Vector3.Distance(playerCamera.transform.position, transform.position);
        if (distance <= triggerDistance)
        {
            TriggerAudioSequence();
        }
    }
    
    void CheckInteractionTrigger()
    {
        if (playerCamera == null) return;
        
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, triggerDistance, interactionLayer))
        {
            // Check for obstructions between camera and hit point
            Vector3 directionToHit = hit.point - playerCamera.transform.position;
            float distanceToHit = Vector3.Distance(playerCamera.transform.position, hit.point);
            RaycastHit obstructionHit;
            
            // Use obstructionLayer if set, otherwise use everything except interactionLayer
            LayerMask obstructionCheckLayer = obstructionLayer.value != 0 ? obstructionLayer : ~interactionLayer;
            
            if (Physics.Raycast(playerCamera.transform.position, directionToHit.normalized, out obstructionHit, distanceToHit, obstructionCheckLayer))
            {
                // Check if the obstruction is the target object itself
                if (obstructionHit.collider != hit.collider)
                {
                    // There's an obstruction blocking the view
                    return;
                }
            }
            
            if (hit.collider.transform == transform)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    TriggerAudioSequence();
                }
            }
        }
    }
    
    void CheckRaycastTrigger()
    {
        if (playerCamera == null) return;
        
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, triggerDistance, interactionLayer))
        {
            // Check for obstructions between camera and hit point
            Vector3 directionToHit = hit.point - playerCamera.transform.position;
            float distanceToHit = Vector3.Distance(playerCamera.transform.position, hit.point);
            RaycastHit obstructionHit;
            
            // Use obstructionLayer if set, otherwise use everything except interactionLayer
            LayerMask obstructionCheckLayer = obstructionLayer.value != 0 ? obstructionLayer : ~interactionLayer;
            
            if (Physics.Raycast(playerCamera.transform.position, directionToHit.normalized, out obstructionHit, distanceToHit, obstructionCheckLayer))
            {
                // Check if the obstruction is the target object itself
                if (obstructionHit.collider != hit.collider)
                {
                    // There's an obstruction blocking the view
                    return;
                }
            }
            
            if (hit.collider.transform == transform)
            {
                TriggerAudioSequence();
            }
        }
    }
    
    bool HasLineOfSight()
    {
        if (obstructionLayer.value == 0) return true; // No obstruction layer set
        
        Vector3 rayDirection = (transform.position - playerCamera.transform.position).normalized;
        float rayDistance = Vector3.Distance(playerCamera.transform.position, transform.position);
        
        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, rayDirection, out hit, rayDistance, obstructionLayer))
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
        
        // Stop all audio sources
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
    
    // Gizmos for trigger visualization
    void OnDrawGizmosSelected()
    {
        if (triggerType == TriggerType.Manual) return;
        
        Gizmos.color = Color.yellow;
        
        switch (triggerType)
        {
            case TriggerType.LookAt:
            case TriggerType.Proximity:
                Gizmos.DrawWireSphere(transform.position, triggerDistance);
                break;
            case TriggerType.Raycast:
            case TriggerType.Interaction:
                // Show interaction range
                if (playerCamera != null)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(transform.position, triggerDistance);
                }
                break;
        }
        
        // Show look angle for LookAt trigger
        if (triggerType == TriggerType.LookAt && playerCamera != null)
        {
            Gizmos.color = Color.red;
            Vector3 forward = playerCamera.transform.forward;
            Vector3 left = Quaternion.AngleAxis(-lookAngle, Vector3.up) * forward;
            Vector3 right = Quaternion.AngleAxis(lookAngle, Vector3.up) * forward;
            
            Gizmos.DrawRay(transform.position, left * triggerDistance);
            Gizmos.DrawRay(transform.position, right * triggerDistance);
        }
    }
    
    // Public properties for external access
    public bool IsPlaying => isPlaying;
    public int CurrentSequenceIndex => currentSequenceIndex;
    public int TotalSequences => audioSequences.Length;
    public bool HasTriggered => hasTriggered;
}
