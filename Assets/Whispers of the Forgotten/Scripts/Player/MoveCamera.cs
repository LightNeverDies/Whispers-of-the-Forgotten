using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [Header("Camera Settings")]
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    [Header("Zoom (FOV)")]
    [Tooltip("If enabled, allows changing Camera.fieldOfView for a zoom effect (perspective camera).")]
    public bool enableZoom = true;
    [Tooltip("If false, zoom input is ignored (useful for tutorial gating).")]
    public bool canUseZoom = true;
    [Tooltip("Only apply zoom while the cursor is locked (prevents zooming while interacting with UI).")]
    public bool zoomOnlyWhenCursorLocked = true;

    [Tooltip("Hold this mouse button to zoom (e.g. right click). Use -1 to disable.")]
    public int holdZoomMouseButton = 1; // 1 = RMB, 0 = LMB, 2 = MMB. Use -1 to disable.
    [Tooltip("Field of view while holding zoom button. Lower = more zoom.")]
    public float holdZoomFov = 35f;

    [Tooltip("Minimum allowed FOV (most zoomed-in).")]
    public float minFov = 25f;
    [Tooltip("Maximum allowed FOV (most zoomed-out).")]
    public float maxFov = 75f;
    [Tooltip("How quickly FOV interpolates to the target. 0 = instant.")]
    public float zoomSmoothSpeed = 12f;
    
    [Header("Input Limits")]
    [Tooltip("If true, caps how fast the camera can rotate to avoid huge jumps on low FPS / spikes.")]
    public bool limitLookSpeed = true;
    [Tooltip("Maximum degrees per second for Mouse X/Y.")]
    public float maxDegreesPerSecond = 720f;

    [Header("Smoothing")]
    public float smoothing = 10f;
    public bool useSmoothing = false; // Disabled by default for responsive feel
    
    [Header("Constraints")]
    public bool isOnBed = false;
    public float minYRotation = -90f;
    public float maxYRotation = 90f;
    public float minXRotation = -90f;
    public float maxXRotation = 30f;

    // Cached components
    private Transform cameraTransform;
    private Transform playerBodyTransform;
    private Camera cam;
    
    // Rotation variables
    private float xRotation = 0f;
    private float yRotation = 0f;

    // Zoom variables
    private float baseFov;
    private float targetFov;
    
    // Smoothing variables
    private Vector3 currentRotation;
    private Vector3 targetRotation;
    
    // Performance optimization
    private bool isInitialized = false;

    void Start()
    {
        InitializeCamera();
    }

    void InitializeCamera()
    {
        if (isInitialized) return;
        
        // Cache transforms for better performance
        cameraTransform = transform;
        playerBodyTransform = playerBody;
        cam = GetComponent<Camera>();
        
        // Set cursor state
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Initialize rotation
        if (playerBodyTransform != null)
        {
            float rawY = playerBodyTransform.eulerAngles.y;
            yRotation = (rawY > 180f) ? rawY - 360f : rawY;
        }
        
        // Initialize smoothing
        currentRotation = cameraTransform.localEulerAngles;
        targetRotation = currentRotation;

        // Initialize zoom
        if (cam != null)
        {
            baseFov = cam.fieldOfView;
            baseFov = Mathf.Clamp(baseFov, minFov, maxFov);
            targetFov = baseFov;
            cam.fieldOfView = baseFov;
        }
        
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) InitializeCamera();
        
        HandleMouseInput();
        ApplyRotation();
        HandleZoom();
    }

    void HandleMouseInput()
    {
        // Get mouse input with frame rate independence
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Optional: cap rotation speed (degrees per frame) based on maxDegreesPerSecond.
        // This avoids "snapping" when deltaTime spikes (e.g., editor hiccups).
        if (limitLookSpeed && maxDegreesPerSecond > 0f)
        {
            float maxDelta = maxDegreesPerSecond * Time.deltaTime;
            mouseX = Mathf.Clamp(mouseX, -maxDelta, maxDelta);
            mouseY = Mathf.Clamp(mouseY, -maxDelta, maxDelta);
        }

        if (isOnBed)
        {
            // Bed mode - restricted movement
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minXRotation, maxXRotation);
            
            yRotation += mouseX;
            yRotation = Mathf.Clamp(yRotation, minYRotation, maxYRotation);
            
            targetRotation = new Vector3(xRotation, 0f, 0f);
            
            if (playerBodyTransform != null)
            {
                playerBodyTransform.rotation = Quaternion.Euler(0f, yRotation, 0f);
            }
        }
        else
        {
            // Normal mode
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            
            targetRotation = new Vector3(xRotation, 0f, 0f);
            
            if (playerBodyTransform != null)
            {
                playerBodyTransform.Rotate(Vector3.up * mouseX);
            }
        }
    }

    void ApplyRotation()
    {
        if (useSmoothing)
        {
            // Smooth rotation for better feel
            currentRotation = Vector3.Lerp(currentRotation, targetRotation, Time.deltaTime * smoothing);
            cameraTransform.localRotation = Quaternion.Euler(currentRotation);
        }
        else
        {
            // Direct rotation for maximum responsiveness
            cameraTransform.localRotation = Quaternion.Euler(targetRotation);
        }
    }

    void HandleZoom()
    {
        if (!enableZoom || !canUseZoom || cam == null) return;
        if (zoomOnlyWhenCursorLocked && Cursor.lockState != CursorLockMode.Locked) return;

        // Hold-to-zoom overrides target while held.
        bool holdZoomActive = holdZoomMouseButton >= 0 && Input.GetMouseButton(holdZoomMouseButton);
        float desired = holdZoomActive ? Mathf.Clamp(holdZoomFov, minFov, maxFov) : baseFov;

        if (zoomSmoothSpeed <= 0f)
        {
            targetFov = desired;
            cam.fieldOfView = targetFov;
            return;
        }

        targetFov = Mathf.Lerp(cam.fieldOfView, desired, Time.deltaTime * zoomSmoothSpeed);
        cam.fieldOfView = targetFov;
    }

    // Public method to disable/enable camera movement
    public void SetCameraEnabled(bool enabled)
    {
        this.enabled = enabled;
    }

    // Public method to reset camera rotation
    public void ResetCameraRotation()
    {
        xRotation = 0f;
        yRotation = 0f;
        currentRotation = Vector3.zero;
        targetRotation = Vector3.zero;
        cameraTransform.localRotation = Quaternion.identity;
    }
}
