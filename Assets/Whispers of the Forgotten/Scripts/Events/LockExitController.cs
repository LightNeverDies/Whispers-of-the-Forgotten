using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LockExitController : MonoBehaviour
{
    public GameObject triggerZone;
    public GameObject secretDoor;
    public GameObject poltergiest;
    public AudioSource heavyObjectPushedSoundEffect;
    public AudioSource poltergiestSoundEffect;
    public Image transitionImage;
    public float transitionDuration = 1.0f;

    public GameObject player;
    public CharacterController characterController;
    public Transform newPlayerPosition;
    public Camera playerCamera;
    public Vector3 newCameraRotation = new Vector3(0, 90, 0);

    private bool isPlayerMoved = false;

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


    private void Update()
    {
        if(isPlayerMoved)
        {
            triggerZone.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CloseDoor();
        }
    }

    void CloseDoor()
    {
        if (heavyObjectPushedSoundEffect != null)
        {
            heavyObjectPushedSoundEffect.enabled = true;
            heavyObjectPushedSoundEffect.Play();
        }

        StartCoroutine(SmoothRotateDoor(0f, 2f));

        if (poltergiestSoundEffect != null)
        {
            poltergiestSoundEffect.enabled = true;
            poltergiestSoundEffect.Play();
        }

        StartCoroutine(BlackScreenTransition());
    }

    IEnumerator SmoothRotateDoor(float targetAngle, float duration)
    {
        if (secretDoor == null)
        {
            yield break;
        }

        float startAngle = secretDoor.transform.localEulerAngles.y;
        float elapsedTime = 0f;


        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float currentAngle = Mathf.LerpAngle(startAngle, targetAngle, elapsedTime / duration);
            secretDoor.transform.localEulerAngles = new Vector3(0f, currentAngle, 0f);
            yield return null;
        }

        secretDoor.transform.localEulerAngles = new Vector3(0f, targetAngle, 0f);

        if (poltergiest != null)
        {
            poltergiest.SetActive(false);
        }
    }

    IEnumerator BlackScreenTransition()
    {
        float timer = 0f;
        Color initialColor = new Color(transitionImage.color.r, transitionImage.color.g, transitionImage.color.b, 0f);
        Color finalColor = new Color(transitionImage.color.r, transitionImage.color.g, transitionImage.color.b, 1f);

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / transitionDuration);

            transitionImage.color = Color.Lerp(initialColor, finalColor, t);

            yield return null;
        }


        transitionImage.color = finalColor;

        yield return new WaitForSeconds(transitionDuration);

        MovePlayerAndCamera();

        timer = 0f;

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / transitionDuration);

            transitionImage.color = Color.Lerp(finalColor, initialColor, t);

            yield return null;
        }

        transitionImage.color = initialColor;

        isPlayerMoved = true;
    }

    void MovePlayerAndCamera()
    {

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        if (newPlayerPosition != null)
        {
            player.transform.position = newPlayerPosition.position;
            player.transform.rotation = Quaternion.Euler(0, 90, 0);
        }

        MoveCamera moveCamera = playerCamera.GetComponent<MoveCamera>();

        if (moveCamera != null)
        {
            moveCamera.isOnBed = true;
        }

    }
}
