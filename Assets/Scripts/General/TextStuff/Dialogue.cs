using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.UI;

[System.Serializable]
public class Dialogue
{
    public string name;

    /// <summary>
    // add enum for emotion, image, add editor edits for booleans
    /// </summary>

    /*
[TextArea(3, 10)]  // editor text box size
public string[] sentences;

public bool showImage;
[ConditionalField("showImage")] [SearchableEnum] public Characters character;


public enum Characters
{
    Player,
    Brother,

}
/*
[System.Serializable]
public class DialogueHelper
{
    public string sentence;

    public bool showImage;
    [ConditionalField("showImage")] Characters character;


    private enum Characters
    {
        Player,
        Brother,

    }
}
*/

    public DialogueHelper[] sentences;

    [System.Serializable]
    public class DialogueHelper
    {
        [TextArea(3, 10)]  // editor text box size
        public string sentence;

        public bool showImage;
        [ConditionalField("showImage")] [SearchableEnum] public Characters character;
        [ConditionalField("showImage")] [SearchableEnum] public Mood mood;


        public enum Characters
        {
            Player,
            Brother,

        }
        
        public enum Mood
        {
            Happy,
            Sad,
            Mad,
            Laughing,
            Serious,
        }

    }
}
