using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerSkills 
{
    public Dictionary<string, HotKeyAbility> skills;
    public List<HotKeyAbility> abilityList;
    public Theo_Follow brotherSkills;
    public SkillsManager skillsManager;
    public MovementPlatformer playerScript;
   
    [System.Serializable]
    public class HotKeyAbility
    {
        public Action activateSkill;
        public Sprite image;
        public int mana;
        public HotKeyAbility(Action actSkill, Sprite img, int manaFor)
        {
            activateSkill = actSkill;
            image = img;
            mana = manaFor;
        }
    }
    [System.Serializable]
    public enum SkillType
    {
        Teleport,
        LightningBolt,
        LightningWave,
        CannonBall,
    }

    public PlayerSkills()
    {
        skills = new Dictionary<string, HotKeyAbility>();
        HandleSkills();
    }

/* this is a function which return true if there is is alreay a skill unlocked
    public bool isSkillUnlocked(SkillType skillType)
    {
        return unlockedSkillTypeList.Contains(skillType); // returns true if there is and false if there isnt
    }*/



    public void HandleSkills()
    {
        PrefabManager manager = PrefabManager.instance;
        skills.Add("ThunderBolt", new HotKeyAbility(ThunderBolt, manager.GetSprite(PrefabManager.ListOfSprites.ThunderBolt), 25));
        skills.Add("ThunderWave", new HotKeyAbility(ThunderWave, manager.GetSprite(PrefabManager.ListOfSprites.ThunderWave), 50));
        skills.Add("CannonBall", new HotKeyAbility(CannonBall, manager.GetSprite(PrefabManager.ListOfSprites.PlayerSkill_CannonBall), 2));
       // skills.Add("ThunderWave", new HotKeyAbility(brotherSkills.LightningAttackStart, manager.GetSprite(PrefabManager.ListOfSprites.ThunderWave), 50));
    }

    public void ThunderBolt()
    {
        brotherSkills.LightningAttackStart();
    }
    
    public void ThunderWave()
    {
        brotherSkills.StartSkill(PrefabManager.ListOfVFX.ThunderWave);
    }

    public void CannonBall()
    {
        skillsManager.CannonBallSummon();
    }

}
