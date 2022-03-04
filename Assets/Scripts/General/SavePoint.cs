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

    private bool isSaving = false;
  
    void Start()
    {
        gm = GameMaster.instance;
        playerScript = gm.playerInstance.GetComponent<MovementPlatformer>();
    }


    private void OnCollisionStay2D(Collision2D col)
    {
        if (!isSaving && InputManager.instance.KeyDown(Keybindings.KeyList.Down) && col.transform.position.y > (transform.position.y + 2f) && col.gameObject.layer == 11) // player collided from above
        {
            isSaving = true;
            StartCoroutine(Save());
        }
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
