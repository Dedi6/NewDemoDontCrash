using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Follow : MonoBehaviour
{
    public float speedWhenClose, speedWhenFar, speedWhenSkill;
    private float currentSpeed = 12, smokeBombDelay = 0.1f;
    private bool isUsingSkill = false;
    [HideInInspector]
    public bool facingRight = false, playerFacingRight;
    public Transform target;

    public Animator animator;
    [HideInInspector]
    public PrefabManager.ListOfVFX currentSkillPrefab;
    private SkillsManager handlerSkills;
    [HideInInspector]
    public Vector2 skillDirection;

    private State state;
    public enum State
    {
        Normal,
        Attacking,
        UsingSkill,
    }

    private void Start()
    {
        state = State.Normal;
        handlerSkills = GetComponent<SkillsManager>();
    }
    void Update()
    {
        switch(state)
        {
            case State.Normal:
                if (Vector2.Distance(transform.position, target.position) > 0.3f)
                {
                    transform.position = Vector2.MoveTowards(transform.position, target.position, currentSpeed * Time.deltaTime);
                    if (transform.position.x > target.position.x && facingRight)
                        Flip();
                    else if (transform.position.x < target.position.x && !facingRight)
                        Flip();
                }
                else if (Vector2.Distance(transform.position, target.position) < 0.3f && isUsingSkill)
                    StartCoroutine(UseSkill());
                break;
            case State.Attacking:
                break;
            case State.UsingSkill:
                break;
        }

        if (transform.childCount > 1)
            Destroy(transform.GetChild(0).gameObject);
    }

   
    void Flip()
    {
        
        facingRight = !facingRight;
        transform.Rotate(0.0f, 180.0f, 0.0f);

        foreach (Transform child in transform)
        {
            float fixX = facingRight ? -1 : 1;
            if(child.rotation.y != 0)
                child.eulerAngles = new Vector3(0f, 0f, 0f);
            child.localPosition = new Vector2(fixX, 2);
        }
    }



    private void OnTriggerEnter2D(Collider2D other)
    {
        currentSpeed = speedWhenClose;
        animator.SetFloat("speedMultiplier", 1f);
        if (isUsingSkill)
            StartCoroutine(UseSkill());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        currentSpeed = speedWhenFar;
        animator.SetFloat("speedMultiplier", 1.5f);
    }

    private void StartSkillTransformation()
    {
        isUsingSkill = true;
        currentSpeed = speedWhenSkill;
        animator.SetFloat("speedMultiplier", 2f);
    }

    public void SetStateNormal()
    {
        state = State.Normal;
        isUsingSkill = false;
        handlerSkills.isUsingSkill = false;
    }

    public void LightningAttackStart()
    {
        state = State.Attacking;
        animator.SetTrigger("LightningAttack");
    }
    private void LightningAttack()
    {
        GameObject enemy = FindClosestEnemy();
        if(enemy != null)
        {
            RaycastHit2D rayToFloor = Physics2D.Raycast(enemy.transform.position, Vector2.down, Mathf.Infinity);
            Vector2 spawnPosition = new Vector2(rayToFloor.point.x, rayToFloor.point.y + 10);
            PrefabManager.instance.PlayVFX(PrefabManager.ListOfVFX.LightningBolt, spawnPosition);
        }
        handlerSkills.isUsingSkill = false;
    }
    private GameObject FindClosestEnemy()
    {
        float distanceToClosestEnemy = Mathf.Infinity;
        GameObject closestEnemy = null;
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("EnemyAlive");

        foreach(GameObject currentEnemy in allEnemies)
        {
            float currentDistance = (currentEnemy.transform.position - this.transform.position).sqrMagnitude;
            if(currentDistance < distanceToClosestEnemy)
            {
                distanceToClosestEnemy = currentDistance;
                closestEnemy = currentEnemy;
            }
        }

        return closestEnemy;
    }

    public void ThunderWaveSkillStart()
    {
        StartSkillTransformation();
    }

    public void StartSkill(PrefabManager.ListOfVFX prefab)
    {
        currentSkillPrefab = prefab;
        StartSkillTransformation();
        skillDirection = target.GetComponentInParent<MovementPlatformer>().directionPressed;
        playerFacingRight = target.GetComponentInParent<MovementPlatformer>().IsFacingRight();
    }


    private IEnumerator UseSkill()
    {
        isUsingSkill = false;
        PrefabManager pref = PrefabManager.instance;
        pref.PlayVFX(PrefabManager.ListOfVFX.SmokeBomb, transform.position);

        yield return new WaitForSeconds(smokeBombDelay);

        Dissapear();
        pref.PlayVFX(currentSkillPrefab, transform.position);
    }

    public void FinishedSkill()
    {
        handlerSkills.isUsingSkill = false;
    }

    void Dissapear()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        state = State.UsingSkill;
    }

    public void StartApearCoroutine()
    {
        StartCoroutine(AppearAgain());
    }
    
    private IEnumerator AppearAgain()
    {
        PrefabManager.instance.PlayVFX(PrefabManager.ListOfVFX.SmokeBomb, transform.position);

        yield return new WaitForSeconds(0.1f);

        GetComponent<SpriteRenderer>().enabled = true;
        ResetState();
    }

    void ResetState()
    {
        state = State.Normal;
        currentSpeed = speedWhenClose;
        animator.SetFloat("speedMultiplier", 1.5f);
    }
}
