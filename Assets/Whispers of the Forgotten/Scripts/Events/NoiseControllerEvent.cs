using UnityEngine;

public class NoiseControllerEvent : MonoBehaviour
{
    public GameObject gameObjectTrigger;
    public AudioSource spookySound;

    public EventSoundTriggerGlobalController EventSoundTriggerInterface;

    private void OnTriggerEnter(Collider other)
    {
      if (other.CompareTag("Player"))
      {
         EventSoundTriggerInterface.TriggerZone(spookySound, gameObjectTrigger);
      }
    }
}
