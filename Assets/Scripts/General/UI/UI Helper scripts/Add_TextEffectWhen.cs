using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Add_TextEffectWhen : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textObject;
    [SerializeField]
    private string effect;
    private bool isEffectActive;
    private string originalText;

    private void Start()
    {
        originalText = textObject.text;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            ChangeEffect();
    }

    private void ChangeEffect()
    {
        if(!isEffectActive)
        {
            isEffectActive = true;
            textObject.text = new string(effect + originalText);
        }
        else
        {
            isEffectActive = false;
            StartCoroutine(TestCoroutine());
        }
    }

    private IEnumerator TestCoroutine()
    {
        textObject.text = null;
        yield return null;
        textObject.text = new string(originalText);
    }
}
