using UnityEngine;

public class TeleportPlayerInEnd : MonoBehaviour
{
    public float rayDisance = 1f;
    public Hints hints;
    [Tooltip("The hand texture to display in the center of the screen")]
    public Texture2D handTexture;
    [Tooltip("Scale factor for the hand texture (0.1 = 10% of original size)")]
    public float handTextureScale = 0.05f;
    public SubtitleManager subtitleManager;
    public Vector3 teleportPosition;
    public Quaternion teleportRotation;
    public GameObject player;
    public LayerMask itemLayerMask;
    private bool isItemNear = false;

    [HideInInspector]
    public Item currentItem;

    private void Start()
    {
        itemLayerMask = LayerMask.GetMask("Teleport");
    }

    void Update()
    {
        CheckForItem();

        if (Input.GetKeyDown(KeyCode.E) && isItemNear)
        {
            Teleport();
        }
    }

    private void CheckForItem()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool wasItemNear = isItemNear;

        if (Physics.Raycast(ray, out hit, rayDisance, itemLayerMask))
        {
            if (hit.collider != null && hit.collider.CompareTag("MagicBall"))
            {

                Vector3 directionToItem = hit.point - Camera.main.transform.position;
                RaycastHit obstructionHit;

                if (!Physics.Raycast(Camera.main.transform.position, directionToItem, out obstructionHit, Vector3.Distance(Camera.main.transform.position, hit.point), ~itemLayerMask))
                {

                    Item item = hit.collider.GetComponent<Item>();

                    if (item != null)
                    {
                        if (!isItemNear || currentItem != item)
                        {
                            currentItem = item;
                            subtitleManager.ShowSubtitle("I really want to see what is inside.");
                        }

                        isItemNear = true;
                        return;
                    }
                }
            }
        }

        if (wasItemNear)
        {
            isItemNear = false;
            currentItem = null;
        }
    }

    void OnGUI()
    {
        // Only show hand texture when near magic ball
        if (handTexture != null && isItemNear)
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

    private void Teleport()
    {
        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        player.transform.position = teleportPosition;
        player.transform.rotation = teleportRotation;

        if (controller != null)
        {
            controller.enabled = true;
        }

    }

}
