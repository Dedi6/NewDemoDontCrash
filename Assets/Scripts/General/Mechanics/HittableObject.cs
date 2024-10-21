using UnityEngine;

public class HittableObject : MonoBehaviour
{
    public UnityEngine.Events.UnityEvent triggered;
    [SerializeField]
    private bool shouldTriggerOnce, isBush;
    [SerializeField] GameObject bushSprite;

    public void HitObject()
    {
        triggered.Invoke();
        if (shouldTriggerOnce)
            GetComponent<CircleCollider2D>().enabled = false;
    }

    private void OnEnable()
    {
        if(isBush && PlayerPrefs.HasKey("Demoman_FirstTime"))
        {
            bushSprite.SetActive(true);
            gameObject.SetActive(false);
            /*Animator bushAnimator = GetComponent<Animator>();
            bushAnimator.SetTrigger("IsEmpty");
            bushAnimator.enabled = false;
            this.enabled = false;*/
        }
    }
}
