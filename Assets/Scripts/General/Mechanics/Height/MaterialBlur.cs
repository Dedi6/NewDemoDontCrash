using UnityEngine;

public class MaterialBlur : MonoBehaviour
{
    [SerializeField]
    private Material material;

    void Start()
    {
        material = GetComponent<SpriteRenderer>().material;
    }
}
