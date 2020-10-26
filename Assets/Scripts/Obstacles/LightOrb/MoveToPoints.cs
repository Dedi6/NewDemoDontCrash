using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToPoints : MonoBehaviour
{
    public float speed, timePerMove;
    private Vector2 originalPos;
    private Vector2 targetPos;
    public Transform[] positionsArray;
    private int currentPos = 1, numberOfPositions;
    private bool shouldMove = false, wasStarted;

    void Start()
    {
        transform.position = positionsArray[0].transform.position;
        originalPos = transform.position;
        numberOfPositions = positionsArray.Length;
        StartCoroutine(MovePositions());
        wasStarted = true;
    }

    void Update()
    {

        if (shouldMove)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            if (Vector2.Distance(transform.position, targetPos) <= 0)
            {
                shouldMove = false;
            }
        }
    }

    private IEnumerator MovePositions()
    {
        yield return new WaitForSeconds(timePerMove);

        originalPos = transform.position;
        targetPos = positionsArray[currentPos].transform.position;
        shouldMove = true;

        StartCoroutine(MovePositions());
        if (numberOfPositions > 2)
            currentPos = currentPos == numberOfPositions - 1 ? 0 : currentPos + 1;
        else
            currentPos = currentPos == 1 ? 0 : 1;
    }

    private void OnDisable()
    {
        transform.position = positionsArray[0].transform.position;
        currentPos = 1;
    }

    private void OnEnable()
    {
        if(wasStarted)
            StartCoroutine(MovePositions());
    }
}
