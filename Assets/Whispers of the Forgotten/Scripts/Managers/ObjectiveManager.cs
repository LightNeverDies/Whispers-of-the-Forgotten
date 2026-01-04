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
    private int totalPieces = 7;
    private Coroutine stageTransitionCoroutine;

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
            new ObjectiveStage(new string[] { "Find Child's Room Key", "Open Child's Room" }),
            new ObjectiveStage(new string[] { "Find Bedroom Key", "Open Bedroom" }),
            new ObjectiveStage(new string[] { $"Collect Story Pieces {storyPickController.collectedPieces}/{totalPieces}" }),
            new ObjectiveStage(new string[] { "Find Evidence for the Secret Room" }),
            new ObjectiveStage(new string[] { "Find Red Key", "Open Red Door" })
        };

        if (storyPickController.collectedPieces > 0)
        {
            OnPieceCollected(storyPickController.collectedPieces);
        }

        UpdateObjectiveText();
        AutoAdvanceIfCurrentStageAlreadyCompleted();
    }

    // Helper method to normalize apostrophes/quotes for comparison
    private string NormalizeString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        
        // Replace different types of apostrophes/quotes with standard apostrophe
        string result = input.Trim();
        char standardApostrophe = '\''; // Standard apostrophe
        
        // Common apostrophe/quote characters that might cause issues
        result = result.Replace('\u0060', standardApostrophe); // Grave accent `
        result = result.Replace('\u00B4', standardApostrophe); // Acute accent ´
        result = result.Replace('\u2018', standardApostrophe); // Left single quotation mark '
        result = result.Replace('\u2019', standardApostrophe); // Right single quotation mark '
        result = result.Replace('\u201A', standardApostrophe); // Single low-9 quotation mark ‚
        result = result.Replace('\u201B', standardApostrophe); // Single high-reversed-9 quotation mark ‛
        
        return result;
    }

    public void OnItemPickedUp(string itemName)
    {
        if (currentStageIndex < 0 || currentStageIndex >= stages.Length)
        {
            return;
        }

        ObjectiveStage currentStage = stages[currentStageIndex];
        string normalizedItemName = NormalizeString(itemName);
        
        foreach (var objective in currentStage.objectives)
        {
            string normalizedDescription = NormalizeString(objective.description);
            
            if (normalizedDescription.Equals(normalizedItemName, System.StringComparison.OrdinalIgnoreCase) && !objective.isCompleted)
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
            StartStageTransition();
        }
        else
        {
            UpdateObjectiveText();
        }
    }


    public void OnPieceCollected(int piecesCollected)
    {
        if (SceneManager.GetActiveScene().name != "WhisperEndGame") {
            if (stages == null) return; // Safety check - stages not initialized yet
            
            // Search through ALL stages to find the "Collect Story Pieces" objective
            // (not just currentStageIndex, because stories might be collected before reaching that stage)
            int storyStageIndex = -1;
            Objective targetObjective = null;
            
            for (int i = 0; i < stages.Length; i++)
            {
                foreach (var objective in stages[i].objectives)
                {
                    if (objective.description.Contains("Collect Story Pieces"))
                    {
                        storyStageIndex = i;
                        targetObjective = objective;
                        break;
                    }
                }
                if (targetObjective != null) break;
            }
            
            if (targetObjective != null)
            {
                targetObjective.description = $"Collect Story Pieces {piecesCollected}/{totalPieces}";

                if (piecesCollected >= totalPieces)
                {
                    targetObjective.isCompleted = true;
                    // Only trigger stage transition if we're currently on the story pieces stage
                    if (currentStageIndex == storyStageIndex)
                    {
                        StartStageTransition();
                        return; // HandleStageTransition will update the text
                    }
                }
                // Update the text if we're currently viewing this stage
                if (currentStageIndex == storyStageIndex)
                {
                    UpdateObjectiveText();
                }
            }

            // If we completed story pieces early (or during another stage's transition delay),
            // we may later arrive at the story stage already completed. Ensure we don't get stuck.
            AutoAdvanceIfCurrentStageAlreadyCompleted();
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

    private void StartStageTransition()
    {
        if (stageTransitionCoroutine != null) return;
        stageTransitionCoroutine = StartCoroutine(HandleStageTransition());
    }

    private void AutoAdvanceIfCurrentStageAlreadyCompleted()
    {
        if (stages == null) return;
        if (currentStageIndex < 0 || currentStageIndex >= stages.Length) return;

        // If current stage is already completed (e.g. story pieces collected earlier),
        // automatically proceed so we don't get stuck waiting for an OnItemPickedUp call.
        if (stages[currentStageIndex].IsStageCompleted())
        {
            StartStageTransition();
        }
    }

    IEnumerator HandleStageTransition()
    {
        UpdateObjectiveText();
        yield return new WaitForSeconds(stageTransitionDelay);

        // Advance to next stage, and if the next stage is already completed (e.g. story pieces),
        // keep advancing to avoid getting stuck.
        while (true)
        {
            currentStageIndex++;

            if (currentStageIndex >= stages.Length)
            {
                objectiveText.text = "Objectives completed!";
                yield return new WaitForSeconds(stageTransitionDelay);
                objectiveText.text = " ";
                break;
            }

            // Index 6 is "Find Evidence for the Secret Room" stage
            if (currentStageIndex == 6)
            {
                // Start UV text controllers mission when entering this stage
                foreach (var controller in uvTextControllers)
                {
                    if (controller != null)
                    {
                        controller.StartMission();
                    }
                }
            }

            UpdateObjectiveText();

            // If this stage isn't already completed, stop here.
            if (!stages[currentStageIndex].IsStageCompleted())
            {
                break;
            }

            // Otherwise, immediately continue to the next stage (no extra delay).
        }

        stageTransitionCoroutine = null;
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
