using Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StoryPickController : MonoBehaviour, IInteractableHint
{
    public Hints hints;
    [Tooltip("The hand texture to display in the center of the screen")]
    public Texture2D handTexture;
    [Tooltip("Scale factor for the hand texture (0.1 = 10% of original size)")]
    public float handTextureScale = 0.05f;
    public float rayDisance = 1f;
    public LayerMask itemLayerMask;
    public GameObject storyLine; // UI панелът с историята (Canvas Panel)
    public GameObject inventory;
    public ObjectiveManager objectiveManager;
    public Button continueButton;
    public float typingSpeed = 25f;
    public Image backgroundImage; // Image компонент за показване на хартията

    public bool isItemNear = false;
    private DoorController doorController;

    [HideInInspector] 
    public Item currentItem;
    public bool IsInteractableNear => isItemNear;
    public int collectedPieces;

    private void Start()
    {
        storyLine.SetActive(false);
        continueButton.gameObject.SetActive(false);
        itemLayerMask = LayerMask.GetMask("Pickable");
        doorController = FindObjectOfType<DoorController>();
    }

    private void Update()
    {
        CheckForItem();

        if (Input.GetKeyUp(KeyCode.E) && isItemNear)
        {
            if (inventory.activeInHierarchy)
            {
                inventory.SetActive(false);
            }
            ToggleStoryLine();
            ReadMessage();
        }
    }

    private void CheckForItem()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        bool wasItemNear = isItemNear;

        if (Physics.Raycast(ray, out hit, rayDisance, itemLayerMask))
        {
            if (hit.collider != null && hit.collider.CompareTag("Story"))
            {
                Vector3 directionToItem = hit.point - Camera.main.transform.position;
                RaycastHit obstructionHit;

                if (!Physics.Raycast(Camera.main.transform.position, directionToItem, out obstructionHit,
                        Vector3.Distance(Camera.main.transform.position, hit.point), ~itemLayerMask))
                {
                    Item item = hit.collider.GetComponent<Item>();
                    if (item != null && item.isReadOnly)
                    {
                        if (!isItemNear || currentItem != item)
                        {
                            currentItem = item;
                        }
                        isItemNear = true;
                        return;
                    }
                }
            }
        }

        if (wasItemNear)
        {
            isItemNear = false;
            currentItem = null;
        }
    }

    void OnGUI()
    {
        // Only show hand texture when near a story item
        if (handTexture != null && isItemNear)
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

    private void ReadMessage()
    {
        if (currentItem != null && currentItem.isReadOnly)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            TextMeshProUGUI contentText = storyLine.GetComponentInChildren<TextMeshProUGUI>();
            contentText.text = currentItem.content;

            if (currentItem.storyBackground != null && backgroundImage != null)
            {
                backgroundImage.sprite = currentItem.storyBackground;
                backgroundImage.color = Color.white;
            }

            continueButton.gameObject.SetActive(true);
            Time.timeScale = 0f;
            collectedPieces += 1;
            objectiveManager.OnPieceCollected(collectedPieces);
            isItemNear = false;

            Destroy(currentItem.gameObject);
            currentItem = null;
        }
    }

    public void ContinueButtonEndGame()
    {
        if (SceneManager.GetActiveScene().name == "WhisperEndGame" && doorController != null)
        {
            doorController.EndGame();
            storyLine.SetActive(!storyLine.activeSelf);
            Cursor.visible = false;
            Time.timeScale = 1f;
        }
    }

    public void ToggleStoryLine()
    {
        storyLine.SetActive(!storyLine.activeSelf);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Time.timeScale = 1f;
    }
}