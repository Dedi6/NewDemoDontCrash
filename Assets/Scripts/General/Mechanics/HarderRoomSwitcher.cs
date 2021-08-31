using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;


public class HarderRoomSwitcher : MonoBehaviour
{
    public Color newColor;
    private Color orignalColor;
    public GameObject normalRoomObjects, background;
    public Transform StartPos;
    private bool isActive;
    public bool isEndPoint;
    [SerializeField]
    private Animator animator;


    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.layer == 11)
        {
            GetComponentInParent<RoomManagerOne>().PlayerRespawnReset2();

            if (!isActive)
                SwitchToBSide();
            else
                RevertToNormal(false);

        }
    }

    private void SwitchToBSide()
    {
        SwitcherHelper(true);
    }

    public void RevertToNormal(bool reachedEnd)
    {
        SwitcherHelper(false);
        if (reachedEnd)
            Debug.Log("sdsfs");
    }

    private void SwitcherHelper(bool activatingNow)
    {
        for (int i = 0; i < normalRoomObjects.transform.childCount; i++)
            normalRoomObjects.transform.GetChild(i).gameObject.SetActive(!activatingNow);

        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(activatingNow);

        isActive = activatingNow;
        if(activatingNow) background.GetComponent<Light2D>().color = newColor; else background.GetComponent<Light2D>().color = Color.white;

        animator.SetBool("IsActive", activatingNow);
    }

}
