using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meteor : MonoBehaviour
{

    public float speed;
    [HideInInspector]
    public Vector3 direction;
    public bool isFriendly;
    public int skillDmg;
    public Animator animator;
    private bool stopMotion = false;

    // Update is called once per frame
    void Update()
    {
        if(!stopMotion)
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 11 && !isFriendly) // player
        {
            if (col.transform.position.x > transform.position.x)
                col.GetComponent<MovementPlatformer>().KnockBackPlayer(25f, 1f, 0.5f, true);
            else
                col.GetComponent<MovementPlatformer>().KnockBackPlayer(25f, 1f, 0.5f, false);
            col.GetComponent<MovementPlatformer>().GotHitByAnEnemy(4);
            animator.SetTrigger("HitPlayer");
            StopMotion();
        }
        else if(col.gameObject.layer == 12 && isFriendly) // enemy
        {
            col.GetComponent<Enemy>().TakeDamage(skillDmg);
        }
        else if (col.gameObject.layer == 8) // 8 is ground
        {
            animator.SetTrigger("HitPlayer"); // but actually hit ground kek
            StopMotion();
        }
    }

    public void SetDirection(Vector2 dir, float velocity)
    {
        direction = dir;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 135f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        speed = velocity;
    }

    public void StopMotion()
    {
        //  this.enabled = false;
        GetComponent<CircleCollider2D>().enabled = false;
        stopMotion = true;
    }

}
