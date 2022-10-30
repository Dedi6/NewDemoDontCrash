using UnityEngine;

public class HittableObject : MonoBehaviour
{
    public UnityEngine.Events.UnityEvent triggered;
    [SerializeField]
    private bool shouldTriggerOnce;

    public void HitObject()
    {
        triggered.Invoke();
        if (shouldTriggerOnce)
            GetComponent<CircleCollider2D>().enabled = false;
    }
}
