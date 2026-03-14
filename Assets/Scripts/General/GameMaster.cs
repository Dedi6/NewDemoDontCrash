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
    public GameObject brotherInstance, confinedCamera;

    private Rigidbody2D player_Rigidbody;

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
        
        if (playerInstance != null)
            player_Rigidbody = playerInstance.GetComponent<Rigidbody2D>();
        else
            Debug.LogWarning("GameMaster: Player GameObject not found with tag 'Player'");

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

    public void Shake_SpecificCamera(float time, float force, GameObject cameraObject)
    {
        cameraObject.GetComponent<ScreenShake>().ShakeyShakey(time, force);
    }

    public void Shake_ConfinedCamera(float time, float force, GameObject cameraObject)
    {
        cameraObject.GetComponent<ScreenShake>().ShakeyShakey(time, force);
    }

    public void ShakeCameraTopDown(float time, float force)
    {
        StartCoroutine(firstRoom.GetComponent<RoomManagerOne>().virtualCam.GetComponent<ScreenShake>().ShakeyShakey(time, force));
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

    public Rigidbody2D Get_PlayerRigidBody()
    {
        return player_Rigidbody;
    }

    public Player_Pull_Handler.Parry_State Get_ParryState()
    {
        return playerInstance.GetComponent<Player_Pull_Handler>().Get_ParryState();
    }

    public static void Succesful_Parry()
    {
        Debug.Log("Parry successful!");

        GameMaster.instance.playerInstance.GetComponent<Player_Pull_Handler>().Succesful_Parry();
    }
}
