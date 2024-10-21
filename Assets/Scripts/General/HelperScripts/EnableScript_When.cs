using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableScript_When : MonoBehaviour
{

    private void OnEnable()
    {
        Invoke("WaitBefore", 0.1f);
    }
    private void WaitBefore()
    {
        GetComponent<PreventDeselectionGroup>().enabled = true;
    }

    private void OnDisable()
    {
        GetComponent<PreventDeselectionGroup>().enabled = false;
    }
}
