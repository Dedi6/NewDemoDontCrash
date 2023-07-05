using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class GameSaveManager : MonoBehaviour
{
    public static GameSaveManager instance;
    private static readonly string keyword = "p";
    
    [System.Serializable]
    public class KeybindForPlatforms
    {
        public Keybindings keybindings;
        public string path;
    }

    public KeybindForPlatforms[] arrayOfBindings;
    [HideInInspector]
    public Slot[] playerInventoryHolder;

    [System.Serializable]
    public class PlayerData {

        public float savePointX;
        public float savePointY;
        public int currentScene;
        public string aSkillName, bSkillName;
        public string[] arrayOfSkills;
        public PlayerData(string[] array)
        {
            aSkillName = GameMaster.instance.aSkillString;
            bSkillName = GameMaster.instance.bSkillString;
            savePointX = GameMaster.instance.savePointPosition.x;
            savePointY = GameMaster.instance.savePointPosition.y;
            currentScene = SceneManager.GetActiveScene().buildIndex;
            arrayOfSkills = array;
        }
    }

    [System.Serializable]
    public class InventoryData
    {
        public Slot[] slotsArray;
        public float testFloat;
        public Inventory_Item itemTest;
        public Inventory_Item[] itemArray;
        public int[] itemEmumInt;
        public int[] amountOfItem;
        public bool[] isFree;
    }

    public SkillsUI skillsLoader;
    public InventoryController inventoryController;

    [HideInInspector]
    public int lastScene;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(this);
        DontDestroyOnLoad(this);
    }

    public void ForceCreateFile()
    {
        Directory.CreateDirectory(Application.persistentDataPath + "/game_save");
        Directory.CreateDirectory(Application.persistentDataPath + "/game_save/playerData");
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

    public bool DoesSaveFileExist(string path)
    {
        return Directory.Exists(Application.persistentDataPath + path);
    }

    public void SaveKeybindings()
    {
        if (!IsSaveFile())
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/game_save");
        }
        if (!Directory.Exists(Application.persistentDataPath + "/game_save/keybindings")) // create a keybindigs directory
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/game_save/keybindings");
        }
        
        foreach (KeybindForPlatforms binding in arrayOfBindings)
        {
            BinaryFormatter binaryF = new BinaryFormatter();
            FileStream stream = File.Create(Application.persistentDataPath + binding.path);
            var json = JsonUtility.ToJson(binding.keybindings);
            binaryF.Serialize(stream, json);
            stream.Close();
        }
       // SavePLayerData();
    }

    public void SavePLayerData()
    {
        BinaryFormatter binaryF = new BinaryFormatter();
        FileStream stream = File.Create(Application.persistentDataPath + "/game_save/playerData/playerData.txt");
        PlayerData data = new PlayerData(skillsLoader.unlockedSkillsArray);
        binaryF.Serialize(stream, data);
        stream.Close();
    }

    public bool IsSaveFile()
    {
        return Directory.Exists(Application.persistentDataPath + "/game_save");
    }

    public IEnumerator LoadGameAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        LoadGame();
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
        if (File.Exists(Application.persistentDataPath + "/game_save/playerData/playerData.txt"))
        {
            FileStream file = File.Open(Application.persistentDataPath + "/game_save/playerData/playerData.txt", FileMode.Open);
            PlayerData data = bf.Deserialize(file) as PlayerData;
            //PlayerData data = JsonUtility.FromJsonOverwrite((string)bf.Deserialize(file), binding.keybindings);
            file.Close();
        }
        LoadSkills();
        LoadPlayer();
    }

    public void LoadSkills()
    {
        PlayerData data = LoadPlayerData();
        
        
        /*string[] d = new string[2];  //fix 1
        d[0] = "ThunderBolt";
        d[1] = "ThunderWave";
        skillsLoader.LoadSkills("ThunderBolt", "ThunderBolt", d);*/

        skillsLoader.LoadSkills(data.aSkillName, data.bSkillName, data.arrayOfSkills);
    }

    public void LoadPlayer()
    {
        PlayerData data = LoadPlayerData();
        Vector2 point = new Vector2(data.savePointX, data.savePointY);
        GameMaster gm = GameMaster.instance;
        gm.savePointPosition = point;
        gm.TeleportPlayerToSave(point);
    }

    public int GetLastScene()
    {
        PlayerData data = LoadPlayerData();
        return data.currentScene;
    }

    public static PlayerData LoadPlayerData()
    {
        if (File.Exists(Application.persistentDataPath + "/game_save/playerData/playerData.txt"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/game_save/playerData/playerData.txt", FileMode.Open);

            PlayerData data = bf.Deserialize(file) as PlayerData;

            file.Close();

            return data;
        }
        else
            return null;
    }

    public void SaveMemoryStream(string path, MemoryStream memoryStream)
    {
        if (!Directory.Exists(Application.persistentDataPath + "/game_save/race_Data")) // create a keybindigs directory
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/game_save/race_Data");
        }

        BinaryFormatter binaryF = new BinaryFormatter();
        FileStream stream = File.Create(Application.persistentDataPath + "/game_save/race_Data/" + path + ".txt");
        binaryF.Serialize(stream, memoryStream);
        stream.Close();
    }

    public MemoryStream LoadMemoryStream(string path)
    {
        if (File.Exists(Application.persistentDataPath + "/game_save/race_Data/" + path + ".txt"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/game_save/race_Data/" + path + ".txt", FileMode.Open);
            MemoryStream memoStream = bf.Deserialize(file) as MemoryStream;
            file.Close();
            return memoStream;
        }
        else
            return null;
    }

    
    public void DeleteSaveFile()
    {
        Directory.Delete(Application.persistentDataPath + "/game_save", true);
        PlayerPrefs.DeleteKey("FirstTimePlaying");
    }



    public void Save_Inventory(string path, Inventory_Base playerInventory)
    {
        if (!Directory.Exists(Application.persistentDataPath + "/game_save/player_inventory"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/game_save/player_inventory");
        }

        string savePath = Application.persistentDataPath + "/game_save/player_inventory/" + path + ".txt";

        var saveObject = new InventoryData
        {
            slotsArray = playerInventory.slots,
            itemEmumInt = playerInventory.GetItemIntArray(),
            amountOfItem = playerInventory.GetAmountArray(),
            isFree = playerInventory.GetIsFreeArray(),
            itemArray = playerInventory.GetItemsArray()
        };

        FileStream stream = File.Create(savePath);
        stream.Dispose();
        var json = JsonUtility.ToJson(saveObject);
        var ecnryptedJson = EncryptDecrypt(json);
        File.WriteAllText(savePath, ecnryptedJson);
        stream.Close();
    }

    private static string EncryptDecrypt(string data)
    {
        string result = "";

        for (int i = 0; i < data.Length; i++)
        {
            result += (char)(data[i] ^ keyword[i % keyword.Length]);
        }

        return result;
    }

    public Slot[] Load_Inventory(string path)
    {
        string _pathOfInventory = Application.persistentDataPath + "/game_save/player_inventory/" + path + ".txt";
        if (File.Exists(_pathOfInventory))
        {
         //   BinaryFormatter bf = new BinaryFormatter();
          //  var data = new InventoryData();
            FileStream stream = File.Open(_pathOfInventory, FileMode.Open);
            stream.Dispose();
            //    File.ReadAllText(_pathOfInventory);
            string json = EncryptDecrypt(File.ReadAllText(_pathOfInventory));
            Debug.Log(json);
            InventoryData data = JsonUtility.FromJson<InventoryData>(json);
            //InventoryData data = JsonUtility.FromJson<InventoryData>(File.ReadAllText(_pathOfInventory));
            stream.Close();
            for (int i = 0; i < data.slotsArray.Length; i++)
            {
                // if(data.itemEmumInt[i] != -1)
                //  data.slotsArray[i].currentItem.item = (Inventory_Item.Item)data.itemEmumInt[i];
                data.slotsArray[i].currentItem = data.itemArray[i];
                data.slotsArray[i].amount = data.amountOfItem[i];
                data.slotsArray[i].isFree = data.isFree[i];
                if (data.itemArray[i] != null)
                    data.slotsArray[i].UpdateSlot(data.itemArray[i], data.amountOfItem[i]);
                else
                    data.slotsArray[i].ClearSlot();
            }
            return data.slotsArray;
        }
        else
            return null;
    }



    public void Save_Inventory_Storage(Storage_Interact storage, string path)
    {
        string _pathOfInventory = Application.persistentDataPath + "/game_save/player_inventory/" + path + ".txt";

        if (!Directory.Exists(Application.persistentDataPath + "/game_save/player_inventory/"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/game_save/player_inventory/");
        }

        FileStream stream = File.Create(_pathOfInventory);
        stream.Dispose();

        using (
            var writer = new BinaryWriter(File.Open(_pathOfInventory, FileMode.Open))
        )
        {
            storage.Save(new GameDataWriter(writer));
        }
    }

    public void Load_Inventory_Storage(Storage_Interact storage, string path)
    {
        string _pathOfInventory = Application.persistentDataPath + "/game_save/player_inventory/" + path + ".txt";

        using (
            var reader = new BinaryReader(File.Open(_pathOfInventory, FileMode.Open))
        )
        {
            storage.Load(new GameDataReader(reader));
        }
    }
}
