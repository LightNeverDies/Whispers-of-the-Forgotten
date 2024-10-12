using UnityEngine;

public class MasterLockController : MonoBehaviour
{
    private string currentNumbers = "0000";
    private string correctCombination = "1884";
    public GameObject lockGameObject;
    public LockRaycast lockRaycast;
    public OpenJewaryBox openJewaryBox;

    // Метод за обновяване на комбинацията
    public void UpdateCurrentNumbers(string gameObjectName, int currentNumber)
    {
        int index = GetIndexFromName(gameObjectName);
        if (index >= 0 && index < currentNumbers.Length)
        {
            // Създайте нова версия на `currentNumbers` с обновената стойност
            char[] numbersArray = currentNumbers.ToCharArray();
            numbersArray[index] = currentNumber.ToString()[0];
            currentNumbers = new string(numbersArray);
        }
    }

    // Метод за получаване на индекс от името на обекта
    private int GetIndexFromName(string gameObjectName)
    {
        switch (gameObjectName)
        {
            case "LockNumber-1":
                return 0;
            case "LockNumber-2":
                return 1;
            case "LockNumber-3":
                return 2;
            case "LockNumber-4":
                return 3;
            default:
                return -1;
        }
    }


    // Метод за проверка на комбинацията
    public void CheckLock()
    {
        if (currentNumbers == correctCombination)
        {
            Unlock();
        }
    }

    private void Unlock()
    {
        Destroy(lockGameObject.gameObject);
        lockRaycast.CloseLockView();
        openJewaryBox.TriggerEventJewaryBox();
    }
}
