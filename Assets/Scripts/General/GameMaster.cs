using UnityEngine;

public class GameMaster : MonoBehaviour
{

    public static GameMaster instance;
    public Vector2 lastCheckPointPosition;
    public Vector2 savePointPosition;
    public GameObject playerInstance;
    [HideInInspector]
    public string aSkillString, bSkillString;
    [HideInInspector]
    public GameObject currentRoom;
    public GameObject firstRoom;
    public Transform spawnPoint;
    [HideInInspector]
    public GameObject brotherInstance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
            Destroy(gameObject);
        playerInstance = GameObject.FindGameObjectWithTag("Player");
        brotherInstance = GameObject.FindGameObjectWithTag("Brother");

        Application.targetFrameRate = 60;
        //Time.timeScale = 0.9f;
    }

    private void Start()
    {
        if (ShouldLoadPlayer())
        {
            StartCoroutine(GameSaveManager.instance.LoadGameAfterDelay(0.2f));
            PlayerPrefs.DeleteKey("LoadPlayer");
        }
    }

    public void UpdateSkillsName(string a, string b) { aSkillString = a; bSkillString = b;}

    public void ShakeCamera(float time, float force)
    {
        StartCoroutine(currentRoom.GetComponent<RoomManagerOne>().virtualCam.GetComponent<ScreenShake>().ShakeyShakey(time, force));
    }

    public void StopCameraShake()
    {
        currentRoom.GetComponent<RoomManagerOne>().virtualCam.GetComponent<ScreenShake>().StopShake();
    }

    public void LoadSavePoint(Vector2 pos)
    {
        savePointPosition = pos;
        //playerInstance.transform.position = 
    }

    public void TeleportPlayerToSave(Vector2 loadPoint)
    {
        playerInstance.transform.position = loadPoint;
        brotherInstance.transform.position = loadPoint;
    }

    private bool ShouldLoadPlayer()
    {
        return PlayerPrefs.HasKey("LoadPlayer");
    }
}
