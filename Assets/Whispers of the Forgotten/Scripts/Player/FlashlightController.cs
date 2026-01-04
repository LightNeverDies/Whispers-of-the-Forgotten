using UnityEngine;
using UnityEngine.UI;

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight Components")]
    public Light flashlight;
    public AudioSource flashlightClick;
    
    [Header("Movement Settings")]
    public float rotationSpeed = 4f;
    public float fadeSpeed = 15f;
    
    [Header("Light Settings")]
    public Color normalColor = Color.white;
    public Color uvColor = new Color(0.4f, 0.1f, 0.8f);
    public float whiteIntensity = 5f;
    public float uvIntensity = 4f;
    
    [Header("State")]
    public bool isInLockView = false;
    public bool canUseFlashlight = false;
    public bool isEventGoing = false;
    
    [Header("UI")]
    public Image flashlightIconOffImage;
    public Image flashlightIconWhiteImage;
    public Image flashlightIconUVImage;
    public GameObject player;

    public enum FlashlightState { Off, White, UV }
    public FlashlightState flashlightState = FlashlightState.Off;
    public bool IsUVLightOn => flashlightState == FlashlightState.UV;

    // Cached components for performance
    private Transform flashlightTransform;
    private Transform cameraTransform;
    private PlayerMovement playerMovement;
    
    // Performance variables
    private float targetIntensity = 0f;
    private float flickerTimer = 0f;
    private float pulseTimer = 0f;
    
    // State tracking
    private FlashlightState lastFlashlightState = FlashlightState.Off;

    void Start()
    {
        InitializeComponents();
    }

    void InitializeComponents()
    {
        // Cache transforms for better performance
        if (flashlight != null)
        {
            flashlightTransform = flashlight.transform;
        }
        
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
        }
        
        // Initialize flashlight state
        if (flashlight != null)
        {
            flashlight.enabled = false;
            flashlight.intensity = 0f;
        }
    }

    void Update()
    {
        if (!canUseFlashlight)
            return;

        // Check if player input is enabled
        if (playerMovement != null && !playerMovement.inputEnabled) 
            return;
            
        if (isInLockView) 
            return;

        HandleInput();
        UpdateFlashlightPosition();
        UpdateFlashlightIntensity();
        UpdateUI();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlashlight();
        }

        if (Input.GetKeyDown(KeyCode.R) && flashlightState != FlashlightState.Off)
        {
            SwitchFlashlightMode();
        }
    }

    void ToggleFlashlight()
    {
        PlayClickSound();

        if (flashlightState == FlashlightState.Off)
        {
            flashlightState = FlashlightState.White;
            if (flashlight != null)
            {
                flashlight.enabled = true;
                flashlight.color = normalColor;
                targetIntensity = whiteIntensity;
            }
        }
        else
        {
            flashlightState = FlashlightState.Off;
            targetIntensity = 0f;
        }
    }

    void SwitchFlashlightMode()
    {
        PlayClickSound();

        if (flashlightState == FlashlightState.White)
        {
            flashlightState = FlashlightState.UV;
            if (flashlight != null)
            {
                flashlight.color = uvColor;
                targetIntensity = uvIntensity;
            }
        }
        else if (flashlightState == FlashlightState.UV)
        {
            flashlightState = FlashlightState.White;
            if (flashlight != null)
            {
                flashlight.color = normalColor;
                targetIntensity = whiteIntensity;
            }
        }
    }

    void PlayClickSound()
    {
        if (flashlightClick != null)
        {
            flashlightClick.enabled = true;
            flashlightClick.Play();
        }
    }

    void UpdateFlashlightPosition()
    {
        if (flashlight != null && flashlight.enabled && cameraTransform != null)
        {
            // Update position and rotation more efficiently
            flashlightTransform.position = cameraTransform.position;
            
            // Use Slerp for smoother rotation
            flashlightTransform.rotation = Quaternion.Slerp(
                flashlightTransform.rotation, 
                cameraTransform.rotation, 
                Time.deltaTime * rotationSpeed
            );
        }
    }

    void UpdateFlashlightIntensity()
    {
        if (flashlight == null) return;

        if (isEventGoing && flashlightState != FlashlightState.Off)
        {
            // Event flickering
            flickerTimer -= Time.deltaTime;
            if (flickerTimer <= 0f)
            {
                flashlight.intensity = targetIntensity * Random.Range(0.3f, 4f);
                flickerTimer = Random.Range(0.02f, 0.15f);
            }
        }
        else
        {
            // Normal pulsing
            pulseTimer += Time.deltaTime * 2f;
            float pulse = 1f + Mathf.Sin(pulseTimer * 2f) * 0.02f;
            float finalTarget = flashlightState == FlashlightState.Off ? 0f : targetIntensity * pulse;
            
            flashlight.intensity = Mathf.Lerp(flashlight.intensity, finalTarget, Time.deltaTime * fadeSpeed);

            // Disable flashlight when intensity is very low
            if (flashlightState == FlashlightState.Off && flashlight.intensity < 0.05f)
            {
                flashlight.enabled = false;
            }
        }
    }

    void UpdateUI()
    {
        // Only update UI when state changes
        if (flashlightState != lastFlashlightState)
        {
            if (flashlightIconOffImage != null)
                flashlightIconOffImage.enabled = (flashlightState == FlashlightState.Off);
            if (flashlightIconWhiteImage != null)
                flashlightIconWhiteImage.enabled = (flashlightState == FlashlightState.White);
            if (flashlightIconUVImage != null)
                flashlightIconUVImage.enabled = (flashlightState == FlashlightState.UV);
                
            lastFlashlightState = flashlightState;
        }
    }

    // Public methods for external control
    public void SetEventState(bool eventState)
    {
        isEventGoing = eventState;
    }

    public void SetFlashlightEnabled(bool enabled)
    {
        canUseFlashlight = enabled;
        if (!enabled && flashlight != null)
        {
            flashlight.enabled = false;
            flashlightState = FlashlightState.Off;
        }
    }

    public void ForceUpdateFlashlight()
    {
        // Removed the update frequency limiting
    }
}
