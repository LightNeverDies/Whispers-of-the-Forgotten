using UnityEngine;

public class MasterLockController : MonoBehaviour
{
    private string currentNumbers = "0000";
    private string correctCombination = "1884";
    public GameObject lockGameObject;
    public LockRaycast lockRaycast;
    public OpenJewaryBox openJewaryBox;

    // ����� �� ���������� �� ������������
    public void UpdateCurrentNumbers(string gameObjectName, int currentNumber)
    {
        int index = GetIndexFromName(gameObjectName);
        if (index >= 0 && index < currentNumbers.Length)
        {
            // �������� ���� ������ �� `currentNumbers` � ���������� ��������
            char[] numbersArray = currentNumbers.ToCharArray();
            numbersArray[index] = currentNumber.ToString()[0];
            currentNumbers = new string(numbersArray);
        }
    }

    // ����� �� ���������� �� ������ �� ����� �� ������
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


    // ����� �� �������� �� ������������
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
