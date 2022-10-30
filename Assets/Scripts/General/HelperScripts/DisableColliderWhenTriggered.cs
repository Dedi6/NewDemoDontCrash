using UnityEngine;

public class DisableColliderWhenTriggered : MonoBehaviour
{
    [SerializeField]
    private ColliderType colliderType;
    [SerializeField]
    private bool shouldDestroyCollider;

    private enum ColliderType
    {
        BoxCollider,
        CircleCollider,
        Capsule,
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (shouldDestroyCollider)
            DestroyCollider();
        else
            DisableCollider();
    }

    void DisableCollider()
    {
        switch (colliderType)
        {
            case ColliderType.BoxCollider:
                GetComponent<BoxCollider2D>().enabled = false;
                break;
            case ColliderType.CircleCollider:
                GetComponent<CircleCollider2D>().enabled = false;
                break;
            case ColliderType.Capsule:
                GetComponent<CapsuleCollider2D>().enabled = false;
                break;
        }
    }

    void DestroyCollider()
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
