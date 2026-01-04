using Interfaces;
using System.Collections;
using UnityEngine;

public class FridgeController : MonoBehaviour, IInteractableHint
{
    [Header("Fridge Settings")]
    public float smooth = 2.0f;
    public float fridgeOpenAngle = 110.0f;
    public float rotationTolerance = 1.0f;
    public float interactDistance = 1.0f;
    public LayerMask interactionLayer;
    [Tooltip("Layer mask for obstructions (walls, etc.) that block raycast. Leave as 'Nothing' to use everything except interactionLayer.")]
    public LayerMask obstructionLayer;
    [Tooltip("Axis to rotate around (typically Y for side opening, Z for top opening)")]
    public Vector3 rotationAxis = Vector3.up;
    
    private Quaternion defaultRot;
    private Quaternion openRot;
    
    [Header("Interaction")]
    public Hints hints;
    [Tooltip("The hand texture to display in the center of the screen")]
    public Texture2D handTexture;
    [Tooltip("Scale factor for the hand texture (0.1 = 10% of original size)")]
    public float handTextureScale = 0.05f;
    public AudioSource fridgeOpenSound;
    public AudioSource fridgeCloseSound;
    
    public bool IsInteractableNear => isNearFridge;
    
    private bool fridgeOpened = false;
    private bool isMoving = false;
    public bool isNearFridge = false;

    void Start()
    {
        defaultRot = transform.rotation;
        // Calculate open rotation based on the rotation axis
        Vector3 rotationEuler = defaultRot.eulerAngles;
        if (rotationAxis == Vector3.up)
        {
            rotationEuler.y += fridgeOpenAngle;
        }
        else if (rotationAxis == Vector3.right || rotationAxis == Vector3.left)
        {
            rotationEuler.x += fridgeOpenAngle;
        }
        else if (rotationAxis == Vector3.forward || rotationAxis == Vector3.back)
        {
            rotationEuler.z += fridgeOpenAngle;
        }
        openRot = Quaternion.Euler(rotationEuler);
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactionLayer))
        {
            // Check for obstructions between camera and hit point
            Vector3 directionToHit = hit.point - Camera.main.transform.position;
            float distanceToHit = Vector3.Distance(Camera.main.transform.position, hit.point);
            RaycastHit obstructionHit;
            
            // Use obstructionLayer if set, otherwise use everything except interactionLayer
            LayerMask obstructionCheckLayer = obstructionLayer.value != 0 ? obstructionLayer : ~interactionLayer;
            
            if (Physics.Raycast(Camera.main.transform.position, directionToHit.normalized, out obstructionHit, distanceToHit, obstructionCheckLayer))
            {
                // Check if the obstruction is the target object itself
                if (obstructionHit.collider != hit.collider)
                {
                    // There's an obstruction blocking the view
                    if (isNearFridge)
                    {
                        isNearFridge = false;
                    }
                    return;
                }
            }
            
            if (hit.collider.CompareTag("Fridge") || hit.collider.transform == this.transform || hit.collider.transform.IsChildOf(this.transform))
            {
                FridgeController fridgeController = hit.collider.GetComponent<FridgeController>();
                if (fridgeController == null)
                {
                    // Check if parent has the controller
                    fridgeController = hit.collider.GetComponentInParent<FridgeController>();
                }
                
                if (fridgeController != null && !isMoving)
                {
                    fridgeController.HandleInteraction();
                    isNearFridge = true;
                }
            }
        }
        else if (isNearFridge)
        {
            isNearFridge = false;
        }
    }

    public void HandleInteraction()
    {

        // Handle interaction
        if (Input.GetKeyDown(KeyCode.E) && !isMoving)
        {
            ToggleFridge();
        }
    }

    public void ToggleFridge()
    {
        if (fridgeOpened)
        {
            CloseFridge();
        }
        else
        {
            OpenFridge();
        }
    }

    public void OpenFridge()
    {
        if (isMoving || fridgeOpened) return;
        
        StartCoroutine(OpenFridgeCoroutine());
    }

    public void CloseFridge()
    {
        if (isMoving || !fridgeOpened) return;
        
        StartCoroutine(CloseFridgeCoroutine());
    }

    private IEnumerator OpenFridgeCoroutine()
    {
        isMoving = true;
        fridgeOpened = true;

        if (fridgeOpenSound != null)
        {
            fridgeOpenSound.enabled = true;
            fridgeOpenSound.Play();
        }

        while (Quaternion.Angle(transform.rotation, openRot) > rotationTolerance)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, openRot, Time.deltaTime * smooth);
            yield return null;
        }

        transform.rotation = openRot;
        isMoving = false;
    }

    private IEnumerator CloseFridgeCoroutine()
    {
        isMoving = true;
        fridgeOpened = false;

        if (fridgeCloseSound != null)
        {
            fridgeCloseSound.enabled = true;
            fridgeCloseSound.Play();
        }

        while (Quaternion.Angle(transform.rotation, defaultRot) > rotationTolerance)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, defaultRot, Time.deltaTime * smooth);
            yield return null;
        }

        transform.rotation = defaultRot;
        isMoving = false;
    }

    void OnGUI()
    {
        // Only show hand texture when near an interactable fridge
        if (handTexture != null && isNearFridge)
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
}

