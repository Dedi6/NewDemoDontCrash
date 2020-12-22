using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool gameIsPaused = false;
    public GameObject pauseMenuUI;
    public GameObject SkillMenuUi;
    public GameObject keyBindsMenu;
    public GameObject spellHandler;
    public GameObject audioMenu;
    public GameObject controlsMenu;
    public GameObject cheatsMenu;
    private bool keybindsOpen, skillBookOpen, spellHandlerOpen, controlsOpen, audioOpen, cheatsOpen;
    
    private void Start()
    {
        Cursor.visible = false;
        SetAudioSliders();
    }

    void Update()
    {
        if (InputManager.instance.KeyDown(Keybindings.KeyList.PauseMenu))
        {
            if (keybindsOpen || skillBookOpen || spellHandlerOpen || controlsOpen || audioOpen || cheatsOpen)
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
        Cursor.visible = false;
    }

    void Pause(GameObject UIElement)
    {
        Cursor.visible = true;
        UIElement.SetActive(true);
        Time.timeScale = 0f;
        gameIsPaused = true;
    }

    public void HandleActiveMenu()
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
        else if (controlsOpen)
        {
            controlsMenu.SetActive(false);
            controlsOpen = false;
            Pause(pauseMenuUI);
        }
        else if (audioOpen)
        {
            audioMenu.SetActive(false);
            audioOpen = false;
            Pause(pauseMenuUI);
            AudioManager.instance.SaveMixerStats();
        }
        else if (cheatsOpen)
        {
            cheatsMenu.SetActive(false);
            cheatsOpen = false;
            Pause(pauseMenuUI);
        }
    }


    public void OpenKeybindsMenu()
    {
        Resume(controlsMenu);
        Pause(keyBindsMenu);
        keybindsOpen = true;
    }

    public void OpenSkillsMenu()
    {
        Resume(pauseMenuUI);
        Pause(SkillMenuUi);
        skillBookOpen = true;
    }

    public void OpenAudioMenu()
    {
        Resume(pauseMenuUI);
        Pause(audioMenu);
        audioOpen = true;
    }

    public void OpenControlsMenu()
    {
        Resume(pauseMenuUI);
        Pause(controlsMenu);
        controlsOpen = true;
    }

    public void OpenCheatsMenu()
    {
        Resume(pauseMenuUI);
        Pause(cheatsMenu);
        cheatsOpen = true;
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

    public void SetAudioSliders()
    {
        Slider[] sliders = audioMenu.GetComponentsInChildren<Slider>();
        sliders[0].value = PlayerPrefs.GetFloat("MixerMaster");
        sliders[1].value = PlayerPrefs.GetFloat("MixerMusic");
        sliders[2].value = PlayerPrefs.GetFloat("MixerSFX");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    /*   public void GoBackToMenu()    /// should have this up sometime in the future
       {

       }*/
}
