using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warp_Distortion_Shader : MonoBehaviour
{
    [SerializeField] private float _shockWaveTime = 0.75f;

    private Coroutine _shockWaveCoroutine;

    private Material _material;

    private static int _shockWaveStrength = Shader.PropertyToID("_ShockWaveStrength");

    private void Awake()
    {
        _material = GetComponent<SpriteRenderer>().material;
    }

    public void CallShockWave(float startPos, float endPos, float time)
    {
        _shockWaveTime = time;
        _shockWaveCoroutine = StartCoroutine(ShockWaveAction(startPos, endPos));
    }

    private IEnumerator ShockWaveAction(float startPos, float endPos)
    {
        _material.SetFloat(_shockWaveStrength, startPos);

        float lerpedAmount = 0f;

        float elapsedTime = 0f;

        while(elapsedTime < _shockWaveTime)
        {
            elapsedTime += Time.deltaTime;

            lerpedAmount = Mathf.Lerp(startPos, endPos, (elapsedTime / _shockWaveTime));

            _material.SetFloat(_shockWaveStrength, lerpedAmount);

            yield return null;
        }
    }
}
