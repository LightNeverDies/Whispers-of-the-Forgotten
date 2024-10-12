using System.Collections;
using UnityEngine;

public class GameEventsController : MonoBehaviour
{
    [Header("Common Settings")]
    public GameObject triggerZone;
    public GameObject gameObj;

    [Header("Fall Event Settings")]
    public AudioSource fallObjectSoundEffect;
    public Vector3 fallTargetPosition;
    public Quaternion fallTargetRotation;
    public float fallDuration = 1.0f;

    [Header("Throw Event Settings")]
    public Vector3 throwTargetPosition;
    public float throwDuration = 1.0f;
    public float throwHeight = 2.0f;
    public AudioSource throwSoundEffect;

    [Header("Move Event Settings")]
    public Vector3 moveTargetPosition;
    public float moveDuration = 2.0f;
    public AudioSource moveSoundEffect;

    public enum GameEventsState { FallObjects, ThrowObjects, MovingObjects }
    public GameEventsState currentEventState;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Example of random event triggering
            TriggerRandomEvent();
        }
    }


    public void TriggerFallEvent()
    {
        currentEventState = GameEventsState.FallObjects;
        StartCoroutine(FallEvent());
    }

    public void TriggerThrowEvent()
    {
        currentEventState = GameEventsState.ThrowObjects;
        StartCoroutine(ThrowEvent());
    }

    public void TriggerMoveEvent()
    {
        currentEventState = GameEventsState.MovingObjects;
        StartCoroutine(MoveEvent());
    }

    public void TriggerRandomEvent()
    {
        Debug.Log(currentEventState);
        switch (currentEventState)
        {
            case GameEventsState.FallObjects:
                StartCoroutine(FallEvent());
                break;
            case GameEventsState.ThrowObjects:
                StartCoroutine(ThrowEvent());
                break;
            case GameEventsState.MovingObjects:
                StartCoroutine(MoveEvent());
                break;
        }
    }
    private IEnumerator FallEvent()
    {
        PlaySoundEffect(fallObjectSoundEffect);

        Vector3 initialPosition = gameObj.transform.position;
        Quaternion initialRotation = gameObj.transform.rotation;

        float elapsedTime = 0f;

        while (elapsedTime < fallDuration)
        {
            float t = elapsedTime / fallDuration;

            gameObj.transform.position = Vector3.Lerp(initialPosition, fallTargetPosition, t);
            gameObj.transform.rotation = Quaternion.Lerp(initialRotation, fallTargetRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        gameObj.transform.position = fallTargetPosition;
        gameObj.transform.rotation = fallTargetRotation;

        triggerZone.SetActive(false);
    }


    private IEnumerator ThrowEvent()
    {
        PlaySoundEffect(throwSoundEffect);

        Vector3 startPosition = gameObj.transform.position;
        Vector3 endPosition = throwTargetPosition;

        float elapsedTime = 0f;

        while (elapsedTime < throwDuration)
        {
            float t = elapsedTime / throwDuration;

            // Parabolic trajectory calculation
            Vector3 currentPosition = Vector3.Lerp(startPosition, endPosition, t);
            currentPosition.y += throwHeight * Mathf.Sin(Mathf.PI * t);

            gameObj.transform.position = currentPosition;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        gameObj.transform.position = endPosition;

        triggerZone.SetActive(false);
    }

    private IEnumerator MoveEvent()
    {
        PlaySoundEffect(moveSoundEffect);

        Vector3 initialPosition = gameObj.transform.position;
        Vector3 targetPosition = moveTargetPosition;

        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            float t = elapsedTime / moveDuration;
            gameObj.transform.position = Vector3.Lerp(initialPosition, targetPosition, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        gameObj.transform.position = targetPosition;

        triggerZone.SetActive(false);
    }

    private void PlaySoundEffect(AudioSource audioSource)
    {
        if (audioSource != null)
        {
            audioSource.enabled = true;
            audioSource.Play();
        }
    }

}