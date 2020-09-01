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
            InputManager im = InputManager.instance;
            if(im.IsUsingKeyboard())
            {
                string t = "Press '" + im.keybindings.CheckKey(key).ToString() + "' " + text;
                TextBubble.Create(parent, new Vector3(-1, 2), t, appearTime);
            }
            else
            {
                string name = im.keybindings.CheckKey(key).ToString();
                string t = "Press '" + im.GetControllerKeyWord(name) + "' " + text;
                TextBubble.Create(parent, new Vector3(-1, 2), t, appearTime);
            }

            Destroy(gameObject);
        }
    }

}
