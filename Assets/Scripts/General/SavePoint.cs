using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    public float waitBeforeSave = 0.5f;
    public Transform loadPoint;
    public Animator animator;
    private GameMaster gm;
    private MovementPlatformer playerScript;

    private bool isSaving = false, canSave;
  
    void Start()
    {
        gm = GameMaster.instance;
        playerScript = gm.playerInstance.GetComponent<MovementPlatformer>();
    }


    private void Update()
    {
        if (canSave && playerScript.isGrounded && IsPressingDown() && !isSaving) // player collided from above
        {
            isSaving = true;
            StartCoroutine(Save());
        }
    }

    private bool IsPressingDown()
    {
        return InputManager.instance.KeyDown(Keybindings.KeyList.Down) || Input.GetAxisRaw("Vertical") < 0;
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
        StartCoroutine(playerScript.SwitchStateIgnore(0.51f));
        playerScript.animator.SetTrigger("Heal");
        playerScript.animator.speed = 0.8f;

        yield return new WaitForSeconds(0.5f);

        animator.SetBool("Active", true);

        playerScript.animator.speed = 1f;
        gm.savePointPosition = loadPoint.position;
        GameSaveManager.instance.SaveGame();
        isSaving = false;

        yield return new WaitForSeconds(0.1f);

        playerScript.FullRestore();
    }

    public void SaveNow()
    {
        StartCoroutine(Save());
    }

    public void ShakeTheCamera()
    {
        gm.ShakeCamera(0.1f, 1f);
        GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>().enabled = true;
        AudioManager.instance.PlaySound(AudioManager.SoundList.Save);
    }
}
