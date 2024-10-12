using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    float xRotation = 0f;
    float yRotation = 0f;  // ������ ������� �� Y (������/�������)

    // ����������� �� ������� ������ �� �� �������
    public bool isOnBed = false;
    public float minYRotation = 0f;  // ��������� ������� ������ (0 �������)
    public float maxYRotation = 180f; // ���������� ������� ������� (180 �������)
    public float minXRotation = -90f; // ����������� �� ������� �� X (������/������)
    public float maxXRotation = 30f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // ��������� �������� ������� �� ������ (�� Y)
        yRotation = playerBody.eulerAngles.y;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        if (isOnBed)
        {
            // ������������ ��������� �� �������� �� X (������/������)
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, minXRotation, maxXRotation);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            // ���������� ��������� �� Y �����, ���� ��������� � �������� �� 0 �� 180 �������
            yRotation += mouseX;
            yRotation = Mathf.Clamp(yRotation, minYRotation, maxYRotation);

            // ������� ��������� �� Y ���� � ��������� �� 0 �� 180 �������
            playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
        else
        {
            // �������� �������� �� �������� (��� �����������)
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}
