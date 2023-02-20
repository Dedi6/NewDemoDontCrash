using UnityEngine;

public class BrotherMoveEnable : MonoBehaviour
{
    private BrotherMove brotherM;
    void Start()
    {
        brotherM = GameMaster.instance.brotherInstance.GetComponent<BrotherMove>();
    }

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.layer == 11) // 11 is player
            brotherM.enabled = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.layer == 11) // 11 is player
            brotherM.enabled = false;
    }
}
