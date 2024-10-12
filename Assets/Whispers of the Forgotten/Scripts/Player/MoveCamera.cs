using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    float xRotation = 0f;
    float yRotation = 0f;  // Текуща ротация по Y (наляво/надясно)

    // Ограничения за ротация когато си на леглото
    public bool isOnBed = false;
    public float minYRotation = 0f;  // Минимална ротация наляво (0 градуса)
    public float maxYRotation = 180f; // Максимална ротация надясно (180 градуса)
    public float minXRotation = -90f; // Ограничение за ротация по X (нагоре/надолу)
    public float maxXRotation = 30f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Запазваме текущата ротация на тялото (по Y)
        yRotation = playerBody.eulerAngles.y;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        if (isOnBed)
        {
            // Ограничаваме ротацията на камерата по X (нагоре/надолу)
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minXRotation, maxXRotation);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // Обновяваме ротацията по Y ръчно, като запазваме в диапазон от 0 до 180 градуса
            yRotation += mouseX;
            yRotation = Mathf.Clamp(yRotation, minYRotation, maxYRotation);

            // Приложи ротацията по Y само в диапазона от 0 до 180 градуса
            playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
        else
        {
            // Нормално движение на камерата (без ограничения)
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}
