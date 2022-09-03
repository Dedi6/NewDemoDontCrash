using UnityEngine;

public class DisableColliderWhenTriggered : MonoBehaviour
{
    [SerializeField]
    private ColliderType colliderType;
    private enum ColliderType
    {
        BoxCollider,
        CircleCollider,
        Capsule,
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (colliderType)
        {
            case ColliderType.BoxCollider:
                Destroy(GetComponent<BoxCollider2D>());
                break;
            case ColliderType.CircleCollider:
                Destroy(GetComponent<CircleCollider2D>());
                break;
            case ColliderType.Capsule:
                Destroy(GetComponent<CapsuleCollider2D>());
                break;
        }
    }
}
