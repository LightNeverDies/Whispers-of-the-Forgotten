using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuTransitionController : MonoBehaviour
{
    [Header("Transition Settings")]
    public Image transitionImage;
    public float transitionDuration = 1.0f;
    
    [Header("Scene Settings")]
    public string sceneToLoad = "Menu";
    public string transitionMessage = "";
    
    [Header("Debug")]
    public bool debugLog = false;
    
    private void Start()
    {
        if (transitionImage != null)
        {
            transitionImage.color = new Color(transitionImage.color.r, transitionImage.color.g, transitionImage.color.b, 0f);
            RectTransform rectTransform = transitionImage.rectTransform;
            rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
            rectTransform.anchoredPosition = Vector2.zero;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartCoroutine(TransitionToMenu());
        }
    }
    
    public void TriggerTransition()
    {
        StartCoroutine(TransitionToMenu());
    }
    
    IEnumerator TransitionToMenu()
    {
        // Store the message in PlayerPrefs so the menu can access it
        if (!string.IsNullOrEmpty(transitionMessage))
        {
            PlayerPrefs.SetString("TransitionMessage", transitionMessage);
            PlayerPrefs.Save();
        }
        
        // Fade to black
        float timer = 0f;
        Color initialColor = new Color(transitionImage.color.r, transitionImage.color.g, transitionImage.color.b, 0f);
        Color finalColor = new Color(transitionImage.color.r, transitionImage.color.g, transitionImage.color.b, 1f);
        
        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / transitionDuration);
            
            if (transitionImage != null)
            {
                transitionImage.color = Color.Lerp(initialColor, finalColor, t);
            }
            
            yield return null;
        }
        
        if (transitionImage != null)
        {
            transitionImage.color = finalColor;
        }
        
        yield return new WaitForSeconds(transitionDuration);
        
        // Load the scene
        SceneManager.LoadScene(sceneToLoad);
    }
}

