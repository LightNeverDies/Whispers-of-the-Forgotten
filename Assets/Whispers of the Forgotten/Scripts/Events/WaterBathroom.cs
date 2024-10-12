using UnityEngine;

public class WaterBathroom : MonoBehaviour
{
    public float interactDistance = 1.0f;
    public Hints hints;
    public LayerMask interactionLayer;
    private float drainSpeed = 0.2f;
    public GameObject secretObject;

    private bool isNearWater = false;
    private bool isDraining = false; // ���� ������ � ������� �� ������

    void Update()
    {
        // ��� ������ ���� �� ������, �������� �������
        if (isDraining)
        {
            DrainWater();
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactionLayer))
        {
            if (hit.collider.CompareTag("Water"))
            {
                isNearWater = true;
                hints.ShowHint("Press E to Drain Water");

                if (Input.GetKeyDown(KeyCode.E))
                {
                    isDraining = true; // ���������� �� ������� �� ��������
                    hints.HideHint();  // �������� �� hint-�, ������ ���� ��� ���������� �������
                }
                return;
            }
        }

        if (isNearWater)
        {
            hints.HideHint();
            isNearWater = false;
        }
    }

    void DrainWater()
    {
        // �������� ���� ������� �� Z � X � ��� ��� ��-������ �� 0
        if (transform.localScale.z > 0 || transform.localScale.x > 0)
        {
            float newScaleX = transform.localScale.x - drainSpeed * Time.deltaTime;
            float newScaleZ = transform.localScale.z - drainSpeed * Time.deltaTime;

            // ������������ �� ������� �� �� ����� ��� 0
            newScaleX = Mathf.Max(newScaleX, 0);
            newScaleZ = Mathf.Max(newScaleZ, 0);

            transform.localScale = new Vector3(newScaleX, transform.localScale.y, newScaleZ);
        }

        // ���������� �� ��������� �� Y
        if (transform.position.y > 5.629)
        {
            transform.position -= new Vector3(0, drainSpeed * Time.deltaTime, 0);
        }

        // ��� ������� �� Z � X � ���������� 0, ������� �� ����������
        if (transform.localScale.z <= 0 && transform.localScale.x <= 0)
        {
            transform.localScale = new Vector3(0, transform.localScale.y, 0);
            gameObject.SetActive(false);
            isDraining = false; // ������� �� ������� �� ��������

            BoxCollider secretObjectCollider = secretObject.GetComponent<BoxCollider>();

            if (secretObjectCollider != null)
            {
                secretObjectCollider.enabled = true;
            }
        }
    }
}
