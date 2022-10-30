using UnityEngine;
using MyBox;

public class SpawnObjectWhen : MonoBehaviour
{
    [SerializeField]
    private GameObject objectToSpawn;
    public bool spawnAtSpecificPosition;
    [ConditionalField("spawnAtSpecificPosition")] public Transform spawnPrefabAt;

    public void SpawnObject()
    {
        if(!spawnAtSpecificPosition)
            Instantiate(objectToSpawn, transform.position, Quaternion.identity, transform.parent);   // parent is for garbage
        else
            Instantiate(objectToSpawn, spawnPrefabAt.position, Quaternion.identity, transform.parent);
    }
}
