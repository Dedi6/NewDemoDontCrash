using UnityEngine;

public class HittableObject : MonoBehaviour
{
    public UnityEngine.Events.UnityEvent triggered;

    public void HitObject()
    {
        triggered.Invoke();
    }
}
