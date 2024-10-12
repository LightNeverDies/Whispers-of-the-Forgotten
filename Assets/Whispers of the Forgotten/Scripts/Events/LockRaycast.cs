using UnityEngine;

public class LockRaycast : MonoBehaviour
{
    public Camera mainCamera;
    public Transform lockTransform; // Transform на катинара
    public Hints hints;
    public SubtitleManager subtitleManager;
    public GameObject lockUI;
    public float interactDistance = 1.0f;
    public LayerMask interactionLayer;
    public MoveCamera moveCameraScript; // Скрипт за движение на камерата
    public CharacterController playerController; // Контролер за движение на играча
    public GameObject inventoryManager; // GameObject с Inventory System
    private MonoBehaviour inventorySystemScript; // Скриптът за Inventory System

    private bool isNearLock = false;
    private bool isOpenLockView = false;
    private Vector3 playerInitialPosition;
    private Quaternion playerInitialRotation;
    private float playerInitialFieldOfView;

    // Позиция и ротация на камерата за близък изглед
    public Vector3 lockViewPositionOffset = new Vector3(0, 0, -2f);
    public float lockViewFieldOfView = 26f; // Поле на виждане за близък изглед

    void Start()
    {
        lockUI.SetActive(false);

        // Намери и запази Inventory System скрипта
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
            if (hit.collider.CompareTag("Lock"))
            {
                isNearLock = true;
                hints.ShowHint("Press E to Interact");
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
            hints.HideHint();
            isNearLock = false;
        }
    }

    public void OpenLockView()
    {
        lockUI.SetActive(true);
        isOpenLockView = true;

        // Запазване на текущото местоположение и ротация на камерата
        playerInitialPosition = mainCamera.transform.position;
        playerInitialRotation = mainCamera.transform.rotation;
        playerInitialFieldOfView = mainCamera.fieldOfView;

        // Настройване на позицията и ротацията на камерата за близък изглед
        Vector3 lockViewPosition = lockTransform.position + lockViewPositionOffset;
        mainCamera.transform.position = lockViewPosition;
        mainCamera.transform.LookAt(lockTransform);

        // Настройване на полето на виждане на камерата
        mainCamera.fieldOfView = lockViewFieldOfView;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Изключване на скрипта за движение на камерата и играча
        if (moveCameraScript != null)
        {
            moveCameraScript.enabled = false;
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Изключване на Inventory System скрипта
        if (inventorySystemScript != null)
        {
            inventorySystemScript.enabled = false;
        }
    }

    public void CloseLockView()
    {
        lockUI.SetActive(false);
        isOpenLockView = false;

        // Възстановяване на позицията на камерата
        mainCamera.transform.position = playerInitialPosition;

        // Възстановяване на ротацията на камерата
        mainCamera.transform.rotation = playerInitialRotation;

        // Възстановяване на полето на виждане на камерата
        mainCamera.fieldOfView = playerInitialFieldOfView;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Включване отново на скрипта за движение на камерата и играча
        if (moveCameraScript != null)
        {
            moveCameraScript.enabled = true;
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Включване на Inventory System скрипта
        if (inventorySystemScript != null)
        {
            inventorySystemScript.enabled = true;
        }
    }
}
