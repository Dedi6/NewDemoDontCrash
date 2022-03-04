using UnityEngine;

public class TriggerAction : MonoBehaviour
{
    public UnityEngine.Events.UnityEvent triggered;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 11) // 11 is player 
            triggered.Invoke();
    }
}
