using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrotherMove : MonoBehaviour
{
    private bool isActive, usingKeyboard;
    private Theo_Follow mainScript;
    private Cinemachine.CinemachineVirtualCamera currentCam;
    private MovementPlatformer player;
    [SerializeField]
    private Transform pointToFollow;
    private Vector3 originalPos;
    private float moveSpeed;
    private InputManager inputM;
    private float axisVert, axisHori;

    void Start()
    {
        mainScript = GetComponent<Theo_Follow>();
        GameMaster gm = GameMaster.instance;
        player = gm.playerInstance.GetComponent<MovementPlatformer>();
        moveSpeed = mainScript.GetCurrentSpeed();
        inputM = gm.GetComponent<InputManager>();
        originalPos = pointToFollow.localPosition;
        usingKeyboard = inputM.IsUsingKeyboard();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if(isActive)
            {
                mainScript.SetStateNormal();
                isActive = false;
                currentCam.m_Follow = player.transform;
                pointToFollow.localPosition = originalPos;
                player.SetStateNormal();
              //  GetComponent<CircleCollider2D>().enabled = true;
                GetComponent<SkillsManager>().isUsingSkill = false;
            }
            else
            {
                StartControl();
            }
        }

        if(isActive)
        {
            HandleMoveInput();
            int facingRight = player.IsFacingRight() ? 1 : -1;
            pointToFollow.Translate(new Vector2(axisHori * facingRight, axisVert) * moveSpeed * Time.deltaTime);
            if (axisHori == 0 && axisVert == 0)
                pointToFollow.position = transform.position;
        }
    }

    void StartControl()
    {
        // mainScript.SetStateSkill();
        ResetAxis();
        isActive = true;
        currentCam = GameMaster.instance.currentRoom.GetComponent<RoomManagerOne>().virtualCam.GetComponent<Cinemachine.CinemachineVirtualCamera>();
        currentCam.m_Follow = transform;
       // GetComponent<CircleCollider2D>().enabled = false;
        player.StartIgnoreInput();
        GetComponent<SkillsManager>().isUsingSkill = true;
    }
    
   private void HandleMoveInput()
   {
        // Use new Input System - GetMoveInput() handles both keyboard and gamepad
        Vector2 moveVector = InputManager.instance.GetMoveInput();
        axisHori = moveVector.x;
        axisVert = moveVector.y;
        
        // For keyboard, maintain discrete key tracking for precise control
        if (usingKeyboard)
        {
            if (inputM.KeyDown(Keybindings.KeyList.Up))
                axisVert = 1;
            if (inputM.KeyDown(Keybindings.KeyList.Down))
                axisVert = -1;
            if (inputM.KeyDown(Keybindings.KeyList.Right))
                axisHori = 1;
            if (inputM.KeyDown(Keybindings.KeyList.Left))
                axisHori = -1;
            if (inputM.KeyUp(Keybindings.KeyList.Up))
                axisVert = 0;
            if (inputM.KeyUp(Keybindings.KeyList.Down))
                axisVert = 0;
            if (inputM.KeyUp(Keybindings.KeyList.Right))
                axisHori = 0;
            if (inputM.KeyUp(Keybindings.KeyList.Left))
                axisHori = 0;
        }
        // For gamepad, GetMoveInput() already returns normalized values
    }

    private void ResetAxis()
    {
        axisHori = 0;
        axisVert = 0;
    }
}
