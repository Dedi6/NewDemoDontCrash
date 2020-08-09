using UnityEngine;

public class GoUpCollider : MonoBehaviour
{

    public float upForce;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.layer == 11)
        {
            col.GetComponent<MovementPlatformer>().rb.velocity = new Vector2(0, upForce);
            GetComponentInParent<RoomManagerOne>().shouldFreeze = false;
        }
    }
}
