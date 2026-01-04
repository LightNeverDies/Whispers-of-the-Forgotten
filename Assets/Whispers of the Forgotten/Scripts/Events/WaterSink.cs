using Interfaces;
using UnityEngine;

public class WaterSink : MonoBehaviour, IInteractableHint
{
    public WaterBathroom waterBathroom;
    public AudioSource waterSink;
    public ParticleSystem waterFlow;
    public LayerMask interactionLayer;
    [Tooltip("Layer mask for obstructions (walls, etc.) that block raycast. Leave as 'Nothing' to use everything except interactionLayer.")]
    public LayerMask obstructionLayer;
    public GameObject waterFromSink;

    public Hints hints;
    [Tooltip("The hand texture to display in the center of the screen")]
    public Texture2D handTexture;
    [Tooltip("Scale factor for the hand texture (0.1 = 10% of original size)")]
    public float handTextureScale = 0.05f;
    public float interactDistance = 1f;
    public Camera mainCamera;

    private bool isRunning = false;
    private bool isPlayerNear = false;

    public bool IsInteractableNear => isPlayerNear;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (waterBathroom != null && waterBathroom.isDraining)
        {
            StartSink();
        }
    }

    void Update()
    {
        if (!isRunning) return;

        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactionLayer))
        {
            // Check for obstructions between camera and hit point
            Vector3 directionToHit = hit.point - mainCamera.transform.position;
            float distanceToHit = Vector3.Distance(mainCamera.transform.position, hit.point);
            RaycastHit obstructionHit;
            
            // Use obstructionLayer if set, otherwise use everything except interactionLayer
            LayerMask obstructionCheckLayer = obstructionLayer.value != 0 ? obstructionLayer : ~interactionLayer;
            
            if (Physics.Raycast(mainCamera.transform.position, directionToHit.normalized, out obstructionHit, distanceToHit, obstructionCheckLayer))
            {
                // Check if the obstruction is the target object itself
                if (obstructionHit.collider != hit.collider)
                {
                    // There's an obstruction blocking the view
                    if (isPlayerNear)
                    {
                        isPlayerNear = false;
                    }
                    return;
                }
            }
            
            if (hit.collider.CompareTag("Sink"))
            {
                isPlayerNear = true;

                if (Input.GetKeyDown(KeyCode.E) && isPlayerNear)
                {
                    StopSink();
                }

                return;
            }
        } else {
            isPlayerNear = false;
        }
    }

    void OnGUI()
    {
        // Only show hand texture when near sink and running
        if (Event.current.type == EventType.Repaint && handTexture != null && isPlayerNear && isRunning)
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

    public void StartSink()
    {
        waterFlow.Play();
        if (waterSink != null) waterSink.Play();
        isRunning = true;
        waterFromSink.SetActive(true);
    }

    public void StopSink()
    {
        waterFlow.Stop();
        if (waterSink != null) waterSink.Stop();
        isRunning = false;
        waterFromSink.SetActive(false);
        isPlayerNear = false;
    }
}
