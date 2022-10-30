using UnityEngine;

public class HealCrystal : MonoBehaviour
{
    public Animator animator;
    [SerializeField]
    private bool isActive;

    private void Start()
    {
        if(isActive)
        {
            EnableCollider();
            animator.Play("HealCrystal_Idle");
        }
    }

    public void CreateCrystal()
    {
        animator.SetTrigger("Create");
        AudioManager.instance.PlaySound(AudioManager.SoundList.HealCrystal_Create);
    }
    public void DestroyCrystal()
    {
        animator.SetTrigger("Destroy");
        GameMaster.instance.playerInstance.GetComponent<Health>().health += 4;
        AudioManager.instance.PlaySound(AudioManager.SoundList.HealCrystal_Hit);
    }

    public void EnableCollider()
    {
        GetComponent<CircleCollider2D>().enabled = true;
    }
}
