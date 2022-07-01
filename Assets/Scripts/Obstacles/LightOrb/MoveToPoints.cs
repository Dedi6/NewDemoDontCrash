using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToPoints : MonoBehaviour
{
    public float speed, timePerMove;
    private Vector2 targetPos;
    public Transform[] positionsArray;
    private Vector2[] positionsStatic;
    private int currentPos = 1, numberOfPositions;
    private bool shouldMove = false, wasStarted;

    void Start()
    {
        transform.position = positionsArray[0].transform.position;
        numberOfPositions = positionsArray.Length;
        HandleStaticPositions();
        StartCoroutine(MovePositions());
        wasStarted = true;
    }

    void HandleStaticPositions()
    {
        positionsStatic = new Vector2[positionsArray.Length];
        for (int i = 0; i < positionsArray.Length; i++)
        {
            positionsStatic[i] = positionsArray[i].localPosition;
        }
    }

    void Update()
    {

        if (shouldMove)
        {
            transform.localPosition = Vector2.MoveTowards(transform.localPosition, targetPos, speed * Time.deltaTime);
            if (Vector2.Distance(transform.position, targetPos) <= 0)
            {
                shouldMove = false;
            }
        }
    }

    private IEnumerator MovePositions()
    {
        yield return new WaitForSeconds(timePerMove);

        targetPos = positionsStatic[currentPos];
        shouldMove = true;

        StartCoroutine(MovePositions());

        if (numberOfPositions > 2)
            currentPos = currentPos == numberOfPositions - 1 ? 0 : currentPos + 1;
        else
            currentPos = currentPos == 1 ? 0 : 1;
    }

    private void OnDisable()
    {
        transform.position = positionsStatic[0];
        currentPos = 1;
    }

    private void OnEnable()
    {
        if(wasStarted)
            StartCoroutine(MovePositions());
    }
}
