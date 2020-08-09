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
        GameSaveManager.instance.LoadSkills();
    }

    void Update()
    {
        if (!isUsingSkill && InputManager.instance.KeyDown(Keybindings.KeyList.Skill1) && manaBar.HaveEnoughMana(manaASkill))
        {
            isUsingSkill = true;
            aSkill();
            manaBar.UseMana(manaASkill);
        }
        if (!isUsingSkill && InputManager.instance.KeyDown(Keybindings.KeyList.Skill2) && manaBar.HaveEnoughMana(manaBSkill))
        {
            isUsingSkill = true;
            bSkill();
            manaBar.UseMana(manaBSkill);
        }

        if (Input.GetMouseButtonDown(0))
            Debug.Log(isUsingSkill);
   
        if (Input.GetKeyDown(KeyCode.Y))
            skillsUI.UnlockSkill("ThunderBolt");
        if (Input.GetKeyDown(KeyCode.T))
            skillsUI.UnlockSkill("ThunderWave");
        if (Input.GetKeyDown(KeyCode.R))
             PlayerPrefs.SetInt("skillsPointer", 0);
    }

    public void SetSkill(string actionName, bool settingFirstSkill)
    {
        if(settingFirstSkill)
        {
            aSkill = playerSkills.skills.Single(s => s.Key == actionName).Value.activateSkill;
            int manaV = playerSkills.skills.Single(s => s.Key == actionName).Value.mana;
            manaASkill = manaV;
            GameMaster.instance.aSkillString = actionName;
        }
        else
        {
            bSkill = playerSkills.skills.Single(s => s.Key == actionName).Value.activateSkill;
            int manaV = playerSkills.skills.Single(s => s.Key == actionName).Value.mana;
            manaBSkill = manaV;
            GameMaster.instance.bSkillString = actionName;
        }
    }


    public Sprite GetImage(string actionName)
    {
        return playerSkills.skills.Single(s => s.Key == actionName).Value.image;
    }

}
