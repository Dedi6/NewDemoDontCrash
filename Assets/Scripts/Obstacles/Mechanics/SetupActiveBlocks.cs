using UnityEngine;

public class SetupActiveBlocks : MonoBehaviour
{
    public GameObject pairedObject;

    private void Awake()
    {
        pairedObject.transform.position = transform.position;
        Vector2 objSize = GetComponent<SpriteRenderer>().size;
        pairedObject.GetComponent<SpriteRenderer>().size = objSize;
        pairedObject.GetComponent<BoxCollider2D>().size = objSize;

    }
}
