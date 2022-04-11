using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrotherText : MonoBehaviour
{
    public string[] randomEncouragement;
    public float appearTime;

    public void PlayRandomEncouragement()
    {
        StartCoroutine(PlayAfterDelay(2.5f));
    }

    private IEnumerator PlayAfterDelay(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);

        int randomInt = Random.Range(0, randomEncouragement.Length);
        TextBubble.Create(transform, Vector2.zero, randomEncouragement[randomInt], appearTime);
    }
}
