using UnityEngine;

public class DestroyBulletOnTouch : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == 10) // 10 is bullet
        {
            collision.GetComponent<bullet>().DestroyBulletAndReset();
        }
    }
}
