using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveUpNDown : MonoBehaviour  // side to side like a roller coaster
{
    public float numberOfMoves, intervalT, offset;
    private float currentMoves, currentInterval, currentOffset;
    public bool goUp;
    private MovementPlatformer mp;
    private bool isCorouActive;

    [Header("Running")]
    [Space]
    public float runningNumberOfMoves; public float runningIntervalT, runningOffset;

    void Start()
    {
        mp = GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>();
        SetCurrentFloats(numberOfMoves, intervalT, offset);
        StartCoroutine(Move());
    }

    private void Update()
    {

        if(mp.moveInput == 0 && currentInterval != intervalT)
        {
            SetCurrentFloats(numberOfMoves, intervalT, offset);
        }
        else if(mp.moveInput != 0 && currentInterval != runningIntervalT)  // running
        {
            SetCurrentFloats(runningNumberOfMoves, runningIntervalT, runningOffset);
        }

        if (!mp.isGrounded && isCorouActive)
        {
            StopAllCoroutines();
            isCorouActive = false;
        }
        else if (mp.isGrounded && !isCorouActive)
            StartCoroutine(Move());
    }

    void SetCurrentFloats(float num, float t, float off)
    {
        transform.localPosition = Vector3.zero;
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
}
