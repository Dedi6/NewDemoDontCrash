using UnityEngine;

public class SpawnPrefabAtLocation : MonoBehaviour
{
    [SerializeField]
    private GameObject objectToSpawn;

    [SerializeField]
    private Transform spawnPos;

    public void SpawnNow()
    {
        GameObject spawnedPrefab = Instantiate(objectToSpawn, spawnPos.position, Quaternion.identity, transform.parent);
    }
}
