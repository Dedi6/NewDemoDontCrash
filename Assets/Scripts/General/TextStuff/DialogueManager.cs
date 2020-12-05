using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{

    public Text nameText;
    public Text dialogueText;

    private Queue<string> sentences;

    private Dialogue.DialogueHelper.Characters character;
    private Dialogue.DialogueHelper.Mood mood;

    void Start()
    {
        sentences = new Queue<string>();
    }


    public void StartDialogue(Dialogue dialgoue)
    {
        nameText.text = dialgoue.name;

        sentences.Clear();

        foreach (Dialogue.DialogueHelper sentence in dialgoue.sentences)
        {
            sentences.Enqueue(sentence.sentence);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if(sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        dialogueText.text = sentence;
    }

    void EndDialogue()
    {

    }
}
