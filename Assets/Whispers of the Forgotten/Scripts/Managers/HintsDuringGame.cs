using Interfaces;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class HintTutorial : MonoBehaviour
{
    public GameObject hintPanel;
    public TextMeshProUGUI hintTitleText;
    public TextMeshProUGUI hintBodyText;

    public InventorySystem inventorySystem;
    public StoryPickController storyPickController;
    public DoorController doorController;

    public GameObject flashlightIcon;
    public FlashlightController flashlightController;
    public MoveCamera moveCamera;

    private int currentHintIndex = 0;
    private string[] hintSteps = new string[]
    {
        "Use <color=red>[W]</color><color=red>[A]</color><color=red>[S]</color><color=red>[D]</color> to move around",
        "Press <color=red>[E]</color> to interact with nearby objects",
        "Press <color=red>[I]</color> to open or close your inventory",
        "Press <color=red>[F]</color> to turn on your flashlight",
        "Press <color=red>[R]</color> to switch flashlight mode",
        "Hold <color=red>[RMB]</color> to zoom"
    };

    private bool[] stepCompleted;

    private List<IInteractableHint> interactableHints;

    void Start()
    {
        hintPanel.SetActive(true);
        stepCompleted = new bool[hintSteps.Length];
        ShowHint(currentHintIndex);

        if (moveCamera == null)
        {
            moveCamera = FindObjectOfType<MoveCamera>();
        }

        interactableHints = new List<IInteractableHint>(FindObjectsOfType<MonoBehaviour>(true).OfType<IInteractableHint>());

        // Block inventory until the tutorial reaches the inventory step.
        if (inventorySystem != null)
        {
            inventorySystem.canToggleInventory = false;
        }
    }

    void Update()
    {
        if (currentHintIndex >= hintSteps.Length) return;

        hintPanel.SetActive(!(inventorySystem != null && inventorySystem.isInventoryOpen));

        if (inventorySystem != null && inventorySystem.isInventoryOpen) return;
    }

    // Use LateUpdate so all interactables have already updated their IsInteractableNear flags for this frame.
    void LateUpdate()
    {
        if (currentHintIndex >= hintSteps.Length) return;
        if (inventorySystem != null && inventorySystem.isInventoryOpen) return;

        switch (currentHintIndex)
        {
            case 0:
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                    Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                    CompleteStep();
                break;
            case 1:
                if (Input.GetKeyDown(KeyCode.E) && IsAnyInteractableNear())
                {
                    CompleteStep();
                }
                break;
            case 2:
                if (Input.GetKeyDown(KeyCode.I)) CompleteStep();
                break;
            case 3:
                if (Input.GetKeyDown(KeyCode.F)) CompleteStep();
                break;
            case 4:
                if (Input.GetKeyDown(KeyCode.R) &&
                    flashlightController != null &&
                    flashlightController.flashlightState != FlashlightController.FlashlightState.Off)
                    CompleteStep();
                break;
            case 5:
                // Zoom: RMB only.
                if (Input.GetMouseButtonDown(1) || Input.GetMouseButton(1))
                    CompleteStep();
                break;
        }
    }

    void CompleteStep()
    {
        stepCompleted[currentHintIndex] = true;
        currentHintIndex++;
        if (currentHintIndex < hintSteps.Length)
        {
            ShowHint(currentHintIndex);
        }
        else
        {
            hintPanel.SetActive(false);
        }
    }

    bool IsAnyInteractableNear()
    {
        foreach (var interactable in interactableHints)
        {
            if (interactable.IsInteractableNear)
                return true;
        }
        return false;
    }

    void ShowHint(int index)
    {
        hintTitleText.text = "HINT";
        hintBodyText.text = hintSteps[index];

        // Allow inventory only when we reach (or pass) the inventory step.
        if (inventorySystem != null)
        {
            inventorySystem.canToggleInventory = index >= 2;
        }

        if (flashlightController != null)
        {
            if (index >= 3) // step 3 is "Press F to turn on flashlight"
            {
                flashlightController.canUseFlashlight = true;
                flashlightIcon.SetActive(true);
            }
            else
            {
                flashlightController.canUseFlashlight = false;
                flashlightIcon.SetActive(false);
            }
        }

        // Allow zoom only when we reach (or pass) the zoom step.
        if (moveCamera != null)
        {
            moveCamera.canUseZoom = index >= 5;
        }
    }
}
