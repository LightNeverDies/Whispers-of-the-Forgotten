using System.Collections;
using UnityEngine;

// I should fix while opening to hide the message open again and etc.... because the player can spam it

public class OpenJewaryBox : MonoBehaviour
{
    public float interactDistance = 1.0f;
    public LayerMask interactionLayer;
    public Hints hints;
    public float rotationTolerance = 20.0f;

    private bool isNearJewaryBox = false;
    private bool isOpening = false;
    private bool isAnimationDone = false;
    private bool hideHint = false;

    public AudioSource jewaryBoxOpenSoundEffect;
    public GameObject jewaryBoxTop;
    public GameObject secretObject;


    void Update()
    {
        HandleOpenJewaryBox();
    }

    public void TriggerEventJewaryBox()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }

        boxCollider.isTrigger = true;

        boxCollider.size = new Vector3(0.1f, 0.1f, 0.1f);
        boxCollider.center = new Vector3(0f, 0f, 0f);

    }

    public void HandleOpenJewaryBox()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if(isAnimationDone) {
            BoxCollider boxColliderJewaryBox = GetComponent<BoxCollider>();

            if (boxColliderJewaryBox)
            {
                boxColliderJewaryBox.enabled = false;
            }

            BoxCollider secretObjectCollider = secretObject.GetComponent<BoxCollider>();
            
            if (secretObjectCollider != null)
            {
                secretObjectCollider.enabled = true;
            }

            isAnimationDone = false;
            return;
        }

        if(isOpening == true)
        {
            hints.HideHint();
            isOpening = false;
            return;
        }

        if(Physics.Raycast(ray, out hit, interactDistance, interactionLayer))
        {
            if(hit.collider.CompareTag("JewaryBox"))
            {
                isNearJewaryBox = true;
                if(!hideHint)
                {
                    hints.ShowHint("Press E to Open");
                }


                if (Input.GetKeyDown(KeyCode.E))
                {
                    isOpening = true;
                    OpenJewaryBoxAnimation();
                }

                return;
            }
        }
        
        if(isNearJewaryBox)
        {
            hints.HideHint();
            isNearJewaryBox = false;
        }
    }

    public void OpenJewaryBoxAnimation()
    {
        hideHint = true;
        StartCoroutine(RotateLid());
    }

    private IEnumerator RotateLid()
    {
        float currentRotationZ = jewaryBoxTop.transform.localEulerAngles.z;
        float targetRotationZ = 90.0f;
        float initialRotationZ = currentRotationZ;

        if(!jewaryBoxOpenSoundEffect.isPlaying && jewaryBoxOpenSoundEffect.enabled == false)
        {
            jewaryBoxOpenSoundEffect.enabled = true;
            jewaryBoxOpenSoundEffect.Play();
        }


        while (currentRotationZ < targetRotationZ)
        {
            // Rotate incrementally over time
            currentRotationZ = Mathf.MoveTowards(currentRotationZ, targetRotationZ, rotationTolerance * Time.deltaTime);
            jewaryBoxTop.transform.localEulerAngles = new Vector3(
                jewaryBoxTop.transform.localEulerAngles.x,
                jewaryBoxTop.transform.localEulerAngles.y,
                currentRotationZ
            );

            yield return null; // Wait for the next frame
        }

        // Ensure the final rotation is exactly 90 degrees
        jewaryBoxTop.transform.localEulerAngles = new Vector3(
            jewaryBoxTop.transform.localEulerAngles.x,
            jewaryBoxTop.transform.localEulerAngles.y,
            targetRotationZ
        );

        isAnimationDone = true;
    }
}