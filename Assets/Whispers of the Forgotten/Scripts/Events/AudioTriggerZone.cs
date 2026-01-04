using UnityEngine;

[RequireComponent(typeof(Collider))]
public class AudioTriggerZone : MonoBehaviour
{
    [Header("Audio Controller")]
    public ReusableAudioController audioController;
    
    [Header("Trigger Settings")]
    public bool triggerOnEnter = true;
    public bool triggerOnExit = false;
    public bool triggerOnStay = false;
    public float stayTriggerInterval = 1f;
    
    [Header("Player Detection")]
    public string playerTag = "Player";
    public LayerMask playerLayerMask = -1;
    
    [Header("Visual Settings")]
    public bool showTriggerZone = true;
    public Color triggerZoneColor = Color.yellow;
    
    private float lastStayTrigger = 0f;
    
    void Start()
    {
        // Auto-find audio controller if not assigned
        if (audioController == null)
        {
            audioController = GetComponent<ReusableAudioController>();
        }
        
        // Ensure collider is set as trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (triggerOnEnter && IsPlayer(other))
        {
            TriggerAudio();
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (triggerOnExit && IsPlayer(other))
        {
            TriggerAudio();
        }
    }
    
    void OnTriggerStay(Collider other)
    {
        if (triggerOnStay && IsPlayer(other))
        {
            if (Time.time - lastStayTrigger >= stayTriggerInterval)
            {
                TriggerAudio();
                lastStayTrigger = Time.time;
            }
        }
    }
    
    private bool IsPlayer(Collider other)
    {
        return other.CompareTag(playerTag) || 
               (playerLayerMask.value & (1 << other.gameObject.layer)) != 0;
    }
    
    private void TriggerAudio()
    {
        if (audioController != null)
        {
            audioController.StartAudioSequence();
        }
    }
    
    void OnDrawGizmos()
    {
        if (showTriggerZone)
        {
            Gizmos.color = triggerZoneColor;
            
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                if (col is BoxCollider)
                {
                    BoxCollider boxCol = col as BoxCollider;
                    Gizmos.DrawWireCube(transform.position + boxCol.center, boxCol.size);
                }
                else if (col is SphereCollider)
                {
                    SphereCollider sphereCol = col as SphereCollider;
                    Gizmos.DrawWireSphere(transform.position + sphereCol.center, sphereCol.radius);
                }
                else if (col is CapsuleCollider)
                {
                    CapsuleCollider capsuleCol = col as CapsuleCollider;
                    Gizmos.DrawWireCube(transform.position + capsuleCol.center, 
                        new Vector3(capsuleCol.radius * 2, capsuleCol.height, capsuleCol.radius * 2));
                }
            }
        }
    }
}
