using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterFallParticles : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem particles;
    [SerializeField]
    private float emissionRate, intervals;
    [SerializeField]
    private float timeOn;

    private void Start()
    {
        StartCoroutine(WaterFallCorou());
    }

    private IEnumerator WaterFallCorou()
    {
        var emission = particles.emission;
        emission.rateOverTime = emissionRate;

        yield return new WaitForSeconds(timeOn);

        emission.rateOverTime = 0;

        yield return new WaitForSeconds(intervals);


        StartCoroutine(WaterFallCorou());
    }
}
