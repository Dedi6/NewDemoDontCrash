using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class Pullable_Object : MonoBehaviour
{
    [SerializeField]
    private bool is_SidePull = true, is_UpPull = true, is_Animated = false, shouldStopWhenPulling = false;

    [ConditionalField(nameof(is_Animated))]
    [SerializeField]
    [SearchableEnum]
    private Object_Animation_Type animationType;


    [System.Serializable]
    private enum Object_Animation_Type
    {
        Lantern,
        Branch,
    }

    public bool Can_Pull_Side()
    {
        return is_SidePull;
    }
    public bool Can_Pull_Up()
    {
        return is_UpPull;
    }

    public bool Should_Animate_OnPull()
    {
        return is_Animated;
    }
    public bool Should_Stop_WhenPulling()
    {
        return shouldStopWhenPulling;
    }

    public void Animate_Now()
    {
        switch(animationType)
        {
            case Object_Animation_Type.Branch:
                GetComponent<Animator>().SetTrigger("AnimateNow");
                AudioManager.instance.PlaySound(AudioManager.SoundList.Grab_Branch);
                break;
            case Object_Animation_Type.Lantern:
                GetComponent<Lantern_Swing>().SwingLantern();
                AudioManager.instance.PlaySound(AudioManager.SoundList.Grab_Lantern);
                break;
        }
    }
}
