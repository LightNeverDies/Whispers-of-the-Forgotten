using UnityEngine;

public class LockerController : MonoBehaviour
{
    public Hints hints;
    public float smooth = 2.0f;
    public float lockerOpenDistance = 0.3f;
    public float positionTolerance = 0.1f;
    public float rayDistance = 1.0f;
    public LayerMask itemLayerMask;
    public AudioSource lockerSourceOpen;
    public AudioSource lockerSourceClose;
    public InventorySystem inventory;
    public GameObject gameObjectItem;

    private Vector3 defaultPos;
    private Vector3 openPos;
    private bool lockerOpened = false;
    private bool lastLockerState = false;
    private bool isPlayerNear = false;

    public Collider[] itemColliders;

    private void Start()
    {
        defaultPos = transform.position;
        openPos = defaultPos + transform.right * lockerOpenDistance;

        SetItemCollidersActive(false);
    }

    private void Update()
    {
        CheckForPlayerInteraction();

        if (lockerOpened)
        {
            if (Vector3.Distance(transform.position, openPos) > positionTolerance)
            {
                transform.position = Vector3.Lerp(transform.position, openPos, Time.deltaTime * smooth);
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, defaultPos, Time.deltaTime * smooth);
        }

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
            if (hit.collider.gameObject == gameObject)
            {
                if (!isPlayerNear)
                {
                    hints.ShowHint(lockerOpened ? "Press E to Close" : "Press E to Open");
                    isPlayerNear = true;
                }
                return;
            }
        }

        if (isPlayerNear)
        {
            hints.HideHint();
            isPlayerNear = false;
        }
    }

    private void ToggleLockerState()
    {
        lockerOpened = !lockerOpened;

        if (lockerOpened != lastLockerState)
        {
            if (lockerOpened)
            {
                lockerSourceClose.enabled = false;
                lockerSourceOpen.enabled = true;
                lockerSourceOpen.Play();
            }
            else
            {
                lockerSourceOpen.enabled = false;
                lockerSourceClose.enabled = true;
                lockerSourceClose.Play();
            }

            lastLockerState = lockerOpened;

            // Update item colliders based on locker state
            SetItemCollidersActive(lockerOpened);
        }
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
