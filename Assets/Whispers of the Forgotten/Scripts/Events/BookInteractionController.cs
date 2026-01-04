using System.Collections;
using System.Linq;
using UnityEngine;

public class BookInteractionController : MonoBehaviour
{
    public Camera mainCamera;
    public Transform mainBookshelfTransform;
    public Hints hints;
    [Tooltip("The hand texture to display in the center of the screen")]
    public Texture2D handTexture;
    [Tooltip("Scale factor for the hand texture (0.1 = 10% of original size)")]
    public float handTextureScale = 0.05f;
    public GameObject bookUI; // UI
    public float interactDistance = 2.0f;
    public LayerMask interactionLayer;
    [Tooltip("Layer mask for obstructions (walls, etc.) that block raycast. Leave as 'Nothing' to use everything except interactionLayer.")]
    public LayerMask obstructionLayer;
    public MoveCamera moveCameraScript;
    public AudioSource heavyObjectPushedSoundEffect;
    public GameObject secretDoor;
    public CharacterController playerController;
    public GameObject inventoryManager; // GameObject Inventory System
    private MonoBehaviour inventorySystemScript; // Inventory System

    private bool isNearBook = false;
    private bool isOpenBookView = false;
    private Vector3 playerInitialPosition;
    private Quaternion playerInitialRotation;
    private float playerInitialFieldOfView;
    private bool isColorsPuzzleDone = false;

    private int[] correctCombination = { 2, 5, 6, 3 };

    void Start()
    {
        bookUI.SetActive(false);

        inventorySystemScript = inventoryManager.GetComponent<MonoBehaviour>();

        playerInitialPosition = mainCamera.transform.position;
        playerInitialRotation = mainCamera.transform.rotation;
        playerInitialFieldOfView = mainCamera.fieldOfView;
    }

    void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(isColorsPuzzleDone)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q) && isOpenBookView)
        {
            CloseBookView();
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
                    if (isNearBook)
                    {
                        isNearBook = false;
                    }
                    return;
                }
            }
            
            if (hit.collider.CompareTag("BooksHolder"))
            {
                isNearBook = true;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    OpenBookView(mainBookshelfTransform);
                }

                return;
            }

            if (hit.collider.CompareTag("Book"))
            {
                BookController bookController = hit.collider.GetComponent<BookController>();
                if (bookController != null && Input.GetMouseButtonDown(0))
                {
                    bookController.CycleMaterial();
                    CheckForCorrectCombination();
                }
            }
        }

        if (isNearBook)
        {
            isNearBook = false;
        }
    }

    void OnGUI()
    {
        // Only show hand texture when near an interactable book
        if (handTexture != null && isNearBook)
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

    public void OpenBookView(Transform targetTransform)
    {
        FindObjectOfType<FlashlightController>().isInLockView = true;
        bookUI.SetActive(true);
        isOpenBookView = true;

        mainBookshelfTransform.GetComponent<BoxCollider>().enabled = false;

        playerInitialPosition = mainCamera.transform.position;
        playerInitialRotation = mainCamera.transform.rotation;
        playerInitialFieldOfView = mainCamera.fieldOfView;

        Vector3 bookViewPosition = targetTransform.position + new Vector3(0.8f, 0.1f, 0.1f);
        mainCamera.transform.position = bookViewPosition;
        mainCamera.transform.LookAt(targetTransform);

        mainCamera.transform.rotation = Quaternion.Euler(0, -90f, 0);

        mainCamera.fieldOfView = 26f;

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

    public void CloseBookView()
    {
        FindObjectOfType<FlashlightController>().isInLockView = false;
        bookUI.SetActive(false);
        isOpenBookView = false;

        mainBookshelfTransform.GetComponent<BoxCollider>().enabled = true;

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

    private void CheckForCorrectCombination()
    {
        BookController[] bookControllers = mainBookshelfTransform.GetComponentsInChildren<BookController>()
                                       .Where(book => book.CompareTag("Book")).ToArray();

        int[] currentCombination = new int[correctCombination.Length];

        for (int i = 0; i < correctCombination.Length; i++)
        {
            currentCombination[i] = bookControllers[i].GetCurrentMaterialIndex();
        }

        bool isCorrectCombination = true;
        for (int i = 0; i < correctCombination.Length; i++)
        {
            if (currentCombination[i] != this.correctCombination[i])
            {
                isCorrectCombination = false;
                break;
            }
        }

        if (isCorrectCombination)
        {
            OpenSecretDoor();
            CloseBookView();
            isColorsPuzzleDone = true;
        }
    }

    private void OpenSecretDoor()
    {
        heavyObjectPushedSoundEffect.enabled = true;
        heavyObjectPushedSoundEffect.Play();

        float currentYRotation = secretDoor.transform.rotation.eulerAngles.y;

        float targetAngle = currentYRotation + 45f;

        StartCoroutine(SmoothRotateDoor(targetAngle, 2f));

    }

    IEnumerator SmoothRotateDoor(float targetAngle, float duration)
    {
        Quaternion initialRotation = secretDoor.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            secretDoor.transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        secretDoor.transform.rotation = targetRotation;
    }
}
