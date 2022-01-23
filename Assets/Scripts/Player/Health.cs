using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{

    public int health;
    public int numberOfHearts;

    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    void Awake()
    {
        SetHP();
    }

    void FixedUpdate()
    {

        if(health > 4 * numberOfHearts)
        {
            health = 4 * numberOfHearts;
        }
        
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < health / 4)
            {
                hearts[i].fillAmount = 1;
            }
            else
            {
                hearts[i].fillAmount = 0;
                if(i == health / 4)
                    hearts[i].fillAmount = GetFillPercentile(health % 4);
            }

            if(i < numberOfHearts)
                hearts[i].gameObject.SetActive(true);   //  hearts[i].enabled = true;
            else
                hearts[i].gameObject.SetActive(false); // hearts[i].enabled = false;
        }
    }

    private float GetFillPercentile(int i)
    {
        switch(i)
        {
            case 0:
                return 0;
            case 1:
                return 0.25f;
            case 2:
                return 0.5f;
            case 3:
                return 0.75f;
        }
        return 1f;
    }

    public bool IsFullHealth()
    {
        if (health / 4 == numberOfHearts)
            return true;
        else
            return false;
    }

    public void DealDamage(int dmg)
    {
        health -= dmg;
        if (health <= 0)
        {
            AudioManager.instance.PlaySound(AudioManager.SoundList.RespawnSound);
            health = numberOfHearts;
            StartCoroutine(GetComponent<MovementPlatformer>().TransitionStart());
        }
    }

    public void FullHeal()
    {
        health = numberOfHearts * 4;
    }

    public void IncreaseHp()
    {
        FullHeal();
        health++;
        numberOfHearts++;
        int newHP = numberOfHearts;
        PlayerPrefs.SetInt("HP", newHP);
    }

    public void SetHpAsInt(int hp)
    {
        health = hp;
        numberOfHearts = hp;
    }

    private void SetHP()
    {
        int hp;
        if (PlayerPrefs.HasKey("HP"))
            hp = PlayerPrefs.GetInt("HP");
        else
            hp = 7;
        health = hp * 4;
        numberOfHearts = hp;
    }


}
