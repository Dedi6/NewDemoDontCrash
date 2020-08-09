using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialChatBubbles : MonoBehaviour
{
    public Transform parent;
    public Keybindings.KeyList key;
    public float appearTime;
    public string text;

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 11)
        {
            string t = "Press '"  + InputManager.instance.keybindings.CheckKey(key).ToString() + "' " + text;
            TextBubble.Create(parent, new Vector3(-1, 2), t, appearTime);
            Destroy(gameObject);
        }
    }

}
