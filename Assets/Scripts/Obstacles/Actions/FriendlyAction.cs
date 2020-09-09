using UnityEngine;

public class FriendlyAction : MonoBehaviour
{
    public int damage;
    public AudioManager.SoundList soundEffect;
    private void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.layer == 12)
        {
            AudioManager.instance.PlaySound(soundEffect);
            col.GetComponent<Enemy>().TakeDamage(damage);
        }
    }
}
