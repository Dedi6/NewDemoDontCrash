using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunnyHopping : MonoBehaviour
{

    private MovementPlatformer player_Script;
    [SerializeField]
    private float maxSpeed;


    void Start()
    {
        player_Script = GetComponent<MovementPlatformer>();
    }

    void Update()
    {
        
    }

    private void BunnyHop_Timer()
    {

    }
}
