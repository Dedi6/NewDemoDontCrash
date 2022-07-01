using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class CheckForHit : MonoBehaviour
{
    [SerializeField]
    private ColliderType colliderType;
    [ConditionalField(nameof(colliderType), false, ColliderType.BoxCollider)] public float colliderX, colliderY;
    [ConditionalField(nameof(colliderType), false, ColliderType.CircleCollider)] public float radius;
    [ConditionalField(nameof(colliderType), false, ColliderType.Capsule)] public float capsuleX, capsuleY;
    [ConditionalField(nameof(colliderType), false, ColliderType.Capsule)] public bool isVertical;


    [SerializeField]
    private int damage;
    [SerializeField]
    private Transform point;
    [SerializeField]
    private float knockBackForce;
    private bool shouldIgnoreCheck;

    private enum ColliderType
    {
        BoxCollider,
        CircleCollider,
        Capsule,
    }


    public void TriggerColliderAtPoint()
    {
        if (shouldIgnoreCheck)
            return;

        switch(colliderType)
        {
            case ColliderType.BoxCollider:
                float angle = transform.eulerAngles.z;
                Collider2D player = Physics2D.OverlapBox(point.transform.position, new Vector2(colliderX, colliderY), angle, 1 << 11);
                if(player != null)
                    PlayerKnockBackAndDamage(player.gameObject);
                break;
            case ColliderType.Capsule:
                break;
            case ColliderType.CircleCollider:
                break;
        }
    }

    public void PlayerKnockBackAndDamage(GameObject player)
    {
        MovementPlatformer playerScript = player.GetComponent<MovementPlatformer>();
        if (player.transform.position.x > transform.position.x)
            playerScript.KnockBackPlayer(knockBackForce, 1f, 0.5f, true);
        else
            playerScript.KnockBackPlayer(knockBackForce, 1f, 0.5f, false);
        playerScript.GotHitByAnEnemy(4);
    }

    public void CallThisToIgnoreCheck()
    {
        shouldIgnoreCheck = true;
    }
}
