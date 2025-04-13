using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class StoryPickController : MonoBehaviour
{
    public Hints hints;
    public float rayDisance = 1f;
    public LayerMask itemLayerMask;
    public GameObject storyLine;
    public GameObject inventory;
    public ObjectiveManager objectiveManager;
    private bool isItemNear = false;
    private DoorController doorController;

    [HideInInspector]
    public Item currentItem;
    public int collectedPieces;
    public Button continueButton;
    public float typingSpeed = 25f;

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

                if (!Physics.Raycast(Camera.main.transform.position, directionToItem, out obstructionHit, Vector3.Distance(Camera.main.transform.position, hit.point), ~itemLayerMask))
                {
                    Item item = hit.collider.GetComponent<Item>();
                    if (item != null && item.isReadOnly)
                    {
                        if (!isItemNear || currentItem != item)
                        {
                            currentItem = item;
                            hints.ShowHint("Press E to Read " + item.itemName);
                        }
                        isItemNear = true;
                        return;
                    }
                }
            }
        }

        if (wasItemNear)
        {
            hints.HideHint();
            isItemNear = false;
            currentItem = null;
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
            continueButton.gameObject.SetActive(true);

            Time.timeScale = 0f;
            collectedPieces += 1;
            objectiveManager.OnPieceCollected(collectedPieces);
            isItemNear = false;
            Destroy(currentItem.gameObject);
            currentItem = null;
            hints.HideHint();
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