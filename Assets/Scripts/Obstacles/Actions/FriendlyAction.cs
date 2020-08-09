using UnityEngine;

public class FriendlyAction : MonoBehaviour
{
    public int damage;
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.layer == 12)
            col.GetComponent<Enemy>().TakeDamage(damage);
    }
}
