using UnityEngine;

public class LockPartController : MonoBehaviour
{
    public float rotationSpeed = 900f;
    public float initialRotationOffset = 0f;
    private bool isDragging = false;
    private string gameObjectName;
    public MasterLockController masterLockController;
    public AudioSource combinationLockSoundEffect;

    void Update()
    {
        HandleDragging();
    }

    private void HandleDragging()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.transform == transform)
            {
                isDragging = true;
                gameObjectName = hit.transform.gameObject.name;
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            int currentNumber = GetCurrentNumber();
            masterLockController.UpdateCurrentNumbers(gameObjectName, currentNumber);
            masterLockController.CheckLock();
        }

        if (isDragging)
        {
            float mouseMovement = Input.GetAxis("Mouse X");
            combinationLockSoundEffect.enabled = true;
            RotatePart(mouseMovement);
        }
    }

    void RotatePart(float mouseMovement)
    {
        transform.Rotate(Vector3.forward, -mouseMovement * rotationSpeed * Time.deltaTime);
        combinationLockSoundEffect.Play();
    }

    public int GetCurrentNumber()
    {
        float rotation = transform.localEulerAngles.z;
        float adjustedRotation = (rotation + initialRotationOffset) % 360;
        int number = Mathf.RoundToInt(adjustedRotation / 36f);
        return (number + 10) % 10;
    }
}
