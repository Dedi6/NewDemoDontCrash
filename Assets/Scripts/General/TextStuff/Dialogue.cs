using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.UI;

[System.Serializable]
public class Dialogue
{
    public string name;
    public bool addDecision;
    [ConditionalField("addDecision")] public string choiceA, choiceB;
    [ConditionalField("addDecision")] public DialogueTrigger dialogueA, dialogueB;


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
            Normal,
        }

    }

    public DialogueHelper.Characters GetCurrentChar(int currentI)
    {
        DialogueHelper dH;
        dH = sentences[currentI];
        return dH.character;
    }

    public DialogueHelper.Mood GetMood(int currentI)
    {
        DialogueHelper dH;
        dH = sentences[currentI];
        return dH.mood;
    }

    public bool ShouldShowImage(int currentI)
    {
        DialogueHelper dH;
        dH = sentences[currentI];
        return dH.showImage;
    }
}
