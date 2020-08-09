using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    public Animator animator;

    private void Awake()
    {
        EnableObject();
    }

    public void DisableObject()
    {
        gameObject.SetActive(false);
    }
    
    public void EnableObject()
    {
        gameObject.SetActive(true);
    }

}
