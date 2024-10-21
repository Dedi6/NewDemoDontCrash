using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mooman_SecondaryExplosions : MonoBehaviour
{

    [SerializeField] Vector2 force;
    private bool shouldSpawnBombs;
    [SerializeField] GameObject bombPrefab;

    public void SetUp()
    {
        shouldSpawnBombs = true;
    }

    public void SpawnBombs()
    {
        if(shouldSpawnBombs)
        {
            GameObject firstBomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);
            GameObject secondBomb = Instantiate(bombPrefab, transform.position, Quaternion.identity);

            firstBomb.GetComponent<Rigidbody2D>().velocity = force;
            secondBomb.GetComponent<Rigidbody2D>().velocity = new Vector2(-force.x, force.y);
            Debug.Log("test");
        }
    }
}
