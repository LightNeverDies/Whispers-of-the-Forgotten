using Interfaces;
using UnityEngine;
using System.Collections;

public class LockerController : MonoBehaviour, IInteractableHint
{
    public Hints hints;
    [Tooltip("The hand texture to display in the center of the screen")]
    public Texture2D handTexture;
    [Tooltip("Scale factor for the hand texture (0.1 = 10% of original size)")]
    public float handTextureScale = 0.05f;
    [Tooltip("Duration in seconds for closing the locker")]
    public float smoothClose = 0.5f;
    [Tooltip("Duration in seconds for opening the locker")]
    public float smoothOpen = 0.5f;
    public float lockerOpenDistance = 0.3f;
    [Tooltip("The direction in which the locker opens. Use transform.right, transform.forward, -transform.right, -transform.forward")]
    public Vector3 openDirection = Vector3.zero;
    public float positionTolerance = 0.1f;
    public float rayDistance = 1.0f;
    public LayerMask itemLayerMask;
    [Tooltip("Layer mask for obstructions (walls, etc.) that block raycast. Leave as 'Nothing' to use everything except itemLayerMask.")]
    public LayerMask obstructionLayer;
    public AudioSource lockerSourceOpen;
    public AudioSource lockerSourceClose;
    public InventorySystem inventory;
    public GameObject gameObjectItem;

    private Vector3 defaultPos;
    private Vector3 openPos;
    private bool lockerOpened = false;
    private bool lastLockerState = false;
    private bool isPlayerNear = false;
    private bool isMoving = false;
    private Coroutine moveCoroutine;

    public Collider[] itemColliders;

    [HideInInspector]
    public bool IsInteractableNear => isPlayerNear;

    private void Start()
    {
        defaultPos = transform.position;
        CalculateOpenPosition();

        SetItemCollidersActive(false);
    }

    private void CalculateOpenPosition()
    {
        // If openDirection is set, use it directly as offset (allows for precise positioning like 0,0,0.4)
        // Otherwise, use transform.right * lockerOpenDistance for backwards compatibility
        if (openDirection != Vector3.zero)
        {
            openPos = defaultPos + openDirection;
        }
        else
        {
            openPos = defaultPos + transform.right * lockerOpenDistance;
        }
    }

    private void Update()
    {
        CheckForPlayerInteraction();

        if (Input.GetKeyDown(KeyCode.E) && isPlayerNear)
        {
            ToggleLockerState();
        }
    }

    private void CheckForPlayerInteraction()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, itemLayerMask))
        {
            // Check for obstructions between camera and hit point
            Vector3 directionToHit = hit.point - Camera.main.transform.position;
            float distanceToHit = Vector3.Distance(Camera.main.transform.position, hit.point);
            RaycastHit obstructionHit;
            
            // Use obstructionLayer if set, otherwise use everything except itemLayerMask
            LayerMask obstructionCheckLayer = obstructionLayer.value != 0 ? obstructionLayer : ~itemLayerMask;
            
            if (Physics.Raycast(Camera.main.transform.position, directionToHit.normalized, out obstructionHit, distanceToHit, obstructionCheckLayer))
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
            
            if (hit.collider.gameObject == gameObject)
            {
                if (!isPlayerNear)
                {
                    isPlayerNear = true;
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
        // Only show hand texture when near an interactable locker
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

    private void ToggleLockerState()
    {
        if (isMoving) return;

        lockerOpened = !lockerOpened;

        if (lockerOpened != lastLockerState)
        {
            if (lockerOpened)
            {
                lockerSourceClose.Stop();
                lockerSourceOpen.Play();
                if (moveCoroutine != null) StopCoroutine(moveCoroutine);
                moveCoroutine = StartCoroutine(MoveLocker(transform.position, openPos, smoothOpen));
            }
            else
            {
                lockerSourceOpen.Stop();
                lockerSourceClose.Play();
                if (moveCoroutine != null) StopCoroutine(moveCoroutine);
                moveCoroutine = StartCoroutine(MoveLocker(transform.position, defaultPos, smoothClose));
            }

            lastLockerState = lockerOpened;

            SetItemCollidersActive(lockerOpened);
        }
    }

    private IEnumerator MoveLocker(Vector3 startPos, Vector3 targetPos, float duration)
    {
        isMoving = true;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        // Ensure we're exactly at the target position
        transform.position = targetPos;
        isMoving = false;
    }


    public void OnItemPickedUp()
    {
        SetItemCollidersActive(false);
        gameObjectItem.GetComponent<BoxCollider>().enabled = true;
    }

    private void SetItemCollidersActive(bool isActive)
    {
        foreach (var collider in itemColliders)
        {
            if (collider != null)
            {
                collider.enabled = isActive;
            }
        }
    }
}
