using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionController : MonoBehaviour
{
    public Image transitionImage;  // Панелът за преход
    public float transitionDuration = 5.0f; // Продължителност на прехода
    public string sceneToLoad;          // Името на сцената, която ще бъде заредена
    public GameObject sceneTransition;
    
    private void Start()
    {
        // Уверяваме се, че панелът е видим и изцяло покрива екрана
        //sceneTransition.SetActive(false);
        transitionImage.color = new Color(transitionImage.color.r, transitionImage.color.g, transitionImage.color.b, 0f);
        RectTransform rectTransform = transitionImage.rectTransform;
        rectTransform.sizeDelta = new Vector2(Screen.width * 2, Screen.height * 2); // Начален размер на панела
        rectTransform.anchoredPosition = Vector2.zero; // Центриран панел
    }

    public IEnumerator ExpandPanel()
    {
        sceneTransition.SetActive(true);
        RectTransform rectTransform = transitionImage.rectTransform;
        Vector2 targetSize = new Vector2(Screen.width, Screen.height); // Финален размер на панела

        float timer = 0f;
        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = timer / transitionDuration;

            // Плавно намаляване на размера на панела и увеличаване на прозрачността
            rectTransform.sizeDelta = Vector2.Lerp(new Vector2(Screen.width * 2, Screen.height * 2), targetSize, t);
            transitionImage.color = new Color(transitionImage.color.r, transitionImage.color.g, transitionImage.color.b, t);

            yield return null;
        }

        // След края на прехода, зареждаме новата сцена
        StartCoroutine(FadeToScene());
    }

    private IEnumerator FadeToScene()
    {
        float timer = 0f;
        Color originalColor = transitionImage.color;

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = timer / transitionDuration;
            transitionImage.color = Color.Lerp(originalColor, Color.white, t);
            yield return null;
        }

        // Зареждаме новата сцена
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
    }
}
