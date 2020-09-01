using UnityEngine;
using UnityEngine.Events;

public class UnlockSpell : MonoBehaviour
{
    public UnityEvent OnTrigger;
    private GameObject player;
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.layer == 11) // player
        {
            OnTrigger.Invoke();
            Destroy(gameObject);
            AudioManager.instance.PlaySound(AudioManager.SoundList.UnlockedSkill);
            PrefabManager.instance.PlayVFX(PrefabManager.ListOfVFX.SkillsUnlockedPop, transform.position);
        }
    }

    public void UnlockBullet()
    {
        player.GetComponent<MovementPlatformer>().bulletUnlocked = true;
        PlayerPrefs.SetInt("BulletUnlocked", 1);
        Destroy(gameObject);
        PrefabManager.instance.PlayVFX(PrefabManager.ListOfVFX.SkillsUnlockedPop, transform.position);
    }


    public void SetBulletLocked()
    {
        if (PlayerPrefs.HasKey("BulletUnlocked"))
            PlayerPrefs.DeleteKey("BulletUnlocked");
        player.GetComponent<MovementPlatformer>().bulletUnlocked = false;
        Destroy(gameObject);
    }

    public void IncreaseHP()
    {
        player.GetComponent<Health>().IncreaseHp();
    }

}
