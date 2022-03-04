using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{

    private GameMaster gm;
    public GameObject respawnPosition;

    [SerializeField]
    private bool isAnimated;
    private bool isActive;

    void Start()
    {
        gm = GameMaster.instance;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            gm.lastCheckPointPosition = respawnPosition.transform.position;
            if (isAnimated && !isActive)
            {
                isActive = true;
                GetComponent<Animator>().SetTrigger("Trigger");
            }
        }
    }    

}
