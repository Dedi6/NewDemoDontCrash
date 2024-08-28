using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject menuMaster;
    [SerializeField]
    private GameObject firstStartButton, newAndLoadButtons;


    private void Start()
    {
        CheckIfFirstTime();
    }

    void CheckIfFirstTime()
    {
        if (GameSaveManager.instance.IsSaveFile())
            SetStartAndLoad(false);
        else
            SetStartAndLoad(true);
    }

    void SetStartAndLoad(bool firstTime)
    {
        firstStartButton.SetActive(firstTime);
        newAndLoadButtons.SetActive(!firstTime);
    }

    public void PlayGame()
    {
        // First PlayAnimation

        Destroy(menuMaster);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private IEnumerator Test()
    {
        Destroy(menuMaster);

        yield return new WaitForSeconds(.1f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
        
    public void StartAndLoad()
    {
        // play animation


        PlayerPrefs.SetInt("LoadPlayer", 1);
        GameSaveManager saveM = GameSaveManager.instance;
        Destroy(menuMaster);
        SceneManager.LoadScene(saveM.GetLastScene());
        saveM.LoadGame();
    }
}
