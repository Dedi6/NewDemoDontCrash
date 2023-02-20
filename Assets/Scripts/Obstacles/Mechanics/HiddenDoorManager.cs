using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenDoorManager : MonoBehaviour
{
    public GameObject[] hiddenDoors;
    public GameObject[] doorCranks;
    public GameObject[] HpPickUp;

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
    }

}
