using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Febucci.UI;
using UnityEngine.Playables;
using TMPro;

public class DialogueManager : MonoBehaviour
{

    public Text nameText;
    public TextAnimatorPlayer textAnimatorP;

    public Animator animator;
    public Animator dialogueAnimator;
    public GameObject animationBox;
    public GameObject dialogueArea, yesNoHolder;

    private Queue<string> sentences;

    /// <summary>
    /// Handle all the animation and sounds of the dialogue box
    /// </summary>
    private Dialogue.DialogueHelper.Characters character;
    private Dialogue.DialogueHelper.Mood mood;
    private Dialogue currentDialogue;
    private int currentText;
    private string charString, moodString;
    private bool typeFinished;
    private AudioManager.SoundList currentSound;
    private AudioManager audioM;
    Coroutine coroutine;

    void Start()
    {
        sentences = new Queue<string>();    // the Queue of the dialogue
        audioM = AudioManager.instance;
        textAnimatorP.textAnimator.onEvent += OnEvent;
    }


    public void StartDialogue(Dialogue dialgoue)
    {
        IgnorePlayersInput();
        dialogueArea.SetActive(true);
        animator.SetBool("IsOpen", true);
        currentText = 0;
        currentDialogue = dialgoue;

        //nameText.text = dialgoue.name;  /// in case I want to add a name

        sentences.Clear();

        foreach (Dialogue.DialogueHelper sentence in dialgoue.sentences)
        {
            sentences.Enqueue(sentence.sentence);
        }

        DisplayNextSentence();
    }

    public void SkipSentence()
    {
        if (!typeFinished)
            textAnimatorP.SkipTypewriter();
        else
            DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        typeFinished = false;
        string sentence = sentences.Dequeue();
        textAnimatorP.ShowText(sentence);
        SetCharacter(); // set animation of current character speaking
    }

    void EndDialogue()
    {
        animator.SetBool("IsOpen", false);
        currentDialogue = null;
        currentText = 0;
        StartCoroutine(ShutOffDialogue());
        ResumePlayersInput();
    }

    private IEnumerator ShutOffDialogue()
    {
        yield return new WaitForSeconds(0.3f);

        dialogueArea.SetActive(false);
    }

    public void FinishedSpeaking()
    {
        if(dialogueAnimator.isActiveAndEnabled)
            dialogueAnimator.Play(charString + moodString + "Idle");
        typeFinished = true;
        //  Debug.Log("Done");
    }

    public void SetCharacter()
    {
        if (currentDialogue.ShouldShowImage(currentText))
        {
            animationBox.SetActive(true);
            character = currentDialogue.GetCurrentChar(currentText);
            mood = currentDialogue.GetMood(currentText);
            SetDialogueAnimation();
        }
        else
        {
            animationBox.SetActive(false);
            currentSound = AudioManager.SoundList.DefaultDialogueSpeak;
        }

        currentText++;
    }


    private void SetDialogueAnimation()
    {

        switch (character)
        {
            default: charString = null; break; // Handle defaults
            case Dialogue.DialogueHelper.Characters.Player:
                currentSound = AudioManager.SoundList.PlayerSpeak;      // set the characters sound
                charString = "Player_";                                 // set the string for the animation
                break;
            case Dialogue.DialogueHelper.Characters.Brother:
                currentSound = AudioManager.SoundList.BrotherSpeak;
                charString = "Brother_";
                break;
        }
        switch (mood)
        {
            default: moodString = null; break;
            case Dialogue.DialogueHelper.Mood.Normal:
                moodString = "Normal_";
                break;

            case Dialogue.DialogueHelper.Mood.Happy:
                moodString = "Happy_";
                break;
            case Dialogue.DialogueHelper.Mood.Mad:
                moodString = "Mad_";
                break;
        }

        dialogueAnimator.Play(charString + moodString + "Speak");
    }

    public void CharacterShowed()
    {
        audioM.PlaySound(currentSound);
    }

    public void SkipDialogue() // this trigger events in the last sentence and the start of all the other ones.
    {
        StartCoroutine(SkipDialogueCoru());
        PlayableDirector pd = PrefabManager.instance.currentDirector;
        if (IsCutsceneActive())
            pd.playableGraph.GetRootPlayable(0).SetSpeed(1000); //maybe put in a coroutine to set the speed back to 1. 
    }                                                           // can also put an event in the end of each timeline scene.

    private IEnumerator SkipDialogueCoru()
    {
        yield return 0;

        if (sentences.Count > 0)
        {
            textAnimatorP.SkipTypewriter();
            string sentence = sentences.Dequeue();
            textAnimatorP.ShowText(sentence);
            textAnimatorP.SkipTypewriter();
            StartCoroutine(SkipDialogueCoru());
        }
        else
        {
            textAnimatorP.SkipTypewriter();
            if(!currentDialogue.addDecision)
                EndDialogue();
            else
            {
                A_Dialgoue();
                SkipDialogue();
            }
        }
    }

    private bool IsCutsceneActive()
    {
        if (PrefabManager.instance.currentDirector != null)
        {
            PlayableDirector pd = PrefabManager.instance.currentDirector;
            if (pd.playableGraph.IsValid())
                return true;
            else
                return false;
        }
        else return false;
    }


    void OnEvent(string message)
    {
        switch (message)
        {
            case "test":
                //do something
                Debug.Log("Evenet happened");
                break;
            case "resumeDir":
                PlayableDirector pd = PrefabManager.instance.currentDirector;
                if (pd.playableGraph.IsValid() && pd.playableGraph.GetRootPlayable(0).GetSpeed() == 0f)
                    pd.playableGraph.GetRootPlayable(0).SetSpeed(1);
                break;
            case "yesno":
                coroutine = StartCoroutine(ShowYesNo());
                break;
        }
    }


    private IEnumerator ShowYesNo()
    {
        GameObject aObject = yesNoHolder.transform.GetChild(0).gameObject;
        GameObject bObject = yesNoHolder.transform.GetChild(1).gameObject;
        aObject.GetComponentInChildren<TextMeshProUGUI>().text = currentDialogue.choiceA;
        bObject.GetComponentInChildren<TextMeshProUGUI>().text = currentDialogue.choiceB;

        //yield return new WaitForSeconds(0.1f);

        aObject.SetActive(true);

        yield return new WaitForSeconds(1f);

        bObject.SetActive(true);
    }

    public void A_Dialgoue()
    {
        HideYesNo();
        StartDialogue(currentDialogue.dialogueA.dialogue);
    }

    public void B_Dialgoue()
    {
        HideYesNo();
        StartDialogue(currentDialogue.dialogueB.dialogue);
    }

    void HideYesNo()
    {
        StopCoroutine(coroutine);
        yesNoHolder.transform.GetChild(0).gameObject.SetActive(false);
        yesNoHolder.transform.GetChild(1).gameObject.SetActive(false);
    }

    private void IgnorePlayersInput()
    {
        GameObject player = GameMaster.instance.playerInstance;
        if(player.TryGetComponent(out MovementPlatformer move))
        {
            move.StartIgnoreInput();
        }
        else if(player.TryGetComponent(out TopDownMovement moveTd))
        {
            moveTd.StartIgnoreInput();
        }
    }

    private void ResumePlayersInput()
    {
        if (!IsCutsceneActive())
            HandleInputResume();
    }

    private void HandleInputResume()
    {
        GameObject player = GameMaster.instance.playerInstance;
        if (player.TryGetComponent(out MovementPlatformer move))
        {
            move.EndIgnoreInput();
        }
        else if (player.TryGetComponent(out TopDownMovement moveTd))
        {
            moveTd.EndIgnoreInput();
        }
    }

    public void CutsceneEnded()
    {
        HandleInputResume();
    }

}
