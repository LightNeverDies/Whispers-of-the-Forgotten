using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour
{
    public float smooth = 2.0f;
    public float DoorOpenAngle = 90.0f;
    public float rotationTolerance = 1.0f;
    public float interactDistance = 1.0f;
    public LayerMask interactionLayer;
    private Quaternion defaultRot;
    private Quaternion openRot;
    public Hints hints;
    public AudioSource doorCreak;
    public AudioSource lockDoor;
    public string requiredKeyName;
    public string itemObjective;

    [HideInInspector]
    public bool isMainDoorOpening = false;

    public InventorySystem inventory;
    public ObjectiveManager objectiveManager;

    private bool doorOpened = false;
    private bool isNearDoor = false;

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
            hints.HideHint();
            isNearDoor = false;
        }

    }

    public void HandleInteraction()
    {
        if (!doorOpened && !inventory.HasItem(requiredKeyName))
        {
            hints.ShowHint("The door is locked. You need the " + requiredKeyName);
        }
        else if (!doorOpened && inventory.HasItem(requiredKeyName))
        {
            hints.ShowHint("Press E to Open");
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
}
