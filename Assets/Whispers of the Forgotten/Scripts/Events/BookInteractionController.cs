using System.Collections;
using System.Linq;
using UnityEngine;

public class BookInteractionController : MonoBehaviour
{
    public Camera mainCamera;
    public Transform mainBookshelfTransform; // Главен обект, съдържащ книгите
    public Hints hints; // Система за подсказки
    public GameObject bookUI; // UI за книгите
    public float interactDistance = 2.0f;
    public LayerMask interactionLayer;
    public MoveCamera moveCameraScript; // Скрипт за движение на камерата
    public AudioSource heavyObjectPushedSoundEffect;
    public GameObject secretDoor;
    public CharacterController playerController; // Контролер за движение на играча
    public GameObject inventoryManager; // GameObject с Inventory System
    private MonoBehaviour inventorySystemScript; // Скриптът за Inventory System

    private bool isNearBook = false;
    private bool isOpenBookView = false;
    private Vector3 playerInitialPosition;
    private Quaternion playerInitialRotation;
    private float playerInitialFieldOfView;
    private bool isColorsPuzzleDone = false;

    // Правилна комбинация на цветовете (индекси в масива colors)
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
            if (hit.collider.CompareTag("BooksHolder"))
            {
                isNearBook = true;
                hints.ShowHint("Press E to Interact");

                if (Input.GetKeyDown(KeyCode.E))
                {
                    OpenBookView(mainBookshelfTransform);
                }

                return;
            }

            if (hit.collider.CompareTag("Book"))
            {
                // Обработка на клика върху книга
                BookController bookController = hit.collider.GetComponent<BookController>();
                if (bookController != null && Input.GetMouseButtonDown(0)) // Ляв клик
                {
                    bookController.CycleMaterial(); // Циклирайте цвета на книгата
                    CheckForCorrectCombination(); // Проверка на комбинацията
                }
            }
        }

        if (isNearBook)
        {
            hints.HideHint();
            isNearBook = false;
        }
    }

    public void OpenBookView(Transform targetTransform)
    {
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

        // Изключване на Inventory System скрипта
        if (inventorySystemScript != null)
        {
            inventorySystemScript.enabled = false;
        }
    }

    public void CloseBookView()
    {
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

        // Включване на Inventory System скрипта
        if (inventorySystemScript != null)
        {
            inventorySystemScript.enabled = true;
        }
    }

    private void CheckForCorrectCombination()
    {
        // Вземаме само книгите с правилния таг
        BookController[] bookControllers = mainBookshelfTransform.GetComponentsInChildren<BookController>()
                                       .Where(book => book.CompareTag("Book")).ToArray();

        int[] currentCombination = new int[correctCombination.Length];

        for (int i = 0; i < correctCombination.Length; i++)
        {
            currentCombination[i] = bookControllers[i].GetCurrentMaterialIndex();
            Debug.Log(currentCombination[i]);
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
