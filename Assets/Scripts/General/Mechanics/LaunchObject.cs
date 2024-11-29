using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchObject : MonoBehaviour
{
    [SerializeField]
    private float launchSpeed, launchSpeed_x, slowMo_Amount, slowMo_Time;


   
    public void Launch()
    {
        StartCoroutine(StartSlowMo());
    }

    private IEnumerator StartSlowMo()
    {
        Time.timeScale = slowMo_Amount;

        yield return new WaitForSeconds(slowMo_Time);

        Time.timeScale = 1f;

        Rigidbody2D _rb = GameMaster.instance.playerInstance.GetComponent<Rigidbody2D>();
        //_rb.velocity = new Vector2(_rb.velocity.x, launchSpeed);
        _rb.velocity = new Vector2(launchSpeed_x, launchSpeed);
    }
}
