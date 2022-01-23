using UnityEngine;

public class HealCrystal : MonoBehaviour
{
    public Animator animator;

    public void CreateCrystal()
    {
        animator.SetTrigger("Create");
    }
    public void DestroyCrystal()
    {
        animator.SetTrigger("Destroy");
        GameMaster.instance.playerInstance.GetComponent<Health>().health += 4;
    }

    public void EnableCollider()
    {
        GetComponent<CircleCollider2D>().enabled = true;
    }
}
