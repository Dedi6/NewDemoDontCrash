using UnityEngine;

public class Hp_Pickable : MonoBehaviour
{
    public string nameForSave;

    private void Start()
    {
        if (PlayerPrefs.HasKey(nameForSave))
            gameObject.SetActive(false);
    }

    public void HpPickUp()
    {
        // check for number of parts. Right now gives hp immediatly
        GameMaster.instance.playerInstance.GetComponent<Health>().IncreaseHp();
        PlayerPrefs.SetInt(nameForSave, 1);
    }
}
