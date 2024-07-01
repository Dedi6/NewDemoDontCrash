using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    public float waitBeforeSave = 0.5f;
    public Transform loadPoint;
    [SerializeField] private Transform theoPosition, viniEndPos;
    public Animator animator;
    private GameMaster gm;
    private MovementPlatformer playerScript;

    private bool isSaving = false, canSave, finishedAnimation;
  
    void Start()
    {
        gm = GameMaster.instance;
        playerScript = gm.playerInstance.GetComponent<MovementPlatformer>();
    }


    private void Update()
    {
        if (canSave && !isSaving && playerScript.isGrounded && IsPressingDown()) // player collided from above
        {
            isSaving = true;
            StartCoroutine(Save());
        }

        if(isSaving && finishedAnimation && IsPressingUp())
        {
            GetUp();
        }
    }

    private bool IsPressingDown()
    {
        return InputManager.instance.KeyDown(Keybindings.KeyList.Down) || Input.GetAxisRaw("Vertical") < 0;
    }

    private bool IsPressingUp()
    {
        return InputManager.instance.KeyDown(Keybindings.KeyList.Up) || Input.GetAxisRaw("Vertical") > 0;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.layer == 11)
            canSave = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.layer == 11)
            canSave = false;
    }

    private IEnumerator Save()
    {
        playerScript.rb.velocity = Vector2.zero;

        yield return new WaitForSeconds(0.1f);

        playerScript.rb.velocity = Vector2.zero;

        isSaving = true;
        // StartCoroutine(playerScript.SwitchStateIgnore(0.51f));
        //  playerScript.animator.SetTrigger("Heal");
        // playerScript.animator.speed = 0.8f;
        playerScript.StartIgnoreInput();
        playerScript.GetComponent<SpriteRenderer>().enabled = false;
        animator.SetTrigger("Start");
        gm.brotherInstance.GetComponent<Theo_Follow>().StartSave_Animation(theoPosition);
        
        GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>().enabled = false;

        yield return new WaitForSeconds(waitBeforeSave);

       // animator.SetBool("Active", true);

        playerScript.animator.speed = 1f;
        gm.savePointPosition = loadPoint.position;
        GameSaveManager.instance.SaveGame();
        finishedAnimation = true;
        //isSaving = false;

        yield return new WaitForSeconds(0.1f);

        playerScript.FullRestore();
    }

    private void GetUp()
    {
        isSaving = false;
        animator.SetTrigger("GetUp");
        playerScript.transform.position = viniEndPos.position;
    }

    public void Exit_SavePoint()
    {
        playerScript.EndIgnoreInput();
        playerScript.GetComponent<SpriteRenderer>().enabled = true;
        if (!playerScript.IsFacingRight()) playerScript.Flip();

        finishedAnimation = false;
        gm.brotherInstance.GetComponent<Theo_Follow>().Reset_Save();
        animator.ResetTrigger("GetUp");
        StopAllCoroutines();
    }

    public void SaveNow()
    {
        StartCoroutine(Save());
    }

    public void ShakeTheCamera()
    {
        //gm.ShakeCamera(0.1f, 1f);
        GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>().enabled = true;
        AudioManager.instance.PlaySound(AudioManager.SoundList.Save);
        Start_TeaCoroutine();
    }


    private IEnumerator DrinkTea_Coroutine()
    {
        float rnd_Time = Random.Range(5f, 12f);

        yield return new WaitForSeconds(rnd_Time);

        animator.SetTrigger("Tea");
    }

    public void Start_TeaCoroutine()
    {
        StartCoroutine(DrinkTea_Coroutine());
    }

    private void OnDisable()
    {
        GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>().enabled = false;
    }
}
