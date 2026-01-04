using UnityEngine;

public class WaterBathroom : MonoBehaviour
{
    public float interactDistance = 1.0f;
    public Hints hints;
    [Tooltip("The hand texture to display in the center of the screen")]
    public Texture2D handTexture;
    [Tooltip("Scale factor for the hand texture (0.1 = 10% of original size)")]
    public float handTextureScale = 0.05f;
    public LayerMask interactionLayer;
    [Tooltip("Layer mask for obstructions (walls, etc.) that block raycast. Leave as 'Nothing' to use everything except interactionLayer.")]
    public LayerMask obstructionLayer;
    private float drainSpeed = 0.2f;
    public GameObject secretObject;

    public WaterSink waterSink;

    private bool isNearWater = false;
    public bool isDraining = false;

    void Update()
    {
        if (isDraining)
        {
            DrainWater();
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactionLayer))
        {
            // Check for obstructions between camera and hit point
            Vector3 camPos = Camera.main.transform.position;
            Vector3 directionToHit = hit.point - camPos;
            float distanceToHit = Vector3.Distance(camPos, hit.point);
            RaycastHit obstructionHit;
            
            // Use obstructionLayer if set, otherwise use everything except interactionLayer
            LayerMask obstructionCheckLayer = obstructionLayer.value != 0 ? obstructionLayer : ~interactionLayer;
            
            if (Physics.Raycast(camPos, directionToHit.normalized, out obstructionHit, distanceToHit, obstructionCheckLayer))
            {
                // Check if the obstruction is the target object itself
                if (obstructionHit.collider != hit.collider)
                {
                    // There's an obstruction blocking the view
                    if (isNearWater)
                    {
                        isNearWater = false;
                    }
                    return;
                }
            }
            
            if (hit.collider.CompareTag("Water"))
            {
                isNearWater = true;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    isDraining = true;
                }
                return;
            }
        }

        if (isNearWater)
        {
            isNearWater = false;
        }
    }

    void OnGUI()
    {
        // Only show hand texture when near water and not draining
        if (Event.current.type == EventType.Repaint && handTexture != null && isNearWater && !isDraining)
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

    void DrainWater()
    {
        if (transform.localScale.z > 0 || transform.localScale.x > 0)
        {
            float newScaleX = transform.localScale.x - drainSpeed * Time.deltaTime;
            float newScaleZ = transform.localScale.z - drainSpeed * Time.deltaTime;
            newScaleX = Mathf.Max(newScaleX, 0);
            newScaleZ = Mathf.Max(newScaleZ, 0);

            transform.localScale = new Vector3(newScaleX, transform.localScale.y, newScaleZ);
        }

        if (transform.position.y > 5.629)
        {
            transform.position -= new Vector3(0, drainSpeed * Time.deltaTime, 0);
        }

        if (transform.localScale.z <= 0 && transform.localScale.x <= 0)
        {
            transform.localScale = new Vector3(0, transform.localScale.y, 0);
            gameObject.SetActive(false);
            isDraining = false;

            BoxCollider secretObjectCollider = secretObject.GetComponent<BoxCollider>();

            if (secretObjectCollider != null)
            {
                secretObjectCollider.enabled = true;
            }

            waterSink.StartSink();
        }
    }
}
