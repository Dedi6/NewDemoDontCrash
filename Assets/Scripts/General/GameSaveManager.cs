﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager instance;
    
    [System.Serializable]
    public class KeybindForPlatforms
    {
        public Keybindings keybindings;
        public string path;
    }

    public KeybindForPlatforms[] arrayOfBindings;

    [System.Serializable]
    public class PlayerData {

        public float savePointX;
        public float savePointY;
        public string aSkillName, bSkillName;
        public string[] arrayOfSkills;
        public PlayerData(string[] array)
        {
            aSkillName = GameMaster.instance.aSkillString;
            bSkillName = GameMaster.instance.bSkillString;
            savePointX = GameMaster.instance.savePointPosition.x;
            savePointY = GameMaster.instance.savePointPosition.y;
            arrayOfSkills = array;
        }
    }

    public SkillsUI skillsLoader;


    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);
        DontDestroyOnLoad(this);
    }

    public void SaveGame()
    {
        if(!IsSaveFile())
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/game_save");
        }
        if(!Directory.Exists(Application.persistentDataPath + "/game_save/keybindings")) // create a keybindigs directory
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/game_save/keybindings");
        }
        if (!Directory.Exists(Application.persistentDataPath + "/game_save/playerData")) // create a playerData directory
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/game_save/playerData");
        }
        foreach (KeybindForPlatforms binding in arrayOfBindings)
        {
            BinaryFormatter binaryF = new BinaryFormatter();
            FileStream stream = File.Create(Application.persistentDataPath + binding.path);
            var json = JsonUtility.ToJson(binding.keybindings);
            binaryF.Serialize(stream, json);
            stream.Close();
        }
        SavePLayerData();
    }

    public void SavePLayerData()
    {
        BinaryFormatter binaryF = new BinaryFormatter();
        FileStream stream = File.Create(Application.persistentDataPath + "/game_save/playerData.txt");
        PlayerData data = new PlayerData(skillsLoader.unlockedSkillsArray);
        binaryF.Serialize(stream, data);
        stream.Close();
    }

    public bool IsSaveFile()
    {
        return Directory.Exists(Application.persistentDataPath + "/game_save");
    }

    public void LoadGame()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/game_save/keybindings"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/game_save/keybindings");
        }
        BinaryFormatter bf = new BinaryFormatter();
        foreach (KeybindForPlatforms binding in arrayOfBindings)
        {
            if(File.Exists(Application.persistentDataPath + binding.path))
            {
                FileStream file = File.Open(Application.persistentDataPath + binding.path, FileMode.Open);
                JsonUtility.FromJsonOverwrite((string)bf.Deserialize(file), binding.keybindings);
                file.Close();
            }
        }
        if (File.Exists(Application.persistentDataPath + "/game_save/playerData.txt"))
        {
            FileStream file = File.Open(Application.persistentDataPath + "/game_save/playerData.txt", FileMode.Open);
            PlayerData data = bf.Deserialize(file) as PlayerData;
            //PlayerData data = JsonUtility.FromJsonOverwrite((string)bf.Deserialize(file), binding.keybindings);
            file.Close();
        }
        LoadSkills();
    }

    public void LoadSkills()
    {
        PlayerData data = LoadPlayerData();
        /*
        string[] d = new string[2];
        d[0] = "ThunderBolt";
        d[1] = "ThunderWave";
        skillsLoader.LoadSkills("ThunderBolt", "ThunderWave", d);*/
        skillsLoader.LoadSkills(data.aSkillName, data.bSkillName, data.arrayOfSkills);
        Vector2 point = new Vector2(data.savePointX, data.savePointY);
        GameMaster.instance.savePointPosition = point;
    }

    public static PlayerData LoadPlayerData()
    {
        if (File.Exists(Application.persistentDataPath + "/game_save/playerData.txt"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/game_save/playerData.txt", FileMode.Open);

            PlayerData data = bf.Deserialize(file) as PlayerData;

            file.Close();

            return data;
        }
        else
            return null;
    }
}
