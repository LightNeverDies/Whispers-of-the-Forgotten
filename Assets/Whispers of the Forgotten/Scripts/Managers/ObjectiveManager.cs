using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ObjectiveManager : MonoBehaviour
{
    public TextMeshProUGUI objectiveText;
    public StoryPickController storyPickController;
    public float completionFadeDuration = 3.0f;
    public float stageTransitionDelay = 2.0f;
    public List<UVTextVisibilityController> uvTextControllers;

    // Променливи за сценичен преход
    public SceneTransitionController sceneTransition;
    public string sceneToLoad;

    private ObjectiveStage[] stages;
    private int currentStageIndex = 0;
    private int totalPieces = 6;

    void Start()
    {
        if(SceneManager.GetActiveScene().name == "WhisperEndGame")
        {
            return;
        }

        stages = new ObjectiveStage[]
        {
            new ObjectiveStage(new string[] { "Find Bathroom Key", "Open Bathroom" }),
            new ObjectiveStage(new string[] { "Find Storage Key", "Open Storage" }),
            new ObjectiveStage(new string[] { "Find Office Key", "Open Office" }),
            new ObjectiveStage(new string[] { "Find Bedroom Key", "Open Bedroom" }),
            new ObjectiveStage(new string[] { $"Collect Story Pieces {storyPickController.collectedPieces}/{totalPieces}" }),
            new ObjectiveStage(new string[] { "Find Red Key", "Open Red Door" })
        };

        if (storyPickController.collectedPieces > 0)
        {
            OnPieceCollected(storyPickController.collectedPieces);
        }

        UpdateObjectiveText();
    }

    public void OnItemPickedUp(string itemName)
    {
        if (currentStageIndex < 0 || currentStageIndex >= stages.Length)
        {
            return;
        }

        ObjectiveStage currentStage = stages[currentStageIndex];

        foreach (var objective in currentStage.objectives)
        {
            if (objective.description.Contains(itemName) && !objective.isCompleted)
            {
                StartCoroutine(AnimateObjectiveCompletion(objective));
                objective.isCompleted = true;

                if (objective.description.Contains("Open Red Door"))
                {

                    if (sceneTransition != null && !string.IsNullOrEmpty(sceneToLoad))
                    {
                        sceneTransition.sceneToLoad = sceneToLoad;
                        StartCoroutine(sceneTransition.ExpandPanel());
                    }
                }

                break;
            }
        }

        if (currentStage.IsStageCompleted())
        {
            StartCoroutine(HandleStageTransition());
        }
        else
        {
            UpdateObjectiveText();
        }
    }


    public void OnPieceCollected(int piecesCollected)
    {
        if (SceneManager.GetActiveScene().name != "WhisperEndGame") {
            foreach (var objective in stages[currentStageIndex].objectives)
            {
                if (objective.description.Contains("Collect Story Pieces"))
                {
                    objective.description = $"Collect Story Pieces {piecesCollected}/{totalPieces}";

                    if (piecesCollected >= totalPieces)
                    {
                        objective.isCompleted = true;
                        StartCoroutine(HandleStageTransition());
                    }
                    break;
                }
            }

            UpdateObjectiveText();
        }    
    }

    void UpdateObjectiveText()
    {
        if (currentStageIndex < 0 || currentStageIndex >= stages.Length)
        {
            return;
        }

        objectiveText.text = "Objectives:\n\n";

        ObjectiveStage currentStage = stages[currentStageIndex];
        foreach (var objective in currentStage.objectives)
        {
            if (objective.isCompleted)
            {
                objectiveText.text += "<s><color=green>- " + objective.description + "</color></s>\n\n";
            }
            else
            {
                objectiveText.text += "<color=red>- " + objective.description + "</color>\n\n";
            }
        }
    }

    IEnumerator HandleStageTransition()
    {
        UpdateObjectiveText();
        yield return new WaitForSeconds(stageTransitionDelay);

        currentStageIndex++;

        if (currentStageIndex >= stages.Length)
        {
            objectiveText.text = "Objectives completed!";
            yield return new WaitForSeconds(stageTransitionDelay);
            objectiveText.text = " ";
        }
        else
        {
            if (currentStageIndex == 4) // Проверка дали сме в етапа за събиране на частите
            {
                stages[currentStageIndex].objectives[0].description = $"Collect Story Pieces {storyPickController.collectedPieces}/{totalPieces}";
                foreach (var controller in uvTextControllers)
                {
                    controller.StartMission();
                }
            }

            UpdateObjectiveText();
        }
    }

    IEnumerator AnimateObjectiveCompletion(Objective objective)
    {
        UpdateObjectiveText();
        yield return new WaitForSeconds(completionFadeDuration);
        UpdateObjectiveText();
    }
}

// Клас за етапите с цели
public class ObjectiveStage
{
    public Objective[] objectives;

    public ObjectiveStage(string[] descriptions)
    {
        objectives = new Objective[descriptions.Length];
        for (int i = 0; i < descriptions.Length; i++)
        {
            objectives[i] = new Objective(descriptions[i]);
        }
    }

    public bool IsStageCompleted()
    {
        foreach (var objective in objectives)
        {
            if (!objective.isCompleted)
            {
                return false;
            }
        }
        return true;
    }
}

// Клас за всяка отделна цел
public class Objective
{
    public string description;
    public bool isCompleted;

    public Objective(string description)
    {
        this.description = description;
        this.isCompleted = false;
    }
}
