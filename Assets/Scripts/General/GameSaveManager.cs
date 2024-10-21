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
        public int[] itemEmumInt;
        public int[] amountOfItem;
    }

    public SkillsUI skillsLoader;
  //  public InventoryController inventoryController;

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

  /*  private void Start()
    {
        LoadGame();
    }*/

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha6))
            LoadGame();
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
            string savePath = Application.persistentDataPath + binding.path;
            var saveObject = binding.keybindings.Get_KeybindsDict();


            FileStream stream = File.Create(savePath);
            stream.Dispose();
            var json = JsonConvert.SerializeObject(saveObject);
            var ecnryptedJson = EncryptDecrypt(json);
            File.WriteAllText(savePath, ecnryptedJson);
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

            string savePath = Application.persistentDataPath + binding.path;
            var saveObject = binding.keybindings.Get_KeybindsDict();


            FileStream stream = File.Create(savePath);
            stream.Dispose();
            var json = JsonConvert.SerializeObject(saveObject);
            var ecnryptedJson = EncryptDecrypt(json);
            File.WriteAllText(savePath, ecnryptedJson);
            stream.Close();

        }
        // SavePLayerData();
    }

    public void SavePLayerData()
    {
        string savePath = Application.persistentDataPath + "/game_save/playerData/playerData.txt";
        var saveObject = new PlayerData(skillsLoader.unlockedSkillsArray); ;

        FileStream stream = File.Create(savePath);
        stream.Dispose();
        var json = JsonUtility.ToJson(saveObject);
        var ecnryptedJson = EncryptDecrypt(json);
        File.WriteAllText(savePath, ecnryptedJson);
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
        Load_Keybinds();
       
        LoadPlayer();
    }

    public void Load_Keybinds()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/game_save/keybindings"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/game_save/keybindings");
        }
        foreach (KeybindForPlatforms binding in arrayOfBindings)
        {
            if (File.Exists(Application.persistentDataPath + binding.path))
            {
                string savePath = Application.persistentDataPath + binding.path;

                FileStream stream = File.Open(savePath, FileMode.Open);
                stream.Dispose();
                string json = EncryptDecrypt(File.ReadAllText(savePath));
                Dictionary<string, string> loaded_Data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                stream.Close();

                binding.keybindings.Load_DictToBinds(loaded_Data);
            }
        }
    }

   /* public void LoadSkills()
    {
        PlayerData data = LoadPlayerData();
        
        
        /*string[] d = new string[2];  //fix 1
        d[0] = "ThunderBolt";
        d[1] = "ThunderWave";
        skillsLoader.LoadSkills("ThunderBolt", "ThunderBolt", d);

      //  skillsLoader.LoadSkills(data.aSkillName, data.bSkillName, data.arrayOfSkills);
    }*/

    public void LoadPlayer()
    {
        PlayerData data = LoadPlayerData();


    /*    string[] d = new string[2];  //fix 1
        d[0] = "ThunderBolt";
        d[1] = "ThunderWave";
        skillsLoader.LoadSkills("ThunderBolt", "ThunderBolt", d);*/

        skillsLoader.LoadSkills(data.aSkillName, data.bSkillName, data.arrayOfSkills);

        Vector2 point = new Vector2(data.savePointX, data.savePointY);
        GameMaster gm = GameMaster.instance;
        gm.savePointPosition = point;
        gm.TeleportPlayerToSave(point);


    }


    public bool IsPlayerDataNull()
    {

        PlayerData data = LoadPlayerData();
        return data == null;
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
            string savePath = Application.persistentDataPath + "/game_save/playerData/playerData.txt";

            FileStream stream = File.Open(savePath, FileMode.Open);
            stream.Dispose();
            string json = EncryptDecrypt(File.ReadAllText(savePath));
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            stream.Close();
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
            amountOfItem = playerInventory.GetAmountArray(),
            itemEmumInt = playerInventory.GetItemIntArray(),
        };

        FileStream stream = File.Create(savePath);
        stream.Dispose();
        var json = JsonUtility.ToJson(saveObject);
        var ecnryptedJson = EncryptDecrypt(json);
        File.WriteAllText(savePath, ecnryptedJson);
        stream.Close();
    }

    private static string EncryptDecrypt(string data)   // if i'll decide to encrypt the data
    {
        return data;

        string result = "";

        for (int i = 0; i < data.Length; i++)
        {
            result += (char)(data[i] ^ keyword[i % keyword.Length]);
        }

        return result;
    }


    public void Load_Storage_Independent(string path, Slot[] slots)
    {
        string _pathOfInventory = Application.persistentDataPath + "/game_save/player_inventory/" + path + ".txt";
        if (File.Exists(_pathOfInventory))
        {
            FileStream stream = File.Open(_pathOfInventory, FileMode.Open);
            stream.Dispose();
            string json = EncryptDecrypt(File.ReadAllText(_pathOfInventory));
            InventoryData data = JsonUtility.FromJson<InventoryData>(json);
            stream.Close();
            for (int i = 0; i < slots.Length; i++)
            {
                if (data.itemEmumInt[i] != -1) // isn't null
                {
                    Inventory_Item currentItem = InventoryController.instance.GetItem_Base((Inventory_Item.Item)data.itemEmumInt[i]);
                    slots[i].UpdateSlot(currentItem, data.amountOfItem[i]);
                }
                else
                    slots[i].LoadEmptySlot();
            }
        }
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

    public void Load_Inventory_Storage(Storage_Interact storage, string path)   // might delete
    {
        string _pathOfInventory = Application.persistentDataPath + "/game_save/player_inventory/" + path + ".txt";

        using (
            var reader = new BinaryReader(File.Open(_pathOfInventory, FileMode.Open))
        )
        {
            storage.Load(new GameDataReader(reader));
        }
    }


    public void Save_Storage_List(List<string> list, string path)
    {
        if (!Directory.Exists(Application.persistentDataPath + "/game_save/player_inventory"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/game_save/player_inventory");
        }

        string savePath = Application.persistentDataPath + "/game_save/player_inventory/" + path + ".txt";

        FileStream stream = File.Create(savePath);
        stream.Dispose();
        var json = JsonConvert.SerializeObject(list);
        var ecnryptedJson = EncryptDecrypt(json);
        File.WriteAllText(savePath, ecnryptedJson);
        stream.Close();
    }

    public List<string> Get_Storage_List(string path)
    {
        string _pathOfInventory = Application.persistentDataPath + "/game_save/player_inventory/" + path + ".txt";

        if (File.Exists(_pathOfInventory))
        {
            FileStream stream = File.Open(_pathOfInventory, FileMode.Open);
            stream.Dispose();
            string json = EncryptDecrypt(File.ReadAllText(_pathOfInventory));
            var value = JsonConvert.DeserializeObject<List<string>>(json);
            return value;
        }

        else
            return null;
    }
}
