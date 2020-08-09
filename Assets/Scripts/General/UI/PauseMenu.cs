using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool gameIsPaused = false;
    public GameObject pauseMenuUI;
    public GameObject SkillMenuUi;
    public GameObject keyBindsMenu;
    public GameObject spellHandler;
    private bool keybindsOpen, skillBookOpen, spellHandlerOpen;
    void Update()
    {
        if (InputManager.instance.KeyDown(Keybindings.KeyList.PauseMenu))
        {
            if (keybindsOpen || skillBookOpen || spellHandlerOpen)
                HandleActiveMenu();
            else
            { 
                if (gameIsPaused)
                {
                    Resume(pauseMenuUI);
                }
                else
                {
                    Pause(pauseMenuUI);
                }
            }
        }
        if (InputManager.instance.KeyDown(Keybindings.KeyList.SkillsMenuHotKey))
        {
            if (gameIsPaused)
            {
                if(skillBookOpen)
                    Resume(SkillMenuUi);
                else if(spellHandlerOpen)
                    GoBackFromSpellHandler();
            }
            else
            {
                OpenSkillsMenu();
                skillBookOpen = true;
            }
        }
    }

    public void Resume(GameObject UIElement)
    {
        UIElement.SetActive(false);
        Time.timeScale = 1f;
        gameIsPaused = false;
    }

    void Pause(GameObject UIElement)
    {
        UIElement.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
    }

    private void HandleActiveMenu()
    {
        if (keybindsOpen)
        {
            keyBindsMenu.SetActive(false);
            keybindsOpen = false;
            Pause(pauseMenuUI);
        }
        else if (skillBookOpen)
        {
            SkillMenuUi.SetActive(false);
            skillBookOpen = false;
            Pause(pauseMenuUI);
        }
        else if (spellHandlerOpen)
        {
            GoBackFromSpellHandler();
        }
    }


    public void OpenKeybindsMenu()
    {
        Resume(pauseMenuUI);
        Pause(keyBindsMenu);
        keybindsOpen = true;
    }

    public void OpenSkillsMenu()
    {
        Resume(pauseMenuUI);
        Pause(SkillMenuUi);
        skillBookOpen = true;
    }
    void GoBackFromSpellHandler()
    {
        spellHandler.SetActive(false);
        spellHandlerOpen = false;
        skillBookOpen = true;
        Pause(SkillMenuUi);
    }

    public void OpenSkillHandler(bool isOpening)
    {
        spellHandlerOpen = isOpening;
        skillBookOpen = !isOpening;
    }
}
