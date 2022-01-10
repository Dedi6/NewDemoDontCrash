using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    public bool isRight = true;
    public Transform spawnPoint;
    public float spawnArrowPer, arrowSpeed;
    public GameObject arrowPrefab;
    public bool isOn;

    void Start()
    {
        if(!isRight)
            transform.Rotate(0.0f, 180.0f, 0.0f);
        StartShooting();
    }

    private IEnumerator ShootArrow()
    {
        AudioManager.instance.PlaySound(AudioManager.SoundList.ArrowShoot);
        GameObject arrow = Instantiate(arrowPrefab, spawnPoint.position, Quaternion.identity, transform);
        Vector2 dir = isRight ? new Vector2(1, 0) : new Vector2(-1, 0);
        arrow.GetComponent<Action_TriggerHitPlayer>().SetMovement(dir, arrowSpeed);

        yield return new WaitForSeconds(spawnArrowPer);

        StartCoroutine(ShootArrow());
    }

    private void OnEnable()
    {
        Invoke("StartShooting", 0.5f);
    }

    private void OnDisable()
    {
        isOn = false;
    }
    void StartShooting()
    {
        if (!isOn)
        {
            StartCoroutine(ShootArrow());
            isOn = true;
        }
    }
}
