using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CellOrganizer : MonoBehaviour
{
    public static CellOrganizer instance;
    public OrganizerHelper[] cells;
    public Transform[] positions;
    public int currentPos = -1;
    public GameObject player;
    public Tilemap tileMap;

    [System.Serializable]
    public class OrganizerHelper
    {
        public Transform positionToFollow;
        public GameObject currentCell;
    }


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
            Destroy(gameObject);

      //  cells = new OrganizerHelper[positions.Length];
        
    }

    private void Start()
    {
        player = GameMaster.instance.playerInstance;
    }

    public void AddCell(GameObject newCell)
    {
        currentPos++;
        cells[currentPos].positionToFollow = positions[currentPos];
        cells[currentPos].currentCell = newCell;
    }

    public bool CanAddCell()
    {
        return currentPos < cells.Length - 1 ? true : false;
    }

    public void ReleaseLatest()
    {

        cells[currentPos].positionToFollow = null;
        cells[currentPos].currentCell.GetComponent<JumpStone>().DeactivateCell();
        cells[currentPos].currentCell = null;

        currentPos--;
    }

    public void ReleaseAll()
    {
        while(currentPos > -1)
        {
            cells[currentPos].positionToFollow = null;
            cells[currentPos].currentCell.GetComponent<JumpStone>().Pop();
            cells[currentPos].currentCell = null;

            currentPos--;
        }
    }

    public Transform GetCurrentPoint()
    {
        return positions[currentPos];
    }

    public bool HaveOrbs()
    {
        return currentPos >= 0 ? true : false;
    }

}
