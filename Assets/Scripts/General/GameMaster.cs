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

        Application.targetFrameRate = 60;
    }

    public void UpdateSkillsName(string a, string b) { aSkillString = a; bSkillString = b;}

    public void ShakeCamera(float time, float force)
    {
        StartCoroutine(currentRoom.GetComponent<RoomManagerOne>().virtualCam.GetComponent<ScreenShake>().ShakeyShakey(time, force));
    }

    public void LoadSavePoint(Vector2 pos)
    {
        savePointPosition = pos;
        //playerInstance.transform.position = 
    }

}
