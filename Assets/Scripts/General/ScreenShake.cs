using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ScreenShake : MonoBehaviour
{
    public NoiseSettings noiseSet;
    
    public IEnumerator ShakeyShakey(float time, float amp)
    {
        var noise = GetComponent<CinemachineVirtualCamera>().AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        noise.m_NoiseProfile = noiseSet;
        noise.m_AmplitudeGain = 2;
        yield return new WaitForSeconds(time);
        noise.m_AmplitudeGain = 0;
    }
}
