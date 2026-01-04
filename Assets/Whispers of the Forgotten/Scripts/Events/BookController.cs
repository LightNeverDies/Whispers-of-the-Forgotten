using UnityEngine;

public class BookController : MonoBehaviour
{
    // Combinations
    //Red + Green = Brown
    //Red + Yellow = Orange
    //Red + Blue = Purple
    //Blue + Green = Cyan

    [SerializeField] private Material[] materials;

    private int currentMaterialIndex = 0;

    void Start()
    {
        UpdateBookMaterial();
    }

    void UpdateBookMaterial()
    {
        GetComponent<Renderer>().material = materials[currentMaterialIndex];
    }

    public void CycleMaterial()
    {
        currentMaterialIndex = (currentMaterialIndex + 1) % materials.Length;
        UpdateBookMaterial();
    }

    public int GetCurrentMaterialIndex()
    {
        return currentMaterialIndex;
    }
}
