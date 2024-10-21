using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Cinemachine;

public class EditorRoomSpawner : EditorWindow
{
    GameObject playerInstance;
    GameObject previousRoom;
    BuildPath buildTo;
    GameObject roomToMove;



    [System.Serializable]
    enum BuildPath
    {
        Right,
        Left,
        Up,
        Down,
    }


    [MenuItem("Tools/EditorRoomSpawner")]
    public static void ShowWindow()
    {
        GetWindow(typeof(EditorRoomSpawner));
    }

    private void OnGUI()
    {
        GUILayout.Label("Spawn New Room", EditorStyles.boldLabel);

        //roomNumber = EditorGUILayout.IntField("Room Number", roomNumber);
        playerInstance = EditorGUILayout.ObjectField("Player Instance", playerInstance, typeof(GameObject), true) as GameObject;
        previousRoom = EditorGUILayout.ObjectField("Previous room", previousRoom, typeof(GameObject), true) as GameObject;

        buildTo = (BuildPath)EditorGUILayout.EnumPopup("Build To", buildTo);


        if (GUILayout.Button("Spawn Room", GUILayout.Height(30)))
            SpawnRoom();

        GUILayout.Space(40);
        roomToMove = EditorGUILayout.ObjectField("Room To Move", previousRoom, typeof(GameObject), true) as GameObject;
        if (GUILayout.Button("Move Room", GUILayout.Height(30)))
            SetRoomPosition(roomToMove);
    }

    private void SpawnRoom()
    {
        GameObject newRoom = PrefabUtility.InstantiatePrefab(Resources.Load("Room"), previousRoom.transform.parent) as GameObject;
        GameObject vCam = PrefabUtility.InstantiatePrefab(Resources.Load("vCamRoom")) as GameObject;


        vCam.transform.parent = newRoom.transform;
        newRoom.GetComponent<RoomManagerOne>().virtualCam = vCam;
        vCam.GetComponent<CinemachineConfiner>().m_BoundingShape2D = newRoom.GetComponent<PolygonCollider2D>();
        vCam.GetComponent<Cinemachine.CinemachineVirtualCamera>().Follow = playerInstance.transform;

        if (previousRoom != null)
            SetRoomPosition(newRoom);

        //newRoom.name = "Room" + " " + roomNumber.ToString();

        GameObjectUtility.EnsureUniqueNameForSibling(newRoom);

        Undo.RegisterCreatedObjectUndo(newRoom, $"Create Object: {newRoom.name}");
        Selection.activeGameObject = newRoom;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());   // make changes requiring a save
    }

    void SetRoomPosition(GameObject newRoom)
    {
        Vector2[] points = previousRoom.GetComponent<PolygonCollider2D>().points;
        float xDistance = (Mathf.Abs(points[0].x) + Mathf.Abs(points[1].x)) + 0.05f;
        float yDistance = (Mathf.Abs(points[1].y) + Mathf.Abs(points[2].y)) + 0.05f;

        switch(buildTo)
        {
            case BuildPath.Right:
                newRoom.transform.position = new Vector2(previousRoom.transform.position.x + xDistance, previousRoom.transform.position.y);
                break;
            case BuildPath.Left:
                newRoom.transform.position = new Vector2(previousRoom.transform.position.x - xDistance, previousRoom.transform.position.y);
                break;
            case BuildPath.Up:
                newRoom.transform.position = new Vector2(previousRoom.transform.position.x, previousRoom.transform.position.y + yDistance);
                break;
            case BuildPath.Down:
                newRoom.transform.position = new Vector2(previousRoom.transform.position.x, previousRoom.transform.position.y - yDistance);
                break;
        }
    }
}
