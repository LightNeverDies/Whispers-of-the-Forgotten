using UnityEngine;
using UnityEngine.UI;

public class SubtitleManager : MonoBehaviour
{
    public Text subtitleText;
    public float displayDuration;

    private float timer;
    private bool isDisplaying;

    void Update()
    {
        if (isDisplaying)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                HideSubtitle();
            }
        }
    }

    public void ShowSubtitle(string text)
    {
        // Normalize line endings to ensure newlines display correctly
        string normalizedText = text.Replace("\r\n", "\n").Replace("\r", "\n");
        subtitleText.text = normalizedText;
        subtitleText.gameObject.SetActive(true);
        isDisplaying = true;
        timer = displayDuration;
    }
    
    public void ShowSubtitle(string text, float duration)
    {
        // Normalize line endings to ensure newlines display correctly
        string normalizedText = text.Replace("\r\n", "\n").Replace("\r", "\n");
        subtitleText.text = normalizedText;
        subtitleText.gameObject.SetActive(true);
        isDisplaying = true;
        timer = duration;
    }

    void HideSubtitle()
    {
        subtitleText.gameObject.SetActive(false);
        isDisplaying = false;
    }
}
