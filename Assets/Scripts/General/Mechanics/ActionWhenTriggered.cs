using UnityEngine;
using UnityEngine.Events;
public class ActionWhenTriggered : MonoBehaviour
{
    public UnityEvent OnTrigger;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 11) // player
        {
            OnTrigger.Invoke();
        }
    }
}
