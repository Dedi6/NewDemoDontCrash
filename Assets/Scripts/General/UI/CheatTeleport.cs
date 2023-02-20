using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheatTeleport : MonoBehaviour
{
   
    public void TeleportToRoom(string roomNumber)
    {
        int number = System.Convert.ToInt32(roomNumber);
        if(1 <= number && number <= 62)
        {
            RoomManagerOne[] rooms = FindObjectsOfType<RoomManagerOne>();
            foreach (RoomManagerOne r in rooms)
            {
                if (r.GetComponent<RoomManagerOne>().roomNumber == number)
                {
                    GameObject player = GameMaster.instance.playerInstance;
                    Vector2 endPos;
                    if (r.GetComponentInChildren<CheckPoint>(true) != null)
                        endPos = r.GetComponentInChildren<CheckPoint>(true).respawnPosition.transform.position;
                    else
                        endPos = r.transform.position;
                    player.transform.position = endPos;
                }
            }
        }
    }

    public void CheatNow()
    {
        string t = GetComponent<InputField>().text;
        TeleportToRoom(t);
    }

    public void SkipRoom()
    {
        int t = GameMaster.instance.currentRoom.GetComponent<RoomManagerOne>().roomNumber + 1;
        TeleportToRoom(t.ToString());
    }

    public void GetInfiniteHp()
    {
        GameMaster.instance.playerInstance.GetComponent<Health>().health = 10000;
        GameMaster.instance.playerInstance.GetComponent<Health>().numberOfHearts = 10000;
    }

    public void SetHpNormal()
    {
        GameMaster.instance.playerInstance.GetComponent<Health>().SetHpAsInt(6);
    }
}
