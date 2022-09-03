using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class TriggerAction : MonoBehaviour
{
    public UnityEngine.Events.UnityEvent triggered;
    private bool isActive = true;
    public bool shouldSpawnAtPoint;
    [SerializeField]
    private bool shouldDestroyTrigger, shouldDisableTrigger;

    [ConditionalField(nameof(shouldSpawnAtPoint), false, true)] public Transform spawnPos;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (isActive && (col.gameObject.layer == 11 || col.gameObject.layer == 6)) // 11 is player 
        {
            StartCoroutine(SwitchBoolAfterSec());
            triggered.Invoke();
            if (shouldDestroyTrigger)
                Destroy(gameObject);
            /*if (shouldDisableTrigger)
                gameObject.SetActive(false);*/
        }
    }

    private IEnumerator SwitchBoolAfterSec()
    {
        isActive = false;

        yield return new WaitForSeconds(0.2f);

        isActive = true;
        if (shouldDisableTrigger)
            gameObject.SetActive(false);
    }

    public void TriggerActionNow()
    {
        triggered.Invoke();
    }
}
