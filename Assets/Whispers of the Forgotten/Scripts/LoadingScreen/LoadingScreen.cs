using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public Slider loadingBar;
    public Text loadingText;

    void Start()
    {
        StartCoroutine(LoadGameScene());
    }

    private IEnumerator LoadGameScene()
    {
        float loadTime = 10f; // Set the time for loading (in seconds)
        float elapsedTime = 0f;

        while (elapsedTime < loadTime)
        {
            elapsedTime += Time.deltaTime;
            loadingBar.value = Mathf.Clamp01(elapsedTime / loadTime);
            yield return null; // Wait for the next frame
        }

        // Once the loading is done, load the actual game scene
        SceneManager.LoadScene("Prology");

    }
}
