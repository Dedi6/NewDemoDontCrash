using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class SkillsManager : MonoBehaviour
{
    public PlayerSkills playerSkills;
    public ManaBar manaBar;
    public Action aSkill, bSkill;
    private int manaASkill, manaBSkill;
    public SkillsUI skillsUI;
    public GameObject player, brother;
    [HideInInspector]
    public bool isUsingSkill = false;
    void Start()
    {
        playerSkills = new PlayerSkills();
        playerSkills.brotherSkills = brother.GetComponent<Follow>();
        playerSkills.playerScript = player.GetComponent<MovementPlatformer>();
        HandleLoad();
    }

    void Update()
    {
        if (!isUsingSkill && InputManager.instance.KeyDown(Keybindings.KeyList.Skill2) && manaBar.HaveEnoughMana(manaASkill))
        {
            isUsingSkill = true;
            aSkill();
            manaBar.UseMana(manaASkill);
        }
        if (!isUsingSkill && InputManager.instance.KeyDown(Keybindings.KeyList.Skill1) && manaBar.HaveEnoughMana(manaBSkill))
        {
            isUsingSkill = true;
            bSkill();
            manaBar.UseMana(manaBSkill);
        }
    }

    public void SetSkill(string actionName, bool settingFirstSkill)
    {
        if(settingFirstSkill)
        {
            if (actionName != null)
            {
                aSkill = playerSkills.skills.Single(s => s.Key == actionName).Value.activateSkill;
                int manaV = playerSkills.skills.Single(s => s.Key == actionName).Value.mana;
                manaASkill = manaV;
            }
            else
            {
                aSkill = null;
                manaASkill = 0;
            }
            GameMaster.instance.aSkillString = actionName;
        }
        else
        {
            if(actionName != null)
            {
                bSkill = playerSkills.skills.Single(s => s.Key == actionName).Value.activateSkill;
                int manaV = playerSkills.skills.Single(s => s.Key == actionName).Value.mana;
                manaBSkill = manaV;
            }
            else
            {
                bSkill = null;
                manaBSkill = 0;
            }
            GameMaster.instance.bSkillString = actionName;
        }
    }


    public Sprite GetImage(string actionName)
    {
        return playerSkills.skills.Single(s => s.Key == actionName).Value.image;
    }

    private void HandleLoad()
    {
        bool firstTimeLogging = IsFirstTimePlaying();
        
        if (firstTimeLogging)
        {
            skillsUI.SetNewArray();
            PlayerPrefs.SetInt("skillsPointer", 0);
            skillsUI.UnlockSkill("ThunderBolt");
            SetSkill("ThunderBolt", true);
            SetSkill("ThunderBolt", false);
            skillsUI.SetStartingSkillsButton();
            //GameSaveManager.instance.SavePLayerData();
        }
        else
        {
            GameSaveManager.instance.LoadSkills();
            //GameSaveManager.instance.LoadSkills();
        }
    }

    private bool IsFirstTimePlaying()
    {
        if (!PlayerPrefs.HasKey("FirstTimePlaying"))
        {
            PlayerPrefs.SetInt("FirstTimePlaying", 1);
            return true;
        }
        else
            return false;
    }

}
