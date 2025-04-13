using System.Collections;
using UnityEngine;

public class NoiseControllerEvent : MonoBehaviour
{
    public GameObject gameObjectTrigger;
    public AudioSource spookySound;

    public EventSoundTriggerGlobalController EventSoundTriggerInterface;

    private bool isTriggered = false;
    private float cooldownTime = 10f;
    private float lastTriggeredTime = -Mathf.Infinity;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTriggered && Time.time >= lastTriggeredTime + cooldownTime)
        {
            isTriggered = true;
            lastTriggeredTime = Time.time;
            EventSoundTriggerInterface.TriggerZone(spookySound, gameObjectTrigger);

            StartCoroutine(ResetTriggerAfterCooldown());
        }
    }

    private IEnumerator ResetTriggerAfterCooldown()
    {
        yield return new WaitForSeconds(cooldownTime);
        isTriggered = false;
    }

}
