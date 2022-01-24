using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchFlicker : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.Rendering.Universal.Light2D light;
    [SerializeField]
    private float flickerPerT;

    void Start()
    {
        StartCoroutine(Flicker());
    }

  
    private IEnumerator Flicker()
    {
        yield return new WaitForSeconds(flickerPerT);

        float intensityFloat = Random.Range(0.9f, 1.1f);
        light.intensity = intensityFloat;
        StartCoroutine(Flicker());
    }
}
