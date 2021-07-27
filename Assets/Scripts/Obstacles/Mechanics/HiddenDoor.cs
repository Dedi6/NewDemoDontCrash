using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HiddenDoor : MonoBehaviour
{
    public Tilemap tilemap;
    private Vector2 size;
    public bool pathRevealed;
    public string hiddenDoorName;

    void Start()
    {
        size = GetComponent<BoxCollider2D>().size;
        CheckIfActive();
    }


    public void CheckIfActive()
    {
        if (PlayerPrefs.HasKey(hiddenDoorName))
            RevealPath();
        else if(!CheckIfInCurrentRoom())
            gameObject.SetActive(false);
    }

    public void RevealPath()
    {
        if(!pathRevealed)
        {
            pathRevealed = true;
            DestroyTiles();
        }
    }

    private void DestroyTiles()
    {
        Vector3Int[] positions = new Vector3Int[(int)size.x * (int)size.y];

        int xLength = (int)size.x;
        int yLength = (int)size.y;
        float xSize = xLength / 2;
        float ySize = yLength / 2;
        Vector3 worldPos = new Vector3(transform.position.x - xSize, transform.position.y + ySize - 1);  // fixes on the right square

        for (int y = 0; y < yLength; y++)
        {
            for (int x = 0; x < xLength; x++)
            {
                Vector3Int currentPos = tilemap.WorldToCell(worldPos);
                tilemap.SetTile(currentPos, null);
                worldPos = new Vector3(worldPos.x + 1, worldPos.y);
            }

            worldPos = new Vector3(worldPos.x - (xLength), worldPos.y - 1f);
        }

        PlayerPrefs.SetInt(hiddenDoorName, 1);

        Destroy(gameObject);
    }

    private bool CheckIfInCurrentRoom()
    {
        return ReferenceEquals(transform.parent.gameObject, GameMaster.instance.currentRoom);
    }
}
