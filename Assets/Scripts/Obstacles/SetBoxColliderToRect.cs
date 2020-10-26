using UnityEngine;

public class SetBoxColliderToRect : MonoBehaviour
{
    void Start()
    {
        Vector2 sizeCol = GetComponent<SpriteRenderer>().size;
        GetComponent<BoxCollider2D>().size = sizeCol;
    }
}
