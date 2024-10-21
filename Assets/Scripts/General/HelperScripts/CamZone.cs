using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CamZone : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCamera vCam = null;
    [SerializeField] private float easeTime;


    // Start is called before the first frame update
    void Start()
    {
        vCam.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CinemachineBrain _brainCam = CinemachineCore.Instance.FindPotentialTargetBrain(vCam);
            _brainCam.m_DefaultBlend.m_Time = easeTime;
            vCam.enabled = true;

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartCoroutine(WaitBeforeRevertingEase());
            vCam.enabled = false;
        }
    }

    private IEnumerator WaitBeforeRevertingEase()
    {
        yield return new WaitForSeconds(easeTime);

        CinemachineBrain _brainCam = CinemachineCore.Instance.FindPotentialTargetBrain(vCam);
        _brainCam.m_DefaultBlend.m_Time = 0.3f;
    }
}
