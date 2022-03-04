using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlurControl : MonoBehaviour
{
    public Material matClose;
    public Material matFar;
    public Material objectsClose;
    public Material objectsFar;
    public float blurAmount, blurObjects;

    private void Start()
    {
        SwitchBlur(false);
    }

    public void SwitchBlur(bool isCloseLayer)
    {
        if(isCloseLayer)
        {
            matClose.SetFloat("_BlurAmount", 0);
            matFar.SetFloat("_BlurAmount", blurAmount);
            objectsClose.SetFloat("_BlurAmount", 0);
            objectsFar.SetFloat("_BlurAmount", blurObjects);
        }
        else
        {
            matClose.SetFloat("_BlurAmount", blurAmount);
            matFar.SetFloat("_BlurAmount", 0);
            objectsClose.SetFloat("_BlurAmount", blurObjects);
            objectsFar.SetFloat("_BlurAmount", 0);
        }
    }

    public void ChangeMatOfObject(GameObject gameObject, bool switchToFar)
    {
        Material newMat = switchToFar ? objectsFar : objectsClose;
        gameObject.GetComponent<SpriteRenderer>().material = newMat;
    }
}
