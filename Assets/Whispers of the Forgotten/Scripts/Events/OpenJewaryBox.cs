using System.Collections;
using UnityEngine;

public class OpenJewaryBox : MonoBehaviour
{
    public float interactDistance = 1.0f;
    public LayerMask interactionLayer;
    [Tooltip("Layer mask for obstructions (walls, etc.) that block raycast. Leave as 'Nothing' to use everything except interactionLayer.")]
    public LayerMask obstructionLayer;
    public Hints hints;
    [Tooltip("The hand texture to display in the center of the screen")]
    public Texture2D handTexture;
    [Tooltip("Scale factor for the hand texture (0.1 = 10% of original size)")]
    public float handTextureScale = 0.05f;
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
            isOpening = false;
            return;
        }

        if(Physics.Raycast(ray, out hit, interactDistance, interactionLayer))
        {
            // Check for obstructions between camera and hit point
            Vector3 directionToHit = hit.point - Camera.main.transform.position;
            float distanceToHit = Vector3.Distance(Camera.main.transform.position, hit.point);
            RaycastHit obstructionHit;
            
            // Use obstructionLayer if set, otherwise use everything except interactionLayer
            LayerMask obstructionCheckLayer = obstructionLayer.value != 0 ? obstructionLayer : ~interactionLayer;
            
            if (Physics.Raycast(Camera.main.transform.position, directionToHit.normalized, out obstructionHit, distanceToHit, obstructionCheckLayer))
            {
                // Check if the obstruction is the target object itself
                if (obstructionHit.collider != hit.collider)
                {
                    // There's an obstruction blocking the view
                    if (isNearJewaryBox)
                    {
                        isNearJewaryBox = false;
                    }
                    return;
                }
            }
            
            if(hit.collider.CompareTag("JewaryBox"))
            {
                if(!hideHint)
                {
                    isNearJewaryBox = true;
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
            isNearJewaryBox = false;
        }
    }

    void OnGUI()
    {
        // Only show hand texture when near an interactable jewelry box
        if (handTexture != null && isNearJewaryBox && !hideHint)
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