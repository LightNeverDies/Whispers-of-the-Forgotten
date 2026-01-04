using UnityEngine;
using Interfaces;

public class LockRaycast : MonoBehaviour, IInteractableHint
{
    public Camera mainCamera;
    public Transform lockTransform;
    public Hints hints;
    [Tooltip("The hand texture to display in the center of the screen")]
    public Texture2D handTexture;
    [Tooltip("Scale factor for the hand texture (0.1 = 10% of original size)")]
    public float handTextureScale = 0.05f;
    public SubtitleManager subtitleManager;
    public GameObject lockUI;
    public float interactDistance = 1.0f;
    public LayerMask interactionLayer;
    [Tooltip("Layer mask for obstructions (walls, etc.) that block raycast. Leave as 'Nothing' to use everything except interactionLayer.")]
    public LayerMask obstructionLayer;
    public MoveCamera moveCameraScript;
    public CharacterController playerController;
    public GameObject inventoryManager;
    private MonoBehaviour inventorySystemScript;
    public BoxCollider boxCollider;

    private bool isNearLock = false;
    private bool isOpenLockView = false;
    private Vector3 playerInitialPosition;
    private Quaternion playerInitialRotation;
    private float playerInitialFieldOfView;

    public bool IsInteractableNear => isNearLock;

    public Vector3 lockViewPositionOffset = new Vector3(0, 0, -0.3f);
    public float lockViewFieldOfView = 26f;

    void Start()
    {
        lockUI.SetActive(false);

        inventorySystemScript = inventoryManager.GetComponent<MonoBehaviour>();

        playerInitialPosition = mainCamera.transform.position;
        playerInitialRotation = mainCamera.transform.rotation;
        playerInitialFieldOfView = mainCamera.fieldOfView;
    }

    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Input.GetKeyDown(KeyCode.Q) && isOpenLockView)
        {
            CloseLockView();
        }

        if (Physics.Raycast(ray, out hit, interactDistance, interactionLayer))
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
                    if (isNearLock)
                    {
                        isNearLock = false;
                    }
                    return;
                }
            }
            
            if (hit.collider.CompareTag("Lock"))
            {
                isNearLock = true;
                subtitleManager.ShowSubtitle("I need four digit code.");
                if (Input.GetKeyDown(KeyCode.E))
                {
                    OpenLockView();
                }

                return;
            }
        }

        if (isNearLock)
        {
            isNearLock = false;
        }
    }

    void OnGUI()
    {
        // Only show hand texture when near lock
        if (handTexture != null && isNearLock)
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

    public void OpenLockView()
    {
        FindObjectOfType<FlashlightController>().isInLockView = true;
        lockUI.SetActive(true);
        isOpenLockView = true;
        boxCollider.enabled = false;

        playerInitialPosition = mainCamera.transform.position;
        playerInitialRotation = mainCamera.transform.rotation;
        playerInitialFieldOfView = mainCamera.fieldOfView;

        Vector3 lockViewPosition = lockTransform.position + lockViewPositionOffset;
        mainCamera.transform.position = lockViewPosition;
        mainCamera.transform.LookAt(lockTransform);

        mainCamera.fieldOfView = lockViewFieldOfView;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (moveCameraScript != null)
        {
            moveCameraScript.enabled = false;
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (inventorySystemScript != null)
        {
            inventorySystemScript.enabled = false;
        }
    }

    public void CloseLockView()
    {
        FindObjectOfType<FlashlightController>().isInLockView = false;
        lockUI.SetActive(false);
        isOpenLockView = false;
        boxCollider.enabled = true;

        mainCamera.transform.position = playerInitialPosition;

        mainCamera.transform.rotation = playerInitialRotation;

        mainCamera.fieldOfView = playerInitialFieldOfView;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (moveCameraScript != null)
        {
            moveCameraScript.enabled = true;
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (inventorySystemScript != null)
        {
            inventorySystemScript.enabled = true;
        }
    }
}
