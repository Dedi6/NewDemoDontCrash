using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchingPlatforms : MonoBehaviour
{

    public GameObject holderA;
    public GameObject holderB;

    public float intervalsUp;
    public float intervalsDown;
    public float yOffset;

    private MovementPlatformer mp;
    private Transform player;

    void Start()
    {
        player = GameMaster.instance.playerInstance.transform;
        mp = GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>();

        mp.teleportedNow += SwitchPlatforms;
    }
    public void SwitchPlatforms()
    {
        holderA.SetActive(!holderA.activeSelf);
        holderB.SetActive(!holderB.activeSelf);
        if (holderA.activeSelf)
            StartCoroutine(PopEnable(holderA));
        else
            StartCoroutine(PopEnable(holderB));
    }

    private IEnumerator PopEnable(GameObject platforms)

    {

        while(platforms.transform.localScale.y <= yOffset)
        {
            platforms.transform.localScale = new Vector3(platforms.transform.localScale.x, platforms.transform.localScale.y + intervalsUp, platforms.transform.localScale.z);
            yield return null; 
        }

        yield return 0;

        while (platforms.transform.localScale.y >= 1)
        {
            platforms.transform.localScale = new Vector3(platforms.transform.localScale.x, platforms.transform.localScale.y - intervalsDown, platforms.transform.localScale.z);
            yield return null;
        }
    }


}
