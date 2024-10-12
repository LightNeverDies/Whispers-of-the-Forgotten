using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhonePuzzelController : MonoBehaviour
{
    public Hints hints;
    public SubtitleManager subtitleManager;
    public GameObject secretDoor;
    public GameObject keypadPanel;
    public float interactDistance = 1f;
    public AudioSource buttonSoundEffect;
    public AudioSource heavyObjectPushedSoundEffect;
    public LayerMask itemLayerMask;

    [HideInInspector]
    public int wrongAnswers = 0;

    private string correctCode = "1304";
    private string inputCode = "";
    private bool isNearPhone = false;
    private bool isPanelActive = false;
    public bool isCodeCorrect = false;
    private bool canTogglePanel = true;

    void Start()
    {
        keypadPanel.SetActive(false);
        LockCursor(true);
        itemLayerMask = LayerMask.GetMask("HiddenObjects");
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
            if (hit.collider.CompareTag("Phone"))
            {
                hints.ShowHint("Press E to Interact.");
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
            hints.HideHint();
            isNearPhone = false;
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
        hints.HideHint();
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
}
