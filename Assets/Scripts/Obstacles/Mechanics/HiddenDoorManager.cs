using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenDoorManager : MonoBehaviour
{
    public GameObject[] hiddenDoors;
    public GameObject[] doorCranks;
    public GameObject[] HpPickUp;
    public GameObject[] cannonBall_Walls;

    public bool resetDoors;

    void Start()
    {
        foreach (GameObject door in hiddenDoors)
        {
            door.SetActive(true);
            
            if (resetDoors)
            {
                PlayerPrefs.DeleteKey(door.GetComponent<HiddenDoor>().hiddenDoorName);
            }
        }

        foreach (GameObject crank in doorCranks)
        {
            crank.SetActive(true);

            if (resetDoors)
            {
            Debug.Log("Why");
                PlayerPrefs.DeleteKey(crank.GetComponent<DoorCrank>().nameForSave);
            }
        }

        foreach (GameObject hp_Pick in HpPickUp)
        {
            hp_Pick.SetActive(true);

            if (resetDoors)
            {
                PlayerPrefs.DeleteKey(hp_Pick.GetComponent<Hp_Pickable>().nameForSave);
            }
        }

        foreach (GameObject cannonWall in cannonBall_Walls)
        {
            cannonWall.SetActive(true);

            if (resetDoors)
            {
                PlayerPrefs.DeleteKey(cannonWall.GetComponent<CannonBall_Wall>().nameForSave);
            }
        }

    }

}
