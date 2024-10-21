using System.Collections;
using UnityEngine;
using Febucci.UI;

public class Add_TextEffectWhen : MonoBehaviour
{
    private TextAnimator_TMP textAnimator;
  /*  [SerializeField]
    private float size_Increase, increase_Speed;
    private RectTransform buttonTrasform;
    private Coroutine coroutine;
    private bool isActive, isPositive;*/

    private void Start()
    {
        textAnimator = GetComponent<TextAnimator_TMP>();
       // buttonTrasform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
      //  StartCoroutine(ShutOffAnimator());
    }

    private IEnumerator ShutOffAnimator()
    {
        yield return null;
        textAnimator.enabled = true;
        yield return null;
        textAnimator.enabled = false;
    }

    public void ChangeAnimator(bool isOn)
    {
        textAnimator.enabled = isOn;
    }


 /*   private void FixedUpdate()
    {
        if(isActive)
        {
            int positive = isPositive ? 1 : -1;

            if (isPositive && buttonTrasform.localScale.x >= size_Increase)
                isActive = false;
            if (!isPositive && buttonTrasform.localScale.x <= 1f)
                isActive = false;

            Vector3 Bef = new Vector3(buttonTrasform.localScale.x + increase_Speed * positive, buttonTrasform.localScale.y + increase_Speed * positive);
            //float rate = increase_Speed * Time.deltaTime;
            //    Vector3 scale = Vector3.Lerp(buttonTrasform.localScale, new Vector3(size_Increase, size_Increase, size_Increase), rate);
            buttonTrasform.localScale = Bef;
        }
    }*/

 /*   public void ChangeSize(bool isMouseOver)
    {
        isActive = true;
        isPositive = isMouseOver;
     /*   if (coroutine != null) StopCoroutine(coroutine);
        coroutine = StartCoroutine(ChangeSize_Coroutine(isMouseOver));*/
  //  }

  
}
