using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public GameObject[] inventorySlots;
    private List<Item> inventoryItems = new List<Item>();
    private bool isItemNear = false;

    public GameObject inventoryPanel;
    public Hints hints;
    public float rayDistance = 1f;
    public LayerMask itemLayerMask;
    public LockerController locker;
    public ObjectiveManager objectiveManager;

    [HideInInspector]
    public Item currentItem;

    private void Start()
    {
        inventoryPanel.SetActive(false);
        itemLayerMask = LayerMask.GetMask("Pickable");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
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
    }

    private void CheckForItem()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayDistance, itemLayerMask))
        {

            if (hit.collider.CompareTag("Pickupable"))
            {

                Item item = hit.collider.GetComponent<Item>();
                
                if (item != null && item.isReadOnly == false)
                {
                    if (!isItemNear || currentItem != item)
                    {
                        currentItem = item;
                        hints.ShowHint("Press E to Pickup " + item.itemName);
                    }
                    isItemNear = true;
                    return;
                }
                
            }
        }

        if(isItemNear)
        {
            hints.HideHint();
            isItemNear = false;
            currentItem = null;
        }
    }

    private void TryPickupItem()
    {
        if (currentItem != null)
        {
            AddItemToInventory(currentItem);
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
            hints.HideHint();
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
