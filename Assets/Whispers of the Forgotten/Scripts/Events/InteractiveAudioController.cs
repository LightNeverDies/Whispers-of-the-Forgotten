using System.Collections;
using UnityEngine;
using Interfaces;

public class InteractiveAudioController : MonoBehaviour, IInteractableHint
{
    [Header("Audio Sequence")]
    public AudioSequence[] audioSequences;
    
    [Header("Interaction Settings")]
    public float interactDistance = 2f;
    public LayerMask interactionLayer = -1;
    [Tooltip("Layer mask for obstructions (walls, etc.) that block raycast. Leave as 'Nothing' to use everything except interactionLayer.")]
    public LayerMask obstructionLayer;
    public Hints hints;
    public Camera mainCamera;
    
    [Header("Subtitle Integration")]
    public SubtitleManager subtitleManager;
    
    [Header("Playback Control")]
    public bool loopSequence = false;
    public bool canRetrigger = true;
    
    [Header("Debug")]
    public bool showDebugInfo = false;
    
    private bool isPlaying = false;
    private bool hasTriggered = false;
    private bool isPlayerNear = false;
    private int currentSequenceIndex = 0;
    private Coroutine currentPlaybackCoroutine;
    
    public bool IsInteractableNear => isPlayerNear;
    
    void Start()
    {
        // Auto-find components if not assigned
        if (subtitleManager == null)
            subtitleManager = FindObjectOfType<SubtitleManager>();
        
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (hints == null)
            hints = FindObjectOfType<Hints>();
        
        SetupAudioSources();
    }
    
    void Update()
    {
        HandleInteraction();
    }
    
    void HandleInteraction()
    {
        if (isPlaying) return;
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactDistance, interactionLayer))
        {
            // Check for obstructions between camera and hit point
            Vector3 directionToHit = hit.point - mainCamera.transform.position;
            float distanceToHit = Vector3.Distance(mainCamera.transform.position, hit.point);
            RaycastHit obstructionHit;
            
            // Use obstructionLayer if set, otherwise use everything except interactionLayer
            LayerMask obstructionCheckLayer = obstructionLayer.value != 0 ? obstructionLayer : ~interactionLayer;
            
            if (Physics.Raycast(mainCamera.transform.position, directionToHit.normalized, out obstructionHit, distanceToHit, obstructionCheckLayer))
            {
                // Check if the obstruction is the target object itself
                if (obstructionHit.collider != hit.collider)
                {
                    // There's an obstruction blocking the view
                    if (isPlayerNear)
                    {
                        isPlayerNear = false;
                    }
                    return;
                }
            }
            
                if (hit.collider.transform == transform)
                {
                    isPlayerNear = true;
                    
                    // Auto-trigger when looking at object (like BookInteractionController)
                    if (canRetrigger || !hasTriggered)
                    {
                        StartAudioSequence();
                        hasTriggered = true;
                    }
                    
                    return;
                }
        }
        
        if (isPlayerNear)
        {
            isPlayerNear = false;
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
        
        // Check if there are valid audio sources
        bool hasValidAudio = false;
        foreach (var sequence in audioSequences)
        {
            if (sequence.audioSource != null && sequence.audioSource.clip != null)
            {
                hasValidAudio = true;
                break;
            }
        }
        
        if (!hasValidAudio)
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
                
                // Show subtitle after audio is done if configured (each line sequentially)
                if (currentSequence.showSubtitleAfterAudio && !string.IsNullOrEmpty(currentSequence.subtitleAfterAudioText) && subtitleManager != null)
                {
                    yield return StartCoroutine(ShowSequentialSubtitles(currentSequence.subtitleAfterAudioText, currentSequence.subtitleAfterAudioDuration));
                }
                
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
            
            // Show subtitle after audio is done if configured (each line sequentially)
            if (sequence.showSubtitleAfterAudio && !string.IsNullOrEmpty(sequence.subtitleAfterAudioText) && subtitleManager != null)
            {
                yield return StartCoroutine(ShowSequentialSubtitles(sequence.subtitleAfterAudioText, sequence.subtitleAfterAudioDuration));
            }
        }
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
    
    // Public properties for external access
    public bool IsPlaying => isPlaying;
    public int CurrentSequenceIndex => currentSequenceIndex;
    public int TotalSequences => audioSequences.Length;
    public bool HasTriggered => hasTriggered;
}
