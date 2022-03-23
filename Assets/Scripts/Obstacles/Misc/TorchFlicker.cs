using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchFlicker : MonoBehaviour
{
    [SerializeField]
    private UnityEngine.Rendering.Universal.Light2D lightTorch;
    [SerializeField]
    private float flickerPerT;

    void Start()
    {
        lightTorch = GetComponent<UnityEngine.Rendering.Universal.Light2D>();
        StartCoroutine(Flicker());
    }

  
    private IEnumerator Flicker()
    {
        yield return new WaitForSeconds(flickerPerT);

        float intensityFloat = Random.Range(0.9f, 1.1f);
        lightTorch.intensity = intensityFloat;
        StartCoroutine(Flicker());
    }
}
