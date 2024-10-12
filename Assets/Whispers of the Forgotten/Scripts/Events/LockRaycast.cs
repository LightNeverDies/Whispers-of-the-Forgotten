using UnityEngine;

public class LockRaycast : MonoBehaviour
{
    public Camera mainCamera;
    public Transform lockTransform; // Transform �� ��������
    public Hints hints;
    public SubtitleManager subtitleManager;
    public GameObject lockUI;
    public float interactDistance = 1.0f;
    public LayerMask interactionLayer;
    public MoveCamera moveCameraScript; // ������ �� �������� �� ��������
    public CharacterController playerController; // ��������� �� �������� �� ������
    public GameObject inventoryManager; // GameObject � Inventory System
    private MonoBehaviour inventorySystemScript; // �������� �� Inventory System

    private bool isNearLock = false;
    private bool isOpenLockView = false;
    private Vector3 playerInitialPosition;
    private Quaternion playerInitialRotation;
    private float playerInitialFieldOfView;

    // ������� � ������� �� �������� �� ������ ������
    public Vector3 lockViewPositionOffset = new Vector3(0, 0, -2f);
    public float lockViewFieldOfView = 26f; // ���� �� ������� �� ������ ������

    void Start()
    {
        lockUI.SetActive(false);

        // ������ � ������ Inventory System �������
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

        // ��������� �� �������� �������������� � ������� �� ��������
        playerInitialPosition = mainCamera.transform.position;
        playerInitialRotation = mainCamera.transform.rotation;
        playerInitialFieldOfView = mainCamera.fieldOfView;

        // ����������� �� ��������� � ��������� �� �������� �� ������ ������
        Vector3 lockViewPosition = lockTransform.position + lockViewPositionOffset;
        mainCamera.transform.position = lockViewPosition;
        mainCamera.transform.LookAt(lockTransform);

        // ����������� �� ������ �� ������� �� ��������
        mainCamera.fieldOfView = lockViewFieldOfView;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // ���������� �� ������� �� �������� �� �������� � ������
        if (moveCameraScript != null)
        {
            moveCameraScript.enabled = false;
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // ���������� �� Inventory System �������
        if (inventorySystemScript != null)
        {
            inventorySystemScript.enabled = false;
        }
    }

    public void CloseLockView()
    {
        lockUI.SetActive(false);
        isOpenLockView = false;

        // �������������� �� ��������� �� ��������
        mainCamera.transform.position = playerInitialPosition;

        // �������������� �� ��������� �� ��������
        mainCamera.transform.rotation = playerInitialRotation;

        // �������������� �� ������ �� ������� �� ��������
        mainCamera.fieldOfView = playerInitialFieldOfView;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // ��������� ������ �� ������� �� �������� �� �������� � ������
        if (moveCameraScript != null)
        {
            moveCameraScript.enabled = true;
        }

        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // ��������� �� Inventory System �������
        if (inventorySystemScript != null)
        {
            inventorySystemScript.enabled = true;
        }
    }
}
