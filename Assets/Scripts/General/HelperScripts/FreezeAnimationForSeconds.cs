using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezeAnimationForSeconds : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
   
  
    private IEnumerator FreezeNow(float freezeTime)
    {
        animator.speed = 0;
        float randomTime = Random.Range(freezeTime - 2f, freezeTime + 2f);

        yield return new WaitForSeconds(randomTime);

        animator.speed = 1;
    }

}
