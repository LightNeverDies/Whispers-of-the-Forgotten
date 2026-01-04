using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public GameObject[] inventorySlots;
    private List<Item> inventoryItems = new List<Item>();
    public bool isItemNear = false;

    public GameObject inventoryPanel;
    public Hints hints;
    [Tooltip("The hand texture to display in the center of the screen")]
    public Texture2D handTexture;
    [Tooltip("Scale factor for the hand texture (0.1 = 10% of original size)")]
    public float handTextureScale = 0.05f;
    public float rayDistance = 1f;
    public LayerMask itemLayerMask;
    [Tooltip("Layer mask for obstructions (walls, etc.) that block raycast. Leave as 'Nothing' to use everything except itemLayerMask.")]
    public LayerMask obstructionLayer;
    public LockerController locker;
    public ObjectiveManager objectiveManager;
    public SubtitleManager subtitleManager;
    

    [HideInInspector]
    public Item currentItem;
    public bool isInventoryOpen = false;

    // Used by tutorial to block inventory opening until the appropriate step.
    [HideInInspector]
    public bool canToggleInventory = true;

    private Camera mainCamera;

    private void Start()
    {
        inventoryPanel.SetActive(false);
        itemLayerMask = LayerMask.GetMask("Pickable");
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (canToggleInventory && Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }

        CheckForItem();

        if (Input.GetKeyDown(KeyCode.E) && isItemNear)
        {
            TryPickupItem();
        }
    }

    private void ToggleInventory()
    {
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);

        isInventoryOpen = inventoryPanel.activeSelf;

        if (inventoryPanel.activeSelf) { 
        
            Time.timeScale = 0f;
        } else
        {
            Time.timeScale = 1f;
        }
    }

    private void CheckForItem()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        // Cursor is locked in FPS controls; use center-screen ray for consistent pickup behavior.
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, itemLayerMask))
        {
            // Check for obstructions between camera and hit point
            Vector3 camPos = mainCamera.transform.position;
            Vector3 directionToItem = hit.point - camPos;
            float distanceToItem = Vector3.Distance(camPos, hit.point);
            RaycastHit obstructionHit;
            
            // Use obstructionLayer if set, otherwise use everything except itemLayerMask
            LayerMask obstructionCheckLayer = obstructionLayer.value != 0 ? obstructionLayer : ~itemLayerMask;
            
            if (Physics.Raycast(camPos, directionToItem.normalized, out obstructionHit, distanceToItem, obstructionCheckLayer))
            {
                // Check if the obstruction is the target object itself
                if (obstructionHit.collider != hit.collider)
                {
                    // There's an obstruction blocking the view
                    if (isItemNear)
                    {
                        isItemNear = false;
                        currentItem = null;
                    }
                    return;
                }
            }

            if (hit.collider.CompareTag("Pickupable"))
            {

                Item item = hit.collider.GetComponent<Item>();
                
                if (item != null && item.isReadOnly == false)
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

        if(isItemNear)
        {
            isItemNear = false;
            currentItem = null;
        }
    }

    void OnGUI()
    {
        // Only show hand texture when near a pickupable item
        if (Event.current.type == EventType.Repaint && handTexture != null && isItemNear)
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

    private void TryPickupItem()
    {
        if (currentItem != null)
        {
            AddItemToInventory(currentItem);
            
            // Show subtitle when item is picked up
            if (subtitleManager != null)
            {
                subtitleManager.ShowSubtitle("I will put it in my bag.");
            }
            
            if (locker != null)
            {
                locker.OnItemPickedUp();
            }

            if (objectiveManager != null)
            {
                objectiveManager.OnItemPickedUp(currentItem.itemObjective);
            }

            Destroy(currentItem.gameObject);
            currentItem = null;
            isItemNear = false;
        }
    }
    private void AddItemToInventory(Item item)
    {
        if (inventoryItems.Count < inventorySlots.Length)
        {
            inventoryItems.Add(item);
            UpdateInventoryUI();
        }
    }

    private void UpdateInventoryUI()
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            Button slotButton = inventorySlots[i].GetComponent<Button>();

            Image slotImage = slotButton.GetComponentInChildren<Image>();

            Text slotText = slotButton.GetComponentInChildren<Text>();

            if (i < inventoryItems.Count)
            {
                slotImage.sprite = inventoryItems[i].itemSprite;
                slotText.text = inventoryItems[i].itemName;
                slotImage.color = new Color32(255, 255, 255, 255);
            }
            else
            {

                slotImage.sprite = null;
                slotText.text = "";
                slotImage.color = new Color32(255, 255, 255, 0);
            }
        }
    }

    public bool HasItem(string itemName)
    {
        foreach (var item in inventoryItems)
        {
            if (item.itemName == itemName)
            {
                return true;
            }
        }
        return false;
    }

    public void UseItem(string itemName)
    {
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].itemName == itemName)
            {
                inventoryItems.RemoveAt(i);
                UpdateInventoryUI();
                return;
            }
        }
    }
}
