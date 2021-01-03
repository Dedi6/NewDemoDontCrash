using UnityEngine;
using MyBox;

public class SetupActiveBlocks : MonoBehaviour
{
    public GameObject pairedObject;
    public bool hasSpikes;
    [ConditionalField("hasSpikes")] public SpikesHelper setByChildrenIndex;


    [System.Serializable]
    public class SpikesHelper
    {
        public SpikeSide[] side;

        public enum SpikeSide
        {
            Up,
            Down,
            Left,
            Right,
        }
    }

    private void Awake()
    {
        pairedObject.transform.position = transform.position;
        Vector2 objSize = GetComponent<SpriteRenderer>().size;
        pairedObject.GetComponent<SpriteRenderer>().size = objSize;
        pairedObject.GetComponent<BoxCollider2D>().size = objSize;
        if(hasSpikes)
        {
            SetSpikes(objSize);
        }
    }

    void SetSpikes(Vector2 sizeO)
    {
        for (int i = 0; i < pairedObject.transform.childCount; i++)
        {
            Transform t = pairedObject.transform.GetChild(i);
            SpriteRenderer sprite = t.GetComponent<SpriteRenderer>();
            Vector2 newSize = new Vector2(sizeO.x, sprite.size.y);
            sprite.size = newSize;
            SpikesHelper.SpikeSide currentSize = setByChildrenIndex.side[i];
            if (currentSize == SpikesHelper.SpikeSide.Up || currentSize == SpikesHelper.SpikeSide.Down)  
            {
                int value = currentSize == SpikesHelper.SpikeSide.Up ? 1 : -1;
                t.GetComponent<BoxCollider2D>().size = new Vector2(sizeO.x, 0.51f);
                t.localPosition = new Vector3(t.localPosition.x, value * sizeO.y / 2, t.localPosition.z);
            }
            else
            {
                int value = currentSize == SpikesHelper.SpikeSide.Right ? 1 : -1;
                t.GetComponent<BoxCollider2D>().size = new Vector2(value * 0.51f, sizeO.y);
                t.localPosition = new Vector3(value * sizeO.x / 2, t.localPosition.y, t.localPosition.z);
            }
        }
    }
}
