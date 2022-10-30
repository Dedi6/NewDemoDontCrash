using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heights_Checkpoint_Helper : MonoBehaviour
{
    [SerializeField]
    private GameObject[] checkpoints;


    private void Start()
    {
        GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>().savedNow += ResetCheckpoints;   
    }


    public void ResetCheckpoints()
    {
        foreach (GameObject gameObject in checkpoints)
        {
            gameObject.GetComponent<CheckPointHeights>().ResetCheckpoint();
        }
    }
}
