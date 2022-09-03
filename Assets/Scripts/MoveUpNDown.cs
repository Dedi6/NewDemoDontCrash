using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveUpNDown : MonoBehaviour  // side to side like a roller coaster
{
    public float numberOfMoves, intervalT, offset;
    private float currentMoves, currentInterval, currentOffset;
    public bool goUp;
    private bool isCorouActive;


    void SetCurrentFloats(float num, float t, float off)
    {
        currentMoves = num;
        currentInterval = t;
        currentOffset = off;
    }


    private IEnumerator Move()
    {
        if (!isCorouActive)
            isCorouActive = true;

        float y = goUp ? currentOffset : -currentOffset;

        transform.Translate(0, y, 0);

        currentMoves--;

        yield return new WaitForSeconds(currentInterval);

        if(currentMoves <= 0)
        {
            goUp = !goUp;
            currentMoves = numberOfMoves;
        }

        StartCoroutine(Move());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnEnable()
    {
        SetCurrentFloats(numberOfMoves, intervalT, offset);
        StartCoroutine(Move());
    }
}
