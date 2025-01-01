using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Pull_Handler : MonoBehaviour
{
    [SerializeField]
    private float holdTime, pullSpeed_Side, pullSpeed_Up, pullDuration;
    private float timer, x_Velocity_Modifier, reach_Memory;
    private PullState _pull_State;
    private bool isTryingToPull, isPulling_Side, is_HitObject, canPull, should_Animate_OnPull;
    private MovementPlatformer player_Script;
    private Animator animator;
    private Rigidbody2D rb;
    private string _animation_String;
    private CapsuleDirection2D _capsule_Direction;
    private GameObject _current_Object_Pulled;
    
    

    [SerializeField]
    private Vector2 check_Capsule_Size_Side, check_Capsule_Size_Up;
    private Vector2 current_Capsule_Size;
    [SerializeField]
    private Transform checkPos_Side, checkPos_Up;
    private Transform current_CheckPos;
    [SerializeField]
    private LayerMask hookable_LayerMask;


    private bool fallBool;
    [SerializeField]
    private float fallSpeed, fallSpeedHigh, cooldown;
    private Coroutine _coroutine_FallSpeed;


    private enum PullState
    {
        Waiting,
        Reaching,
        Pulling,
    }

    private void Start()
    {
        _pull_State = PullState.Waiting;
        player_Script = GetComponent<MovementPlatformer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        player_Script.landedNow += Player_Landed;
    }

    /*void Update()
    {
        
        switch(_pull_State)
        {
            case PullState.Waiting:

                if (InputManager.instance.KeyUp(Keybindings.KeyList.Attack))
                    timer = 0;

                if (!player_Script.Is_Attacking_RightNow())
                    return;

                if (InputManager.instance.GetKey(Keybindings.KeyList.Attack))
                    timer += Time.deltaTime;


                if (timer >= holdTime)
                    StartPull();
                    //player_Script.Start_Pull();

                break;
        }

       

    }*/

    void Update()
    {
        switch (_pull_State)
        {
            case PullState.Waiting:
                if (canPull && reach_Memory > 0 && !player_Script.Is_State_Ignore_Inputs())
                    StartPull();
                break;
        }

        Reach_Memory_Timer();
   //     if(Input.GetKeyDown(KeyCode.L)) GameMaster.instance.ShakeCamera(0.3f, 5f);
    }

    private void FixedUpdate()
    {
        switch (_pull_State)
        {
            case PullState.Reaching:
                SlowDownPlayer();
                Reaching_Check();
                break;
            case PullState.Pulling:
                PullNow();
                break;
            case PullState.Waiting:
                SlowDownFall();
                break;
        }
    }

    private void Reach_Memory_Timer()
    {
        if (reach_Memory >= 0)
            reach_Memory -= Time.deltaTime;
        if (InputManager.instance.KeyDown(Keybindings.KeyList.Shoot))
        {
            reach_Memory = 0.1f;
        }
    }

    private void SlowDownFall()
    {
        if (fallBool)
        {
            float _currentFallSpeed;
            if (rb.velocity.y > 0 && InputManager.instance.GetKey(Keybindings.KeyList.Jump))
                _currentFallSpeed = fallSpeedHigh;
            else
                _currentFallSpeed = fallSpeed;
            rb.velocity += Vector2.up * Physics2D.gravity.y * (_currentFallSpeed - 3) * Time.deltaTime;
        }
    }

    private void SlowDownPlayer()
    {
       // rb.velocity = new Vector2(rb.velocity.x * x_Velocity_Modifier, 0f);
        rb.velocity = new Vector2(rb.velocity.x * x_Velocity_Modifier, rb.velocity.y * 0.2f);
    }

    private void StartPull()
    {
        canPull = false;
        fallBool = false;
        _pull_State = PullState.Reaching;
        _animation_String = Get_AnimationString();
        animator.Play("Player_Pull_Prep_" + _animation_String);
      //  player_Script.Set_AtkAnimation_StallTimer(pullDuration);
        player_Script.SetState_Reaching();

        Handle_CapsuleCheck_Data();

        x_Velocity_Modifier = 1f;
        if(_coroutine_FallSpeed != null) 
            StopCoroutine(_coroutine_FallSpeed);

    }

    private void Reaching_Check()
    {
        if (is_HitObject)
            return;

        Collider2D[] hitHookable_Array = Physics2D.OverlapCapsuleAll(current_CheckPos.position, current_Capsule_Size, _capsule_Direction, 0, hookable_LayerMask);
        foreach (Collider2D currentObject in hitHookable_Array)
        {
            if (currentObject.TryGetComponent(out Pullable_Object target))
            {
                // StartCoroutine(Pull_HitSomething());
                if (!Is_Pull_Orientation_Correct(target))
                    return;
                if (target.Should_Stop_WhenPulling())
                    target.GetComponentInParent<Action_TriggerHitPlayer>().Stop_Movement_ForSeconds(pullDuration);

                should_Animate_OnPull = target.Should_Animate_OnPull();
                _current_Object_Pulled = target.gameObject; // for now. Need to make an array of those

                is_HitObject = true;
                player_Script.Freeze_Game_ForSeconds(0.075f, 0.75f);
                player_Script.SetState_Reaching();
                player_Script.Set_Player_Invincible();
                x_Velocity_Modifier = 0.8f;

                return;
            }
        }
    }

    private bool Is_Pull_Orientation_Correct(Pullable_Object target)
    {
        if (isPulling_Side && target.Can_Pull_Side()) return true;
        if (!isPulling_Side && target.Can_Pull_Up()) return true;

        return false;
    }


    private string Get_AnimationString()
    {
        if (Input.GetAxisRaw("Vertical") > 0)
        {
            isPulling_Side = false;
            return "Up";
        }
        else
        {
            isPulling_Side = true;
            return "Side";
        }
    }

    private void Handle_CapsuleCheck_Data()
    {
        if (isPulling_Side)
        {
            current_CheckPos = checkPos_Side;
            _capsule_Direction = CapsuleDirection2D.Horizontal;
            current_Capsule_Size = check_Capsule_Size_Side;
        }
        else
        {
            current_CheckPos = checkPos_Up;
            _capsule_Direction = CapsuleDirection2D.Vertical;
            current_Capsule_Size = check_Capsule_Size_Up;
        }
    }


    private void PullNow()
    {
        int facingRight_Int = player_Script.IsFacingRight() == true ? 1 : -1;
        Vector2 pull_Vector = isPulling_Side ? new Vector2(facingRight_Int * pullSpeed_Side, 0f) : new Vector2(0f, pullSpeed_Up);
        rb.velocity = pull_Vector;
    }

    public void CheckForPull()
    {
        if (!is_HitObject)
        {
            RevertBack();
            return;
        }

        StartCoroutine(Pull_SwitchState_Coroutine());
        GameMaster.instance.ShakeCamera(0.1f, 1f);
        if(should_Animate_OnPull)
            _current_Object_Pulled.GetComponent<Pullable_Object>().Animate_Now();
    }

    private void RevertBack()
    {
        player_Script.SetStateNormal();
        _pull_State = PullState.Waiting;
        if (player_Script.isGrounded && canPull == false)
            StartCoroutine(Cooldown_Coroutine());

        if (player_Script.isGrounded)
            animator.Play("Player_Idle");
        else
            animator.Play("Player_Falling");
    }

    private IEnumerator Cooldown_Coroutine()
    {
        yield return new WaitForSeconds(cooldown);

        canPull = true;
    }

    private IEnumerator Pull_SwitchState_Coroutine()
    {
        _pull_State = PullState.Pulling;
        animator.Play("Player_Pull_Active_" + _animation_String);
        rb.gravityScale = 0f;
        player_Script.StartIgnoreInput();
        player_Script.Set_Player_Invincible_ForTime(pullDuration);
        player_Script.Reset_Jump_Multiplyer();
        
        yield return new WaitForSeconds(pullDuration);

        player_Script.EndIgnoreInput();
        rb.gravityScale = 3f;
        _pull_State = PullState.Waiting;
        is_HitObject = false;
        canPull = true;
        player_Script.SetStateNormal();

        //Handle_Animations_AfterPull();
        animator.SetBool("IsFalling", true);

        _coroutine_FallSpeed = StartCoroutine(Change_FallSpeed_Bool());

    }

    private void Handle_Animations_AfterPull()
    {
        if(isPulling_Side)
        {
            if (player_Script.isGrounded)
                animator.Play("Player_Idle");
            else
                animator.Play("Player_Falling");
        }
        else
            animator.Play("Player_Jump");

    }

    private IEnumerator Change_FallSpeed_Bool()
    {
        if (!isPulling_Side)
            fallBool = true;

        yield return new WaitForSeconds(1f);

        fallBool = false;
    }

    private void Player_Landed()
    {
        canPull = true;
        fallBool = false;
    }

    public void Set_FallBool_False()
    {
        fallBool = false;
    }

    public void Player_GotHit_Check()
    {
        //if (_pull_State == PullState.Reaching)
      //  {
            _pull_State = PullState.Waiting;
            is_HitObject = false;
            fallBool = false;
            //player_Script.SetStateNormal();
    //    }
    }
}
