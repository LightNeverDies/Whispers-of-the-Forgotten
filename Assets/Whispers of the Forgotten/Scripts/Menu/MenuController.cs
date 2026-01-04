using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuController : MonoBehaviour
{
    public GameObject mainPanel;
    public GameObject thankYouForPlayingText;
    PhonePuzzelController phonePuzzelController = new PhonePuzzelController(); // To check if the game is transitioning to the end game scene "DEMO"

    private TMP_Text textComponent;
    private float colorSpeed = 0.3f; // Speed of color change (slower = more visible colors)

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        thankYouForPlayingText.SetActive(false);
        
        // Get the TextMeshPro component
        textComponent = thankYouForPlayingText.GetComponent<TMP_Text>();
        if (textComponent == null)
        {
            textComponent = thankYouForPlayingText.GetComponentInChildren<TMP_Text>();
        }
    }

    private void Update()
    {
        if(PhonePuzzelController.isTransitioningToEndGame)
        {
            thankYouForPlayingText.SetActive(true);
        }
        
        // Update RGB colors for each letter
        if (textComponent != null)
        {
            UpdateTextColors();
        }
    }
    
    private void UpdateTextColors()
    {
        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;
        
        if (textInfo.characterCount == 0) return;
        
        // First pass: count visible characters
        int visibleCharCount = 0;
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (textInfo.characterInfo[i].isVisible)
                visibleCharCount++;
        }
        
        if (visibleCharCount == 0) return;
        
        float time = Time.time * colorSpeed;
        int visibleCharIndex = 0;
        
        // Second pass: apply colors to visible characters
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            
            // Skip invisible characters (spaces, etc.)
            if (!charInfo.isVisible) continue;
            
            // Calculate hue: spread letters evenly across full spectrum (0-1)
            // Each letter gets a portion of the spectrum, then animates through it
            float letterOffset = (float)visibleCharIndex / Mathf.Max(1, visibleCharCount);
            float hue = (time + letterOffset) % 1f; // Full spectrum coverage (0-1)
            
            // Convert HSV to RGB for rainbow effect (full saturation and value for vibrant colors)
            Color32 letterColor = Color.HSVToRGB(hue, 1f, 1f);
            
            // Get the mesh info for this character
            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            
            Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;
            
            // Apply color to all 4 vertices of the character quad
            vertexColors[vertexIndex + 0] = letterColor;
            vertexColors[vertexIndex + 1] = letterColor;
            vertexColors[vertexIndex + 2] = letterColor;
            vertexColors[vertexIndex + 3] = letterColor;
            
            visibleCharIndex++;
        }
        
        // Update the mesh with new colors (more efficient method)
        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("LoadingScreenHints");
    }

    public void BackToMainMenu()
    {
        mainPanel.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
