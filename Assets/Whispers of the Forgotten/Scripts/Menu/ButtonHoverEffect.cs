using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
    private Vector3 originalScale;
    public float transitionDuration = 0.1f;

    public Color hoverColor = new Color(1, 0.3537736f, 0.3537736f);
    private Color originalColor;

    private Text buttonText;

    void Start()
    {
        originalScale = transform.localScale;

        buttonText = GetComponentInChildren<Text>();

        if (buttonText != null)
        {
            originalColor = buttonText.color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(hoverScale));

        if (buttonText != null)
        {
            buttonText.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleTo(originalScale));

        if (buttonText != null)
        {
            buttonText.color = originalColor;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StopAllCoroutines();
        transform.localScale = originalScale;

        if (buttonText != null)
        {
            buttonText.color = originalColor;
        }
    }

    private IEnumerator ScaleTo(Vector3 targetScale)
    {
        Vector3 currentScale = transform.localScale;
        float timer = 0;

        while (timer < transitionDuration)
        {
            timer += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(currentScale, targetScale, timer / transitionDuration);
            yield return null;
        }

        transform.localScale = targetScale;
    }

}
