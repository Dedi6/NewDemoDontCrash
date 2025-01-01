using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Lerp_Camera_Y : MonoBehaviour
{
    [SerializeField]
    private float velocityThreshold, duration, target_Y_Damping;
   // private float velocityThreshold, duration, _offset_Duration, target_Y_Damping, target_Y_ScreenOffset;

    private float totalScale = 1;
    private float original_Y_Damping;
   // private float original_Y_Damping, original_Y_Offset;

    private CinemachineFramingTransposer vCam;
    private Rigidbody2D player_rb;
    private bool isActive;
    private Coroutine _lerp_Coroutine;
//    private Coroutine _lerp_Coroutine, _offset_Coroutine;

    void Start()
    {
        vCam = GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineFramingTransposer>();
        player_rb = GameMaster.instance.Get_PlayerRigidBody();
        original_Y_Damping = vCam.m_YDamping;
      //  original_Y_Offset = vCam.m_ScreenY;
    }

    private void FixedUpdate()
    {
        if (isActive)
        {
            if(player_rb.velocity.y >= 0)
            {
                isActive = false;
                if(_lerp_Coroutine != null)
                {
                    StopCoroutine(_lerp_Coroutine);
                    _lerp_Coroutine = StartCoroutine(LerpValue(vCam.m_YDamping, original_Y_Damping));

                }
                /*if (_offset_Coroutine != null)
                {
                    StopCoroutine(_offset_Coroutine);
                    _offset_Coroutine = StartCoroutine(LerpValue_Offset(vCam.m_ScreenY, original_Y_Offset));
                }*/
            }
            return;
        }

        if(player_rb.velocity.y < velocityThreshold)
        {
            isActive = true;

            if(_lerp_Coroutine != null) StopCoroutine(_lerp_Coroutine);
            //if(_offset_Coroutine != null) StopCoroutine(_offset_Coroutine);

            _lerp_Coroutine = StartCoroutine(LerpValue(original_Y_Damping, target_Y_Damping));
            // _offset_Coroutine = StartCoroutine(LerpValue_Offset(original_Y_Offset, target_Y_ScreenOffset));
        }
    }

    private IEnumerator LerpValue(float start_Damping, float end_Damping)
    {
        float timeElapsed = 0;

        float reducedRange_Damping = Mathf.Abs(start_Damping - end_Damping);
        float reducedDuration = duration * (reducedRange_Damping / totalScale);

        while (timeElapsed < reducedDuration)
        {
            float t = timeElapsed / reducedDuration;

            vCam.m_YDamping = Mathf.Lerp(start_Damping, end_Damping, t);

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        vCam.m_YDamping = end_Damping;
    }

  /*  private IEnumerator LerpValue_Offset(float start, float end)
    {
        float timeElapsed = 0;

        float reducedRange = Mathf.Abs(start - end);
        float reducedDuration = _offset_Duration * (reducedRange / totalScale);

        while (timeElapsed < reducedDuration)
        {
            float t = timeElapsed / reducedDuration;

            vCam.m_ScreenY = Mathf.Lerp(start, end, t);

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        vCam.m_ScreenY = end;
    }*/
}
