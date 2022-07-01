using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class TriggerAction : MonoBehaviour
{
    public UnityEngine.Events.UnityEvent triggered;
    private bool isActive = true;
    public bool shouldSpawnAtPoint;

    [ConditionalField(nameof(shouldSpawnAtPoint), false, true)] public Transform spawnPos;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (isActive && col.gameObject.layer == 11) // 11 is player 
        {
            StartCoroutine(SwitchBoolAfterSec());
            triggered.Invoke();
        }
    }

    private IEnumerator SwitchBoolAfterSec()
    {
        isActive = false;

        yield return new WaitForSeconds(0.2f);

        isActive = true;
    }

    public void TriggerActionNow()
    {
        triggered.Invoke();
    }
}
