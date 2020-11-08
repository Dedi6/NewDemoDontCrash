using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour
{
    public int numberOfBars;
    public int manaPerBar = 50;
    public GameObject[] manaBarImages;
    public Image[] manaBarFills;
    private int currentManaAmount, maxManaAmount;

    void Start()
    {
        SetManaBars(); // set the mana bars for the current amount
        currentManaAmount = maxManaAmount;
    }

    public void SetManaBars()
    {
        for (int i = 0; i < numberOfBars; i++)
        {
            manaBarImages[i].SetActive(true);
        }
        maxManaAmount = numberOfBars * manaPerBar;
    }

    public void SetManaFull()
    {
        currentManaAmount = maxManaAmount;
        UpdateManaBars();
    }

    public void FillUpMana(int amount)
    {
        if (currentManaAmount == maxManaAmount)
            return;
        if (currentManaAmount + amount > maxManaAmount)
        {
            currentManaAmount = maxManaAmount;
            UpdateManaBars();
        }
        else
        {
            currentManaAmount += amount;
            UpdateManaBars();
        }
    }

    public void UseMana(int amount)
    {
        if (currentManaAmount - amount >= 0)
        {
            currentManaAmount -= amount;
            UpdateManaBars();
        }
        else
        {
            // not enought mana sound or somthing idk
        }
    }

    private void UpdateManaBars()
    {
        for (int i = 0; i < numberOfBars; i++)
        {
            int manaBarMin = i * manaPerBar;
            int manaBarMax = (i + 1) * manaPerBar;

            if(currentManaAmount <= manaBarMin)
            {
                manaBarFills[i].fillAmount = 0f;
            }
            else
            {
                if (currentManaAmount >= manaBarMax)
                    manaBarFills[i].fillAmount = 1f;
                else  // in between
                    manaBarFills[i].fillAmount = (float)(currentManaAmount - manaBarMin) / manaPerBar;
            }
        }
    }

    public bool HaveEnoughMana(int mana)
    {
        if (mana > currentManaAmount)
            return false;
        else
            return true;
    }

    public int GetCurrentMana()
    {
        return currentManaAmount;
    }
}
