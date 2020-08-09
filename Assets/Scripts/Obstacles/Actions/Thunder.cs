using UnityEngine;

public class Thunder : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 11) // player
        {
            if (col.transform.position.x > transform.position.x)
                col.GetComponent<MovementPlatformer>().KnockBackPlayer(25f, 1f, 0.5f, true);
            else
                col.GetComponent<MovementPlatformer>().KnockBackPlayer(25f, 1f, 0.5f, false);
            col.GetComponent<MovementPlatformer>().GotHitByAnEnemy(1);
        }
    }

    public void DestroyThunder()
    {
        Destroy(gameObject);
    }
}
