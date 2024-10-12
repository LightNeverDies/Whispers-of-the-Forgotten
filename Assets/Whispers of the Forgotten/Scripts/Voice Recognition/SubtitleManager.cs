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
        subtitleText.text = text;
        subtitleText.gameObject.SetActive(true);
        isDisplaying = true;
        timer = displayDuration;
    }

    void HideSubtitle()
    {
        subtitleText.gameObject.SetActive(false);
        isDisplaying = false;
    }
}
