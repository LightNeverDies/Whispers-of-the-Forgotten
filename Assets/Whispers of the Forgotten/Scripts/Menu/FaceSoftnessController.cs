using UnityEngine;
using TMPro;
using System.Collections;

public class FaceSoftnessController : MonoBehaviour
{
    public TMP_Text textMeshPro; // Reference to the TextMeshPro component
    public float duration = 3.0f; // Duration of the transition in seconds

    private Material textMaterial; // Material of the TMP object
    private Coroutine transitionCoroutine; // Reference to the current coroutine

    void Start()
    {
        // Get the material of the TMP object
        textMaterial = textMeshPro.fontMaterial;

        // Ensure material is assigned
        if (textMaterial == null)
        {
            Debug.LogError("TextMesh Pro material is not assigned or found.");
            return;
        }

        // Start the transition coroutine
        StartTransition();
    }

    void StartTransition()
    {
        // Start the transition coroutine if it's not already running
        if (transitionCoroutine == null)
        {
            transitionCoroutine = StartCoroutine(LoopTransitionFaceSoftness(0.1f, 0.6f, duration));
        }
    }

    private IEnumerator LoopTransitionFaceSoftness(float startSoftness, float endSoftness, float transitionDuration)
    {
        while (true)
        {
            // Transition from startSoftness to endSoftness
            yield return StartCoroutine(TransitionFaceSoftness(startSoftness, endSoftness, transitionDuration));

            // Transition back from endSoftness to startSoftness
            yield return StartCoroutine(TransitionFaceSoftness(endSoftness, startSoftness, transitionDuration));
        }
    }

    private IEnumerator TransitionFaceSoftness(float startSoftness, float endSoftness, float transitionDuration)
    {
        float elapsedTime = 0.0f;

        while (elapsedTime < transitionDuration)
        {
            float currentSoftness = Mathf.Lerp(startSoftness, endSoftness, elapsedTime / transitionDuration);
            textMaterial.SetFloat("_FaceDilate", currentSoftness);
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        // Ensure the final value is set
        textMaterial.SetFloat("_FaceDilate", endSoftness);
    }
}
