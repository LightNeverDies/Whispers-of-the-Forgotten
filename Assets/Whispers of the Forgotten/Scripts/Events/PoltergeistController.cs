using Interfaces;
using System.Collections;
using UnityEngine;

public class PoltergeistEffect : MonoBehaviour, IEventEffect
{
    public Transform spawnPoint;
    public Transform destinationPoint;
    public GameObject poltergeistPrefab;
    public float speed = 2.5f;

    public FlashlightController flashlightController;
    public AudioSource poltergeistSoundEffect;

    private GameObject currentPoltergeist;
    private bool isRunning = false;

    public void StartEffect()
    {
        if (isRunning) return;
        isRunning = true;
        StartCoroutine(SpawnAndMovePoltergeist());
    }

    public void StopEffect()
    {
        if (currentPoltergeist != null)
        {
            Destroy(currentPoltergeist);
            currentPoltergeist = null;
        }
        flashlightController.isEventGoing = false;
        poltergeistSoundEffect.Stop();
        isRunning = false;
    }

    private IEnumerator SpawnAndMovePoltergeist()
    {
        currentPoltergeist = Instantiate(poltergeistPrefab, spawnPoint.position, Quaternion.identity);
        flashlightController.isEventGoing = true;
        poltergeistSoundEffect.Play();

        Animator animator = currentPoltergeist.GetComponent<Animator>();
        if (animator) animator.SetBool("isWalking", true);

        while (Vector3.Distance(currentPoltergeist.transform.position, destinationPoint.position) > 0.1f)
        {
            currentPoltergeist.transform.position = Vector3.MoveTowards(
                currentPoltergeist.transform.position,
                destinationPoint.position,
                Time.deltaTime * speed);

            Vector3 direction = (destinationPoint.position - currentPoltergeist.transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                currentPoltergeist.transform.rotation = Quaternion.Slerp(
                    currentPoltergeist.transform.rotation,
                    lookRotation,
                    Time.deltaTime * 10f);
            }

            yield return null;
        }

        if (animator) animator.SetBool("isWalking", false);

        Destroy(currentPoltergeist);
        currentPoltergeist = null;

        flashlightController.isEventGoing = false;
        poltergeistSoundEffect.Stop();
        isRunning = false;
    }
}
