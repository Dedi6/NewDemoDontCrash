using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThunderWaveSkill : MonoBehaviour
{

    public float xOffset, yOffset, stompShakeTime, stompShakeForce, stompSpawnHeight, waveSpawnSpacing, waveSpeed;
    public Transform attackPoint;
    private Vector2 thunderSpawnPos;
    public GameObject thunderPrefab;
    private bool facingRight, finishedAnimation, finishedWave;
    private Follow brotherScript;

    private void Start()
    {
        brotherScript = GameObject.FindGameObjectWithTag("Brother").GetComponent<Follow>();
        facingRight = SetFacingRightBool();
        SetOffset();
        if (facingRight)
            Flip();
    }
    public void ThunderSkill()
    {
        int sign = facingRight ? 1 : -1;
        Vector2 direction = new Vector2(sign, 0);
        GameMaster.instance.ShakeCamera(stompShakeTime, stompShakeForce);
        AudioManager.instance.PlaySound(AudioManager.SoundList.MoonBossAttack);
        thunderSpawnPos = new Vector2(attackPoint.position.x, attackPoint.position.y + stompSpawnHeight);
        RaycastHit2D rayToWall = Physics2D.Raycast(attackPoint.transform.position, direction, 100, 1 << 8);
        StartCoroutine(ThunderWave(sign, rayToWall.point.x));
    }


    private IEnumerator ThunderWave(int sign, float wallXPos)
    {
        AudioManager.instance.PlaySound(AudioManager.SoundList.MoonBossZap);
        thunderSpawnPos.x += sign * waveSpawnSpacing;
        if ((thunderSpawnPos.x < wallXPos && facingRight) || (thunderSpawnPos.x > wallXPos && !facingRight))
        {
            yield return new WaitForSeconds(waveSpeed);
            GameObject stompSkill = Instantiate(thunderPrefab, thunderSpawnPos, Quaternion.identity, transform.parent);
            StartCoroutine(ThunderWave(sign, wallXPos));
        }
        else
        {
            if (finishedAnimation)
                Destroy(gameObject);
            finishedWave = true;
        }
    }
    

    private bool SetFacingRightBool()
    {
        if (brotherScript.skillDirection.Equals(Vector2.zero))
            return brotherScript.playerFacingRight;
        else
            return brotherScript.skillDirection.x > 0 ? true : false;
    }

    private void Flip()
    {
        transform.Rotate(0.0f, 180.0f, 0.0f);
    }

    public void EnableSkills()
    {
        brotherScript.FinishedSkill();
        GetComponent<SpriteRenderer>().enabled = false;
        finishedAnimation = true;
        brotherScript.StartApearCoroutine();
        if (finishedWave)
            Destroy(gameObject);
    }

    private void SetOffset()
    {
        float x = facingRight ? xOffset : -xOffset;
        transform.position = new Vector3(transform.position.x + x, transform.position.y + yOffset, 0);
    }
}

