using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeIn : MonoBehaviour
{
    private SpriteRenderer _sRenderer;

    private Color _spriteColour;

    private void Start()
    {
        _sRenderer = GetComponent<SpriteRenderer>();

        _spriteColour = _sRenderer.color;
    }

    public IEnumerator FadeTo(float aValue, float aTime)
    {
        float alpha = _sRenderer.color.a;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha, aValue, t));
            _sRenderer.color = newColor;
            yield return null;
        }

        if(aValue == 1)
            SetFull();
    }

    public void SetFull()
    {
        Color newColor = new Color(1, 1, 1, 1);
        _sRenderer.color = newColor;
    }

    public void SetStartScene()
    {
        SetFull();
        StartCoroutine(FadeTo(0f, 0.75f));
    }
}
