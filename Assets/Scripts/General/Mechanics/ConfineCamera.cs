using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class ConfineCamera : MonoBehaviour
{
    public GameObject newCam; 
    public float dampTime;
    public GameObject currentRoom;
    private CinemachineVirtualCamera originalCam; 

    public void HandleConfiners()
    {
        RoomManagerOne _roomManager = currentRoom.GetComponent<RoomManagerOne>();
        originalCam  = _roomManager.virtualCam.GetComponent<CinemachineVirtualCamera>();
        StartCoroutine(DampConfiners(dampTime));

        newCam.SetActive(true);
        _roomManager.virtualCam = newCam;
        originalCam.gameObject.SetActive(false);

        /*newCam.SetActive(true);
        currentRoom.GetComponent<RoomManagerOne>().virtualCam.SetActive(false);*/
    }

    public void RevertConfiners()
    {
        StartCoroutine(DampConfiners(dampTime));
        RoomManagerOne _roomManager = currentRoom.GetComponent<RoomManagerOne>();

        originalCam.gameObject.SetActive(true);
        _roomManager.virtualCam = originalCam.gameObject;
        newCam.SetActive(false);
    }

    private IEnumerator DampConfiners(float confineTime)
    {
        Camera.main.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Time = 0.45f;

        yield return new WaitForSeconds(confineTime);

        Camera.main.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Time = 0.3f;
    }
}
