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
   
    //private MovementPlatformer playerScript;   // all of the commented out section is if I want to prevent the player from moving while looking down.

    void Start()
    {
        holderCam = GetComponent<RoomManagerOne>().virtualCam.GetComponent<CinemachineVirtualCamera>();
        cam = holderCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        input = InputManager.instance;
        baseY = cam.m_ScreenY;
        newY = baseY - offSet;

       // playerScript = GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>();
    }

    void Update()
    {
        if (input.KeyDown(Keybindings.KeyList.Down) && !isActive)
            coroutine = StartCoroutine(LookDown());
        if (input.KeyUp(Keybindings.KeyList.Down))
            ResetToNormal();

       /* if (playerScript.directionPressedNow == MovementPlatformer.DirectionPressed.Down && !isActive)
            coroutine = StartCoroutine(LookDown());
        if (playerScript.directionPressedNow != MovementPlatformer.DirectionPressed.Down && isActive)
            ResetToNormal();*/
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
