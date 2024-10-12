using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public Light flashlight;  // Reference to the Light component
    public float rotationSpeed = 10f;  // Speed at which the flashlight follows the camera

    [HideInInspector]
    public enum FlashlightState { Off, White, UV }
    public FlashlightState flashlightState = FlashlightState.Off; // Current state of the flashlight
    public bool isEventGoing = false;

    public AudioSource flashlightClick;

    public Color normalColor = Color.white; // Normal light color
    public Color uvColor = new Color(0.4f, 0.1f, 0.8f); // UV light color

    // Add a public property to expose the flashlight state
    public bool IsUVLightOn => flashlightState == FlashlightState.UV;

    // Offset to position the flashlight lower and to the right
   // public Vector3 flashlightOffset = new Vector3(0.3f, -0.2f, 0f); 

    void Update()
    {
        // Toggle flashlight and switch colors with F key
        if (Input.GetKeyDown(KeyCode.F))
        {
            switch (flashlightState)
            {
                case FlashlightState.Off:
                    flashlightState = FlashlightState.White;
                    flashlight.enabled = true;
                    flashlightClick.enabled = true;
                    flashlight.color = normalColor; // Start with normal color
                    break;

                case FlashlightState.White:
                    flashlightState = FlashlightState.UV;
                    flashlight.color = uvColor; // Switch to UV color
                    break;

                case FlashlightState.UV:
                    flashlightState = FlashlightState.Off;
                    flashlight.enabled = false;
                    flashlightClick.enabled = false;
                    break;
            }
        }

        // Move flashlight direction based on camera direction
        if (flashlight.enabled)
        {
            Transform cameraTransform = Camera.main.transform;
            flashlight.transform.position = cameraTransform.position;

            // Set flashlight position to be slightly lower and offset from the camera
            // flashlight.transform.position = cameraTransform.position + cameraTransform.right * flashlightOffset.x
            //  + cameraTransform.up * flashlightOffset.y
            // + cameraTransform.forward * flashlightOffset.z;

            // Set flashlight rotation to match the camera's rotation
            flashlight.transform.rotation = cameraTransform.rotation;

            // Flicker effect during events
            if (isEventGoing)
            {
                flashlight.intensity = Random.Range(0.4f, 0.6f);
            }
            else
            {
                flashlight.intensity = 0.6f;
            }
        }
    }
}
