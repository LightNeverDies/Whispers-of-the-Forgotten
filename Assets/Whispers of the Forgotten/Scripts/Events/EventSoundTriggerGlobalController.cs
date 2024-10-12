using System.Collections;
using UnityEngine;

public class EventSoundTriggerGlobalController : MonoBehaviour
{
    public void TriggerSound(AudioSource spookySound, GameObject gameObjectTrigger)
    {
        spookySound.enabled = true;
        spookySound.Play();

        StartCoroutine(CheckIfSoundFinished(spookySound, gameObjectTrigger));
    }

    public void TriggerZone(AudioSource spookySound, GameObject gameObjectTrigger)
    {
        gameObjectTrigger.SetActive(true);
        TriggerSound(spookySound, gameObjectTrigger);
    }

    private IEnumerator CheckIfSoundFinished(AudioSource spookySound, GameObject gameObjectTrigger)
    {
        yield return new WaitUntil(() => !spookySound.isPlaying);
        Destroy(gameObjectTrigger);
    }
}
