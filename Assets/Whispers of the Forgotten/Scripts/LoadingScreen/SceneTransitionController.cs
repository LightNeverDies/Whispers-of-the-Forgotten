using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneTransitionController : MonoBehaviour
{
    public Image transitionImage;  // ������� �� ������
    public float transitionDuration = 5.0f; // ��������������� �� �������
    public string sceneToLoad;          // ����� �� �������, ����� �� ���� ��������
    public GameObject sceneTransition;
    
    private void Start()
    {
        // ��������� ��, �� ������� � ����� � ������ ������� ������
        //sceneTransition.SetActive(false);
        transitionImage.color = new Color(transitionImage.color.r, transitionImage.color.g, transitionImage.color.b, 0f);
        RectTransform rectTransform = transitionImage.rectTransform;
        rectTransform.sizeDelta = new Vector2(Screen.width * 2, Screen.height * 2); // ������� ������ �� ������
        rectTransform.anchoredPosition = Vector2.zero; // ��������� �����
    }

    public IEnumerator ExpandPanel()
    {
        sceneTransition.SetActive(true);
        RectTransform rectTransform = transitionImage.rectTransform;
        Vector2 targetSize = new Vector2(Screen.width, Screen.height); // ������� ������ �� ������

        float timer = 0f;
        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = timer / transitionDuration;

            // ������ ���������� �� ������� �� ������ � ����������� �� �������������
            rectTransform.sizeDelta = Vector2.Lerp(new Vector2(Screen.width * 2, Screen.height * 2), targetSize, t);
            transitionImage.color = new Color(transitionImage.color.r, transitionImage.color.g, transitionImage.color.b, t);

            yield return null;
        }

        // ���� ���� �� �������, ��������� ������ �����
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

        // ��������� ������ �����
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
    }
}
