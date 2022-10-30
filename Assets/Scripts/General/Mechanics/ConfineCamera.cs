using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class ConfineCamera : MonoBehaviour
{
    public GameObject newCam; 
    public float dampTime;
    public GameObject currentRoom;

    public void HandleConfiners()
    {
        StartCoroutine(DampConfiners(dampTime));
        newCam.SetActive(true);
        currentRoom.GetComponent<RoomManagerOne>().virtualCam.SetActive(false);
    }

    public void RevertConfiners()
    {
        StartCoroutine(DampConfiners(dampTime));
        currentRoom.GetComponent<RoomManagerOne>().virtualCam.SetActive(true);
        newCam.SetActive(false);
    }

    private IEnumerator DampConfiners(float confineTime)
    {
        Camera.main.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Time = 0.45f;

        yield return new WaitForSeconds(confineTime);

        Camera.main.GetComponent<CinemachineBrain>().m_DefaultBlend.m_Time = 0.3f;
    }
}
