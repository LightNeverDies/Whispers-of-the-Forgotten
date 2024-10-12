using System.Collections;
using UnityEngine;

public class PoltergeistController : MonoBehaviour
{
    public Transform spawnPoint;
    public Transform destinationPoint;
    public GameObject poltergeistPrefab;

    public FlashlightController flashlightController;

    private GameObject currentPoltergeist;
    private bool isZoneUsed = false;
    public AudioSource poltergiestSoundEffect;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isZoneUsed && currentPoltergeist == null)
        {
            isZoneUsed = true;

            currentPoltergeist = Instantiate(poltergeistPrefab, spawnPoint.position, Quaternion.identity);

            StartCoroutine(MoveToDestination(currentPoltergeist, destinationPoint.position));

            flashlightController.isEventGoing = true;
            poltergiestSoundEffect.enabled = true;

            poltergiestSoundEffect.Play();
        }
    }

    IEnumerator MoveToDestination(GameObject poltergeist, Vector3 destination)
    {
        Animator animator = poltergeist.GetComponent<Animator>();
        if (animator)
        {
            animator.SetBool("isWalking", true);

            while (Vector3.Distance(poltergeist.transform.position, destination) > 0.1f)
            {
                poltergeist.transform.position = Vector3.MoveTowards(poltergeist.transform.position, destination, Time.deltaTime * 2f);
                yield return null;
            }

            animator.SetBool("isWalking", false);
            Destroy(poltergeist);

            flashlightController.isEventGoing = false;

            isZoneUsed = true;
            poltergiestSoundEffect.enabled = false;
        }
       
    }
}
