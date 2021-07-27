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
    }

    public enum ListOfSprites
    {
        ThunderBolt,
        ThunderWave,
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

