using UnityEngine;

public class WaterBathroom : MonoBehaviour
{
    public float interactDistance = 1.0f;
    public Hints hints;
    public LayerMask interactionLayer;
    private float drainSpeed = 0.2f;
    public GameObject secretObject;

    private bool isNearWater = false;
    private bool isDraining = false; // Дали водата в момента се оттича

    void Update()
    {
        // Ако водата вече се оттича, продължи процеса
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
                    isDraining = true; // Стартиране на процеса на оттичане
                    hints.HideHint();  // Скриване на hint-а, защото вече сме стартирали процеса
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
        // Проверка дали скалата на Z и X е все още по-голяма от 0
        if (transform.localScale.z > 0 || transform.localScale.x > 0)
        {
            float newScaleX = transform.localScale.x - drainSpeed * Time.deltaTime;
            float newScaleZ = transform.localScale.z - drainSpeed * Time.deltaTime;

            // Ограничаване на скалата да не падне под 0
            newScaleX = Mathf.Max(newScaleX, 0);
            newScaleZ = Mathf.Max(newScaleZ, 0);

            transform.localScale = new Vector3(newScaleX, transform.localScale.y, newScaleZ);
        }

        // Намаляване на позицията по Y
        if (transform.position.y > 5.629)
        {
            transform.position -= new Vector3(0, drainSpeed * Time.deltaTime, 0);
        }

        // Ако скалата по Z и X е достигнала 0, спиране на движението
        if (transform.localScale.z <= 0 && transform.localScale.x <= 0)
        {
            transform.localScale = new Vector3(0, transform.localScale.y, 0);
            gameObject.SetActive(false);
            isDraining = false; // Спиране на процеса на оттичане

            BoxCollider secretObjectCollider = secretObject.GetComponent<BoxCollider>();

            if (secretObjectCollider != null)
            {
                secretObjectCollider.enabled = true;
            }
        }
    }
}
