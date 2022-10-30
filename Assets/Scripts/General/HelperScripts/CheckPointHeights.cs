using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointHeights : MonoBehaviour
{
    private GameMaster gm;
    public GameObject respawnPosition;
    [SerializeField] private GameObject light_object;


    private bool isActive;

    void Start()
    {
        gm = GameMaster.instance;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive && other.gameObject.layer == 11) // 11 is player
        {
            isActive = true;
            gm.lastCheckPointPosition = respawnPosition.transform.position;
            GetComponent<Animator>().SetTrigger("Trigger");
        }
    }

    public void ResetCheckpoint()
    {
        if(isActive)
        {
            isActive = false;
            GetComponent<Animator>().SetTrigger("Reset");
            light_object.SetActive(false);
        }
    }
}
