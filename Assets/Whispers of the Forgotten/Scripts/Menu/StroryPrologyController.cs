using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StoryPrologyController : MonoBehaviour
{
    public Button continueButton;           
    public float delay = 3f;              
    public string sceneToLoad;              
    public TextMeshProUGUI textComponent;   
    public float typingSpeed = 0.05f;       

    private string fullText;                
    private string currentText = "";        

    void Start()
    {
        continueButton.gameObject.SetActive(false); 
        fullText = textComponent.text;               
        textComponent.text = "";                     
        StartCoroutine(RevealTextRoutine());         
    }

    IEnumerator RevealTextRoutine()
    {
        for (int i = 0; i < fullText.Length; i++)
        {
            currentText = fullText.Substring(0, i + 1);   
            textComponent.text = currentText;             
            yield return new WaitForSeconds(typingSpeed); 
        }

        continueButton.gameObject.SetActive(true);
        continueButton.onClick.AddListener(OnContinueButtonClicked);
    }

    void OnContinueButtonClicked()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
