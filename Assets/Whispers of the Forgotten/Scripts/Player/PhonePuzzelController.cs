using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PhonePuzzelController : MonoBehaviour
{
    public Hints hints;
    [Tooltip("The hand texture to display in the center of the screen")]
    public Texture2D handTexture;
    [Tooltip("Scale factor for the hand texture (0.1 = 10% of original size)")]
    public float handTextureScale = 0.05f;
    public SubtitleManager subtitleManager;
    public GameObject secretDoor;
    public GameObject keypadPanel;
    public float interactDistance = 1f;
    public AudioSource buttonSoundEffect;
    public AudioSource heavyObjectPushedSoundEffect;
    public LayerMask itemLayerMask;
    [Tooltip("Layer mask for obstructions (walls, etc.) that block raycast. Leave as 'Nothing' to use everything except itemLayerMask.")]
    public LayerMask obstructionLayer;
    
    [Header("End Game Transition")]
    [Tooltip("Black screen image for fade transition")]
    public Image transitionImage;
    [Tooltip("Duration of the fade to black transition")]
    public float transitionDuration = 2.0f;
    [Tooltip("Name of the scene to load after correct code (Thank you screen)")]
    public string endGameSceneName = "ThankYouScene";

    [HideInInspector]
    public int wrongAnswers = 0;

    private string correctCode = "0906";
    private string inputCode = "";
    private bool isNearPhone = false;
    private bool isPanelActive = false;
    public bool isCodeCorrect = false;
    private bool canTogglePanel = true;
    
    [Header("Scene Transition Flag")]
    [Tooltip("Set to true when transitioning to end game scene. Can be checked by other scripts for UI changes.")]
    public static bool isTransitioningToEndGame = false;

    void Start()
    {
        // Reset transition flag when scene loads
        isTransitioningToEndGame = false;
        
        keypadPanel.SetActive(false);
        LockCursor(true);
        itemLayerMask = LayerMask.GetMask("HiddenObjects");
        
        // Initialize transition image
        if (transitionImage != null)
        {
            transitionImage.color = new Color(0f, 0f, 0f, 0f);
            RectTransform rectTransform = transitionImage.rectTransform;
            rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
            rectTransform.anchoredPosition = Vector2.zero;
            transitionImage.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (isCodeCorrect)
        {
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);

        if (Physics.Raycast(ray, out hit, interactDistance, itemLayerMask))
        {
            // Check for obstructions between camera and hit point
            Vector3 directionToHit = hit.point - Camera.main.transform.position;
            float distanceToHit = Vector3.Distance(Camera.main.transform.position, hit.point);
            RaycastHit obstructionHit;
            
            // Use obstructionLayer if set, otherwise use everything except itemLayerMask
            LayerMask obstructionCheckLayer = obstructionLayer.value != 0 ? obstructionLayer : ~itemLayerMask;
            
            if (Physics.Raycast(Camera.main.transform.position, directionToHit.normalized, out obstructionHit, distanceToHit, obstructionCheckLayer))
            {
                // Check if the obstruction is the target object itself
                if (obstructionHit.collider != hit.collider)
                {
                    // There's an obstruction blocking the view
                    if (isNearPhone)
                    {
                        isNearPhone = false;
                    }
                    return;
                }
            }
            
            if (hit.collider.CompareTag("Phone"))
            {
                subtitleManager.ShowSubtitle("I need four digit code.");
                isNearPhone = true;

                if (Input.GetKeyDown(KeyCode.E) && canTogglePanel)
                {
                    ToggleKeypadPanel();
                    StartCoroutine(PreventImmediateToggle());
                }

                return;
            }
        }

        if (isNearPhone)
        {
            isNearPhone = false;
        }
    }

    void OnGUI()
    {
        // Only show hand texture when near phone
        if (handTexture != null && isNearPhone)
        {
            // Calculate scaled size
            float scaledWidth = handTexture.width * handTextureScale;
            float scaledHeight = handTexture.height * handTextureScale;
            
            // Calculate center position of the screen
            float x = (Screen.width - scaledWidth) * 0.5f;
            float y = (Screen.height - scaledHeight) * 0.5f;
            
            // Draw the hand texture at the center with scaled size
            GUI.DrawTexture(new Rect(x, y, scaledWidth, scaledHeight), handTexture);
        }
    }

    public void AddDigit(string digit)
    {
        if (inputCode.Length < 4)
        {
            inputCode += digit;
            buttonSoundEffect.enabled = true;
            buttonSoundEffect.Play();
            if (inputCode.Length == 4)
            {
                CheckCode();
            }
        }
    }

    public void CheckCode()
    {
        if (inputCode == correctCode)
        {
            OpenDoor();
            isCodeCorrect = true;
            CloseKeypadPanel();
            // Start fade to black and scene transition after door opens
            StartCoroutine(TransitionToEndGame());
        }
        else
        {
            inputCode = "";
            CloseKeypadPanel();
            wrongAnswers += 1;
        }
    }

    void OpenDoor()
    {
        heavyObjectPushedSoundEffect.enabled = true;
        heavyObjectPushedSoundEffect.Play();

        float currentYRotation = secretDoor.transform.rotation.eulerAngles.y;

        float targetAngle = currentYRotation + 45f;

        StartCoroutine(SmoothRotateDoor(targetAngle, 2f));
    }

    IEnumerator SmoothRotateDoor(float targetAngle, float duration)
    {
        Quaternion initialRotation = secretDoor.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            secretDoor.transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        secretDoor.transform.rotation = targetRotation;
    }

    void ToggleKeypadPanel()
    {
        isPanelActive = !isPanelActive;
        keypadPanel.SetActive(isPanelActive);

        if (isPanelActive)
        {
            LockCursor(false);
            Time.timeScale = 0f;
        }
        else
        {
            LockCursor(true);
        }
    }

    void CloseKeypadPanel()
    {
        isPanelActive = false;
        keypadPanel.SetActive(false);
        LockCursor(true);
        Time.timeScale = 1f;
    }

    void LockCursor(bool isLocked)
    {
        if (isLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    IEnumerator PreventImmediateToggle()
    {
        canTogglePanel = false;
        yield return new WaitForSeconds(0.5f);
        canTogglePanel = true;
    }
    
    IEnumerator TransitionToEndGame()
    {
        // Set flag to true when starting transition - other scripts can check this
        isTransitioningToEndGame = true;
        
        // Wait a bit for the door opening animation to be visible
        yield return new WaitForSeconds(1.0f);
        
        // Fade to black
        if (transitionImage != null)
        {
            float timer = 0f;
            Color initialColor = new Color(0f, 0f, 0f, 0f);
            Color finalColor = new Color(0f, 0f, 0f, 1f);

            while (timer < transitionDuration)
            {
                timer += Time.deltaTime;
                float t = Mathf.Clamp01(timer / transitionDuration);
                transitionImage.color = Color.Lerp(initialColor, finalColor, t);
                yield return null;
            }

            transitionImage.color = finalColor;
        }
        
        // Wait a moment on black screen before loading scene
        yield return new WaitForSeconds(0.5f);
        
        // Load the end game scene
        if (!string.IsNullOrEmpty(endGameSceneName))
        {
            SceneManager.LoadScene(endGameSceneName);
        }
        else
        {
        }
    }
}
