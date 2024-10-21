using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Events;

public class SkillsManager : MonoBehaviour
{
    public PlayerSkills playerSkills;
    public ManaBar manaBar;
    public Action aSkill, bSkill;
    private int manaASkill, manaBSkill;
    public SkillsUI skillsUI;
    public GameObject player, brother;
    private Theo_Follow brotherScript;
    private MovementPlatformer playerScript;
    [HideInInspector]
    public bool isUsingSkill = false;
    void Start()
    {
        brotherScript = GameMaster.instance.brotherInstance.GetComponent<Theo_Follow>();
        playerScript = GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>();
        playerSkills = new PlayerSkills();
        playerSkills.brotherSkills = brotherScript;
        playerSkills.skillsManager = this;
        playerSkills.playerScript = playerScript;
        skillsUI.SetStartingSkillsButton();
        HandleLoad();
    }

    void Update()
    {
       /* if (!isUsingSkill && InputManager.instance.KeyDown(Keybindings.KeyList.Skill2) && manaBar.HaveEnoughMana(manaASkill))
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
        }*/
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
      /*  else      I merged the Load Skills and Load Player
        {
            GameSaveManager.instance.LoadSkills();
            //GameSaveManager.instance.LoadSkills();
        }*/
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

    public void CannonBallSummon()
    {
        GameObject cannonPrefab = PrefabManager.instance.FindVFX(PrefabManager.ListOfVFX.PlayerSkill_CannonBall);
        GameObject cannonBall_Skill = Instantiate(cannonPrefab, player.transform.position, Quaternion.identity);
        brotherScript.FinishedSkill();
        if(!playerScript.IsFacingRight())
            cannonBall_Skill.transform.Rotate(0.0f, 180.0f, 0.0f);
        cannonBall_Skill.GetComponent<TriggerAction>().triggered.AddListener(delegate { ShootCannonBallNow(cannonBall_Skill.transform); });
    }

    public void ShootCannonBallNow(Transform cannonTransform)
    {
        Debug.Log("WWWOWWWW");
        // summon based on side
        //create cannonball
        // add SFX
        GameObject cannonBallPrefab = PrefabManager.instance.FindVFX(PrefabManager.ListOfVFX.PlayerSkill_CannonBall_Shoot);
        Vector2 posToSpawn = cannonTransform.position;
        Vector2 dir = cannonTransform.rotation.y == 0 ? Vector2.right : Vector2.left;
        GameObject cannonB = Instantiate(cannonBallPrefab, Vector2.zero, Quaternion.identity);
        cannonB.GetComponent<FriendlyAction>().SetPositionAndMovement(dir, 10f, posToSpawn);
      //  AudioManager.instance.ThreeDSound(AudioManager.SoundList.HeightBossCannon, posToSpawn);
    }

}
