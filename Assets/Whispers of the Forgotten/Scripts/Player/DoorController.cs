using Interfaces;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour, IInteractableHint
{
    public float smooth = 2.0f;
    public float DoorOpenAngle = 90.0f;
    public float rotationTolerance = 1.0f;
    public float interactDistance = 1.0f;
    public LayerMask interactionLayer;
    [Tooltip("Layer mask for obstructions (walls, etc.) that block raycast. Leave as 'Nothing' to use everything except interactionLayer.")]
    public LayerMask obstructionLayer;
    private Quaternion defaultRot;
    private Quaternion openRot;
    public SubtitleManager subtitleManager;
    public AudioSource doorCreak;
    public AudioSource lockDoor;
    public string requiredKeyName;
    public string itemObjective;
    [Tooltip("The hand texture to display in the center of the screen")]
    public Texture2D handTexture;
    [Tooltip("Scale factor for the hand texture (0.1 = 10% of original size)")]
    public float handTextureScale = 0.05f;

    [HideInInspector]
    public bool isMainDoorOpening = false;
    public bool IsInteractableNear => isNearDoor;

    public InventorySystem inventory;
    public ObjectiveManager objectiveManager;

    private bool doorOpened = false;
    public bool isNearDoor = false;

    void Start()
    {
        defaultRot = transform.rotation;
        openRot = Quaternion.Euler(defaultRot.eulerAngles + Vector3.up * DoorOpenAngle);
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactionLayer))
        {
            // Check for obstructions between camera and hit point
            Vector3 camPos = Camera.main.transform.position;
            Vector3 directionToHit = hit.point - camPos;
            float distanceToHit = Vector3.Distance(camPos, hit.point);
            RaycastHit obstructionHit;
            
            // Use obstructionLayer if set, otherwise use everything except interactionLayer
            LayerMask obstructionCheckLayer = obstructionLayer.value != 0 ? obstructionLayer : ~interactionLayer;
            
            if (Physics.Raycast(camPos, directionToHit.normalized, out obstructionHit, distanceToHit, obstructionCheckLayer))
            {
                // Check if the obstruction is the target object itself
                if (obstructionHit.collider != hit.collider)
                {
                    // There's an obstruction blocking the view
                    if (isNearDoor)
                    {
                        isNearDoor = false;
                    }
                    return;
                }
            }
            
            if (hit.collider.CompareTag("Door"))
            {
                DoorController doorController = hit.collider.GetComponent<DoorController>();
                if (doorController != null)
                {
                    doorController.HandleInteraction();
                    isNearDoor = true;
                }
            }
        }
        else if (isNearDoor)
        {
            isNearDoor = false;
        }

    }

    public void HandleInteraction()
    {
        if (!doorOpened && !inventory.HasItem(requiredKeyName))
        {
            if (subtitleManager != null)
            {
                subtitleManager.ShowSubtitle("The door is locked. I need to find the key to open it.");
            }
        }

        if (!inventory.HasItem(requiredKeyName) && Input.GetKeyDown(KeyCode.E))
        {
            lockDoor.enabled = true;
            lockDoor.Play();
        }

        if (Input.GetKeyDown(KeyCode.E) && !doorOpened)
        {
            if (inventory.HasItem(requiredKeyName))
            {
                lockDoor.enabled = false;
                OpenDoor();
                doorOpened = true;
                inventory.UseItem(requiredKeyName);

                if (objectiveManager != null)
                {
                    objectiveManager.OnItemPickedUp(itemObjective);
                }
            }
        }
    }

    private void OpenDoor()
    {
        StartCoroutine(OpenDoorCoroutine());
    }

    private IEnumerator OpenDoorCoroutine()
    {
        doorCreak.enabled = true;
        doorCreak.Play();

        while (Quaternion.Angle(transform.rotation, openRot) > rotationTolerance)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, openRot, Time.deltaTime * smooth);
            yield return null;
        }
    }

    public void EndGame()
    {
        OpenDoor();
        StartCoroutine(EndGameSequence());
    }

    // I Should rotate to specific position and not allowed player to move the camera again Like Rotation should be X:5 0 0
    private IEnumerator EndGameSequence()
    {
        // Wait until the door finishes opening
        while (Quaternion.Angle(transform.rotation, openRot) > rotationTolerance)
        {
            yield return null;
        }

        // Rotate the player to face the door
        Transform playerTransform = Camera.main.transform.parent;
        Vector3 directionToDoor = transform.position - playerTransform.position;
        directionToDoor.y = 0; // Keep the rotation on the y-axis only
        Quaternion targetRotation = Quaternion.LookRotation(directionToDoor);

        float rotationSpeed = 2.0f;
        while (Quaternion.Angle(playerTransform.rotation, targetRotation) > rotationTolerance)
        {
            playerTransform.rotation = Quaternion.Slerp(playerTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            yield return null;
        }

        // Wait for 2-3 seconds
        yield return new WaitForSeconds(2.5f);

        // Load the "Menu" scene
        SceneManager.LoadScene("Menu");
    }

    void OnGUI()
    {
        // Only show hand texture when near an interactable door
        if (Event.current.type == EventType.Repaint && handTexture != null && isNearDoor)
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
