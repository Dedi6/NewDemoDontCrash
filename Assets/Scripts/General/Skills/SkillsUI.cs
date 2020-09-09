using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillsUI : MonoBehaviour
{
    public Button firstButton, secondButton;
    private bool settingFirstSkill, skillAssignFirstButton;
    public SkillsManager skillManager;
    public GameObject skillTab, generalSkillsTab;
    [HideInInspector]
    public string[] unlockedSkillsArray;
    public Transform[] arrayOfPositions;
    private int currentTransform = 0;

    private void Start()
    {
        if(unlockedSkillsArray == null)
          unlockedSkillsArray = new string[arrayOfPositions.Length]; // remeber to change the length, also change the playerPrefs in of currentTransform so that it will not save itself.
    }
    public void SetNewArray()
    {
        unlockedSkillsArray = new string[arrayOfPositions.Length];
    }

    public void PressedButtonOne()
    {
        settingFirstSkill = true;
        OpenSkillMenu(true);
    }
    public void PressedButtonTwo()
    {
        settingFirstSkill = false;
        OpenSkillMenu(true);
    }

    public void OpenSkillMenu(bool opening)
    {
        skillTab.SetActive(opening);
        generalSkillsTab.SetActive(!opening);
        GetComponentInParent<PauseMenu>().OpenSkillHandler(opening);
    }

    public void UpdateSkill(string skillName)
    {
        skillManager.SetSkill(skillName, settingFirstSkill);
        if (settingFirstSkill)
            firstButton.GetComponentInChildren<Image>().sprite = skillManager.GetImage(skillName);
        else
            secondButton.GetComponentInChildren<Image>().sprite = skillManager.GetImage(skillName);
        OpenSkillMenu(false);
        GameSaveManager.instance.SavePLayerData();
    }

    public void SetImageNull(bool firstSkill)
    {
        if(firstSkill)
        {
            firstButton.GetComponentInChildren<Image>().sprite = null;
        }
        else
            secondButton.GetComponentInChildren<Image>().sprite = null;
    }

    public void LoadSkills(string skillA, string skillB, string[] arrayOfSkills)
    {
        skillManager.SetSkill(skillA, settingFirstSkill);
        skillManager.SetSkill(skillB, !settingFirstSkill);
        GameMaster.instance.UpdateSkillsName(skillA, skillB);
        firstButton.GetComponentInChildren<Image>().sprite = skillManager.GetImage(skillA);
        secondButton.GetComponentInChildren<Image>().sprite = skillManager.GetImage(skillB);
        SetSkillsArray(arrayOfSkills);
        bool shootUnlocked = PlayerPrefs.HasKey("BulletUnlocked");
        skillManager.player.GetComponent<MovementPlatformer>().bulletUnlocked = shootUnlocked;
    }

    private void SetSkillsArray(string[] array)
    {
        int amountOfSkills = PlayerPrefs.GetInt("skillsPointer");
        PlayerPrefs.SetInt("skillsPointer", 0);
        string[] temporary = new string[arrayOfPositions.Length];
        array.CopyTo(temporary, 0);
        unlockedSkillsArray = temporary;
        skillAssignFirstButton = true;
        for (int i = 0; i < amountOfSkills; i++)
        {
            UnlockSkill(unlockedSkillsArray[i]);
        }
    }

    private void UpdateImage(string skillName)
    {
        skillManager.SetSkill(skillName, settingFirstSkill);
    }

    public void SetStartingSkillsButton()
    {
        firstButton.GetComponentInChildren<Image>().sprite = skillManager.GetImage("ThunderBolt");
        secondButton.GetComponentInChildren<Image>().sprite = skillManager.GetImage("ThunderBolt");
    }

    public void UnlockSkill(string skillName)
    {
        currentTransform = PlayerPrefs.GetInt("skillsPointer");
        foreach (Transform button in skillTab.transform)
        {
            if (button.name == skillName)
            {
                //Debug.Log(unlockedSkillsArray + " " + currentTransform + " " + unlockedSkillsArray[0]);
                unlockedSkillsArray[currentTransform] = skillName;
                SetButton(button.gameObject);
                if (skillAssignFirstButton)
                {
                    skillTab.GetComponent<PreventDeselectionGroup>().firstButton = button.GetChild(0).gameObject;
                    skillAssignFirstButton = false;
                }
            }
        }
    }

    void SetButton(GameObject button)
    {
        button.transform.position = arrayOfPositions[currentTransform].position;
        button.SetActive(true);
        currentTransform++;
        PlayerPrefs.SetInt("skillsPointer", currentTransform);
        PlayerPrefs.Save();
    }
}
