using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class LookDownCamera : MonoBehaviour
{

    public float speed, timeToPress, offSet;
    private CinemachineVirtualCamera holderCam;
    private CinemachineFramingTransposer cam;
    private InputManager input;
    private bool isActive;
    private Coroutine coroutine;
    private float newY, baseY;

    void Start()
    {
        holderCam = GetComponent<RoomManagerOne>().virtualCam.GetComponent<CinemachineVirtualCamera>();
        cam = holderCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        input = InputManager.instance;
        baseY = cam.m_ScreenY;
        newY = baseY - offSet;
    }

    void Update()
    {
        if (input.KeyDown(Keybindings.KeyList.Down) && !isActive)
            coroutine = StartCoroutine(LookDown());
        if (input.KeyUp(Keybindings.KeyList.Down))
            ResetToNormal();
    }

    private IEnumerator LookDown()
    {
        isActive = true;

        yield return new WaitForSeconds(timeToPress);

        while (cam.m_ScreenY >= newY)
        {
            cam.m_ScreenY -= speed;
            yield return null;
        }
    }

    private IEnumerator LookUp()
    {
        StopCorou();

        while (cam.m_ScreenY <= baseY)
        {
            cam.m_ScreenY += speed;
            yield return null;
        }
    }

    void ResetToNormal()
    {
        if (isActive)
            StartCoroutine(LookUp());
        else
        {
            StopCorou();
        }
    }
    
    void StopCorou()
    {
        StopCoroutine(coroutine);
        isActive = false;
    }
}
