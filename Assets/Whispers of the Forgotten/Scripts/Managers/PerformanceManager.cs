using UnityEngine;
using System.Collections.Generic;

public class PerformanceManager : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float raycastDistance = 2f;
    public LayerMask interactionLayerMask;
    public LayerMask itemLayerMask;
    public LayerMask teleportLayerMask;
    
    [Header("Performance Settings")]
    public float raycastUpdateInterval = 0.1f; // Update raycast every 100ms instead of every frame
    public int maxRaycastsPerFrame = 3;
    
    // Singleton pattern
    public static PerformanceManager Instance { get; private set; }
    
    // Cached components
    private Camera mainCamera;
    private Transform cameraTransform;
    
    // Raycast results cache
    private RaycastHit[] raycastHits = new RaycastHit[10];
    private Dictionary<LayerMask, RaycastResult> cachedResults = new Dictionary<LayerMask, RaycastResult>();
    
    // Performance tracking
    private float lastRaycastTime;
    private int currentRaycastCount;
    
    [System.Serializable]
    public class RaycastResult
    {
        public bool hit;
        public RaycastHit hitInfo;
        public GameObject hitObject;
        public float distance;
        public float lastUpdateTime;
    }
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Cache camera
        mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraTransform = mainCamera.transform;
        }
        
        // Initialize layer masks if not set
        if (interactionLayerMask == 0)
            interactionLayerMask = LayerMask.GetMask("Default", "Interactable");
        if (itemLayerMask == 0)
            itemLayerMask = LayerMask.GetMask("Pickable");
        if (teleportLayerMask == 0)
            teleportLayerMask = LayerMask.GetMask("Teleport");
    }
    
    void Update()
    {
        // Reset raycast count each frame
        if (Time.frameCount != lastRaycastTime)
        {
            currentRaycastCount = 0;
            lastRaycastTime = Time.frameCount;
        }
        
        // Update raycast results periodically
        if (Time.time - lastRaycastTime > raycastUpdateInterval)
        {
            UpdateRaycastResults();
        }
    }
    
    void UpdateRaycastResults()
    {
        if (mainCamera == null || cameraTransform == null) return;
        
        // Perform raycast from camera center
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        
        // Update interaction layer results
        UpdateLayerResults(ray, interactionLayerMask);
        
        // Update item layer results
        UpdateLayerResults(ray, itemLayerMask);
        
        // Update teleport layer results
        UpdateLayerResults(ray, teleportLayerMask);
    }
    
    void UpdateLayerResults(Ray ray, LayerMask layerMask)
    {
        int hitCount = Physics.RaycastNonAlloc(ray, raycastHits, raycastDistance, layerMask);
        
        RaycastResult result = new RaycastResult();
        result.hit = hitCount > 0;
        result.lastUpdateTime = Time.time;
        
        if (result.hit)
        {
            // Find closest hit
            float closestDistance = float.MaxValue;
            RaycastHit closestHit = raycastHits[0];
            
            for (int i = 0; i < hitCount; i++)
            {
                if (raycastHits[i].distance < closestDistance)
                {
                    closestDistance = raycastHits[i].distance;
                    closestHit = raycastHits[i];
                }
            }
            
            result.hitInfo = closestHit;
            result.hitObject = closestHit.collider.gameObject;
            result.distance = closestDistance;
        }
        
        cachedResults[layerMask] = result;
    }
    
    // Public methods for other scripts to get raycast results
    public RaycastResult GetInteractionResult()
    {
        return GetCachedResult(interactionLayerMask);
    }
    
    public RaycastResult GetItemResult()
    {
        return GetCachedResult(itemLayerMask);
    }
    
    public RaycastResult GetTeleportResult()
    {
        return GetCachedResult(teleportLayerMask);
    }
    
    public RaycastResult GetCachedResult(LayerMask layerMask)
    {
        if (cachedResults.ContainsKey(layerMask))
        {
            return cachedResults[layerMask];
        }
        return new RaycastResult { hit = false };
    }
    
    // Method to check if we can perform additional raycasts this frame
    public bool CanPerformRaycast()
    {
        return currentRaycastCount < maxRaycastsPerFrame;
    }
    
    // Method to register a raycast (for tracking)
    public void RegisterRaycast()
    {
        currentRaycastCount++;
    }
    
    // Method to force update raycast results (for immediate needs)
    public void ForceUpdateRaycasts()
    {
        UpdateRaycastResults();
    }
    
    // Debug method to show raycast performance
    void OnGUI()
    {
        if (Debug.isDebugBuild)
        {
            // GUI.Label(new Rect(10, 10, 300, 20), $"Raycasts this frame: {currentRaycastCount}/{maxRaycastsPerFrame}");
            // GUI.Label(new Rect(10, 30, 300, 20), $"Cached results: {cachedResults.Count}");
        }
    }
} 