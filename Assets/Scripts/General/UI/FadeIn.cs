using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeIn : MonoBehaviour
{
    private SpriteRenderer _sRenderer;
    [SerializeField]
    private bool isImage;
    private Image _image;

    private Color _spriteColour;

    private void Start()
    {
        if (isImage)
        {
            _image = GetComponent<Image>();
            _spriteColour = _image.color;
            return;
        }

        _sRenderer = GetComponent<SpriteRenderer>();


        _spriteColour = _sRenderer.color;
    }

    public IEnumerator FadeTo(float aValue, float aTime)
    {
        float alpha = isImage ? _image.color.a : _sRenderer.color.a;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / aTime)
        {
            Color newColor = new Color(1, 1, 1, Mathf.Lerp(alpha, aValue, t));

            if (isImage)
                _image.color = newColor;
            else
                _sRenderer.color = newColor;
            
            yield return null;
        }

        if(aValue == 1)
            SetFull();
    }

    public void SetFull()
    {
        Color newColor = new Color(1, 1, 1, 1);

        if (isImage)
            _image.color = newColor;
        else
            _sRenderer.color = newColor;
    }

    public void SetStartScene()
    {
        SetFull();
        StartCoroutine(FadeTo(0f, 0.75f));
    }

    public void Fade_SlowMo()
    {
        Time.timeScale = 0.3f;
        StartCoroutine(Test());
    }

    private IEnumerator Test()
    {
        yield return null;

        Time.timeScale = 0.3f;
        StartCoroutine(FadeTo(1f, 0.6f));
    }

}
