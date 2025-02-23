using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestColliderWroldPos : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
            Debug.Log(GetComponent<CapsuleCollider2D>().bounds.center);
    }
}
