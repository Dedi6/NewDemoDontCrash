using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleChatBubble : MonoBehaviour
{
    public Transform parent;
    public Vector3 pos;
    public float appearTime;
    public string text;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 11)
        {
            TextBubble.Create(parent, pos, text, appearTime);
            Destroy(gameObject);
        }
    }
}
