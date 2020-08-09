using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyHitVfx : MonoBehaviour
{
    public Animator animator;
    void Start()
    {
        Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
    }
}
