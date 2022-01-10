using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

public class RunRecordTest : MonoBehaviour
{
    private float previousY, previousX;
    private bool recording, recordingInitialized, loading, isGrounded;
    private Animator animator;
    private State state;
    public ParticleSystem particles;
    public string raceName;

    private Coroutine corout;
    private Transform player;
    private MemoryStream memoryStream = null;
    private BinaryWriter binaryWriter = null;
    private BinaryReader binaryReader = null;

    private enum State
    {
        Idle,
        Running,
        Jumping,
        Falling,
    }

    void Start()
    {
        player = GameMaster.instance.playerInstance.transform;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            recording = !recording;
            if (!recording)
            {
                ResetReplayFrame();
                loading = true;
            }
            else
                recordingInitialized = false;
        }
        if (recording)
            RecordInputs();
        else if (!recording && loading)
            LoadInputs();

        if(Input.GetKeyDown(KeyCode.M))
        {
            memoryStream = GameSaveManager.instance.LoadMemoryStream(raceName);
            binaryWriter = new BinaryWriter(memoryStream);
            binaryReader = new BinaryReader(memoryStream);
            recording = false;
            ResetReplayFrame();
            loading = true;
        }

        //HandleAnimator();
    }


    private void FixedUpdate()
    {
        HandleAnimator();
        previousY = transform.position.y;
        previousX = transform.position.x;

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 8)
            isGrounded = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 8)
            isGrounded = false;
    }

    private void HandleAnimator()
    {
        if(isGrounded)
        {
            if(state == State.Jumping || state == State.Falling)    // landed
            {
                CreateDust();
                //sfx land
            }

            if(IsMoving() && state != State.Running)
                SetAnimation("Race_Run", State.Running);
            else if(!IsMoving() && state != State.Idle)
                SetAnimation("Race_Idle", State.Idle);

        }
        else
        {
            if(previousY > transform.position.y && state != State.Falling && Moved())
                SetAnimation("Race_Fall", State.Falling);
            else if(previousY < transform.position.y && state != State.Jumping && Moved())
            {
                CreateDust();
                SetAnimation("Race_Jump", State.Jumping);
                //sfx jump
            }
        }
    }

    private bool Moved()
    {
        return Mathf.Abs(previousY - transform.position.y) > 0.05f ? true : false;
    }

    private void SetAnimation(string animName, State newState)
    {
        animator.Play(animName);
        state = newState;
    }


    private bool IsMoving()
    {
        float distance = Mathf.Abs(previousX - transform.position.x);
        return (distance > 0.01f);
    }

    
    private void RecordInputs()
    {
        if (!recordingInitialized)
        {
            InitializeRecording();
            memoryStream.SetLength(0); 
        }

        SaveTransform();
    }

    private void LoadInputs()
    {

        if (memoryStream.Position >= memoryStream.Length)
        {
            loading = false;
            return;
        }
        LoadTransform();
    }


    private void InitializeRecording()
    {
        memoryStream = new MemoryStream();
        binaryWriter = new BinaryWriter(memoryStream);
        binaryReader = new BinaryReader(memoryStream);
        recordingInitialized = true;
    }

    private void ResetReplayFrame()
    {
        memoryStream.Seek(0, SeekOrigin.Begin);
        binaryWriter.Seek(0, SeekOrigin.Begin);
        GameSaveManager.instance.SaveMemoryStream(raceName, memoryStream);
    }

    private void SaveTransform()
    {
        binaryWriter.Write(player.transform.localPosition.x);
        binaryWriter.Write(player.transform.localPosition.y);
        binaryWriter.Write(player.transform.localPosition.z);
        binaryWriter.Write(player.transform.rotation.y);
    }

    private void LoadTransform()
    {
        float x = binaryReader.ReadSingle();
        float y = binaryReader.ReadSingle();
        float z = binaryReader.ReadSingle();
        float yRotation = binaryReader.ReadSingle();
        transform.position = new Vector3(x, y, z);
        transform.eulerAngles = new Vector3(0f, yRotation * 180, 0f);
    }

    private void CreateDust()
    {
        particles.Play();
    }



}
