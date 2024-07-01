using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using MyBox;

public class PrefabManager : MonoBehaviour
{
    public static PrefabManager instance;
    [System.Serializable]
    public struct NamedVFX
    {
        public string name;
        [SearchableEnum]
        public ListOfVFX vfxName;
        public GameObject vfxGameObject;
    }

    [System.Serializable]
    public struct ManageSprites
    {
        public string name;
        [SearchableEnum]
        public ListOfSprites spriteName;
        public Sprite sprite;
    }

    public NamedVFX[] arrayOfVFX;
    public ManageSprites[] arrayOfSprites;
    public GameObject transitionManager, roomNumber, ghostBullet, defaultBulletPrefab;
    public PlayableDirector currentDirector;

    public enum ListOfVFX
    {
        BulletDissapear,
        PlayerHit,
        EnemyHit,
        LightningBolt,
        HealParticle,
        ThunderWave,
        SmokeBomb,
        RedFrog,
        Meteor,
        RageBossSummon,
        ChatBubblePrefab,
        SkillsUnlockedPop,
        ArrowTrap,
        GhostBulletTrail,
        SwordSummon, 
        CannonSummon,
        FanSummon,
        Tp_Switch,
        Tp_Switch_close,
        PlayerSkill_CannonBall,
        PlayerSkill_CannonBall_Shoot,
        VFX_Jumpstone,
    }

    public enum ListOfSprites
    {
        ThunderBolt,
        ThunderWave,
        PlayerSkill_CannonBall,
        Rice,
        Seaweed,
        Tuna, 
        Beef,
        Pork,
        Veggies,
        Tofu,
        v_Sign,
        x_Sign,
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
            Destroy(gameObject);
    }

    public void PlayVFX(ListOfVFX name, Vector2 position)
    {
        GameObject vfx = Instantiate(FindVFX(name), position, Quaternion.identity);
    }

    public void VFXAngle(ListOfVFX name, Vector2 position, float angle)
    {
        GameObject vfx = Instantiate(FindVFX(name), position, Quaternion.Euler(0, 0, angle));
    }

    public GameObject CreatePrefabAndReturnObject(ListOfVFX name, Vector2 position, float angle)
    {
        GameObject vfx = Instantiate(FindVFX(name), position, Quaternion.Euler(0, 0, angle));
        return vfx;
    }

    public GameObject FindVFX(ListOfVFX name)
    {
        foreach (NamedVFX currentVFX in arrayOfVFX)
        {
            if (currentVFX.vfxName == name)
                return currentVFX.vfxGameObject;
        }
        return null;
    }

    public Sprite GetSprite(ListOfSprites name)
    {
        foreach (ManageSprites currentSprite in arrayOfSprites)
        {
            if (currentSprite.spriteName == name)
                return currentSprite.sprite;
        }
        return null;
    }
        
}

