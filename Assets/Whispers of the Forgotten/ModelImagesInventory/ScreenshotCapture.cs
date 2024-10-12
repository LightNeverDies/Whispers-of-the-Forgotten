using UnityEngine;

public class ScreenshotCapture : MonoBehaviour
{
    void Update()
    {
        // Capture screenshot when you press the "K" key
        if (Input.GetKeyDown(KeyCode.K))
        {
            // Use a simple relative path and make sure the directory exists
            string directory = "D:\\GameDev\\Whispers of the Forgotten\\Whispers of the Forgotten\\Assets\\Whispers of the Forgotten\\ModelImagesInventory";
            string path = directory + "/image.png";

            // Ensure the directory exists
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            // Capture the screenshot
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log("Screenshot saved to: " + path);
        }
    }
}
