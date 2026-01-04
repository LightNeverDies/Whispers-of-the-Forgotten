using Interfaces;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering.PostProcessing;

public class PowerButtonController : MonoBehaviour, IInteractableHint
{
    [Header("Interaction Settings")]
    public float interactDistance = 1f;
    public LayerMask interactionLayer;
    [Tooltip("Layer mask for obstructions (walls, etc.) that block raycast. Leave as 'Nothing' to use everything except interactionLayer.")]
    public LayerMask obstructionLayer;
    public Hints hints;
    [Tooltip("The hand texture to display in the center of the screen")]
    public Texture2D handTexture;
    [Tooltip("Scale factor for the hand texture (0.1 = 10% of original size)")]
    public float handTextureScale = 0.05f;

    [Header("Visual & Sound")]
    public Renderer buttonRenderer;
    public Light flickerLight;
    public AudioSource buttonClickSound;

    [Header("Power State")]
    public bool hasPower = true;
    public bool isSpecificTimeEventThatBreaks = false;

    [Header("Colors")]
    public Color powerOnColor = Color.green;
    public Color powerOffColor = Color.red;
    public Color powerBreakColor = Color.black;

    [Header("Flicker Settings")]
    public float flickerSpeed = 2f;
    public float flickerMinIntensity = 1f;
    public float flickerMaxIntensity = 4f;

    [Header("Global Effects")]
    public PostProcessVolume postProcessVolume;
    public Camera mainCamera;

    private bool isPlayerNear = false;
    public bool IsInteractableNear => isPlayerNear;

    private IPowerReactive[] powerReactives;

    void Start()
    {
        // Use FindObjectsOfType with includeInactive = true to find disabled objects (like the orb)
        powerReactives = FindObjectsOfType<MonoBehaviour>(true).OfType<IPowerReactive>().ToArray();
    }

    void Update()
    {
        HandleInteraction();
        UpdateVisuals();
    }

    private void HandleInteraction()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactionLayer))
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
            
            if (hit.collider.CompareTag("ElectricPanel"))
            {
                isPlayerNear = true;

                if (Input.GetKeyDown(KeyCode.E) && isPlayerNear)
                {
                    TogglePower();
                }

                return;
            }
        }

        if (isPlayerNear)
        {
            isPlayerNear = false;
        }
    }

    void OnGUI()
    {
        // Only show hand texture when near power button
        if (handTexture != null && isPlayerNear)
        {
            // Calculate scaled size
            float scaledWidth = handTexture.width * handTextureScale;
            float scaledHeight = handTexture.height * handTextureScale;
            
            // Calculate center position of the screen
            float x = (Screen.width - scaledWidth) * 0.5f;
            float y = (Screen.height - scaledHeight) * 0.5f;
            
            // Draw the hand texture at the center with scaled size
            GUI.DrawTexture(new Rect(x, y, scaledWidth, scaledHeight), handTexture);
        }
    }

    private void UpdateVisuals()
    {
        if (buttonRenderer != null)
        {
            buttonRenderer.material.color = hasPower ? powerOnColor : powerOffColor;

            buttonRenderer.material.SetColor("_EmissionColor", hasPower ? powerOnColor : powerOffColor);
        }

        if (isSpecificTimeEventThatBreaks)
        {
            hasPower = false;
        }

        // Always update flickerLight, even when isSpecificTimeEventThatBreaks is true
        // This allows the light to show red when power is off
        if (flickerLight != null)
        {
            if (hasPower)
            {
                flickerLight.color = powerOnColor;
                flickerLight.intensity = flickerMaxIntensity;
                flickerLight.enabled = true;
            }
            else
            {
                flickerLight.color = powerOffColor;
                float intensity = Mathf.Lerp(flickerMinIntensity, flickerMaxIntensity,
                    (Mathf.Sin(Time.time * flickerSpeed) + 1f) / 2f);
                flickerLight.intensity = intensity;
                flickerLight.enabled = true;
            }
        }
    }

    public void TogglePower()
    {
        // Reset the time event break flag when player manually toggles power
        isSpecificTimeEventThatBreaks = false;
        hasPower = !hasPower;
        ApplyPowerState();
        PlayClick();
    }

    public void SetPower(bool value)
    {
        hasPower = value;
        ApplyPowerState();
        PlayClick();
    }

    public void CutPowerFromEvent()
    {
        hasPower = false;
        isSpecificTimeEventThatBreaks = true;
        ApplyPowerState();
        PlayClick();
    }

    private void ApplyPowerState()
    {
        if (postProcessVolume != null)
        {
            postProcessVolume.weight = hasPower ? 0.5f : 1f;
        }

        foreach (var reactive in powerReactives)
        {
            reactive.SetPower(hasPower);
        }
    }

    private void PlayClick()
    {
        if (buttonClickSound != null)
        {
            buttonClickSound.Play();
        }
    }
}
