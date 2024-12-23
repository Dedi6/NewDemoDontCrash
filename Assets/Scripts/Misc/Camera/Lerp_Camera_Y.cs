using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Lerp_Camera_Y : MonoBehaviour
{
    [SerializeField]
    private float velocityThreshold, duration;

    private float totalScale = 1;
    private float original_Y;

    private CinemachineFramingTransposer vCam;
    private Rigidbody2D player_rb;
    private bool isActive;

    void Start()
    {
        vCam = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
        player_rb = GameMaster.instance.Get_PlayerRigidBody();
        original_Y = vCam.m_ScreenY;
    }

    private void FixedUpdate()
    {
        if (isActive)
        {
            if(player_rb.velocity.y >= 0)
            {
                vCam.m_ScreenY = original_Y;
                isActive = false;
            }
            return;
        }

        if(player_rb.velocity.y < velocityThreshold)
        {
            isActive = true;
            StartCoroutine(LerpValue(original_Y, 1));
        }
    }

    private IEnumerator LerpValue(float start, float end)
    {
        float timeElapsed = 0;

        float reducedRange = Mathf.Abs(start - end);
        float reducedDuration = duration * (reducedRange / totalScale);

        while (timeElapsed < reducedDuration)
        {
            float t = timeElapsed / reducedDuration;

            vCam.m_ScreenY = Mathf.Lerp(start, end, t);
            timeElapsed += Time.deltaTime;

            yield return null;
        }

        vCam.m_ScreenY = end;
    }
}
