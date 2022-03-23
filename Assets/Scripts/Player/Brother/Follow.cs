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
    public GameObject crystal;
    private Transform playerTarget, player;
    private bool isCreatingCrystal;
    private ManaBar manaBar;
    private InputManager inputM;


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
        playerTarget = target;
        player = GameMaster.instance.playerInstance.transform;
        manaBar = player.GetComponent<ManaBar>();
        inputM = InputManager.instance;
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
                else 
                {
                    if (isUsingSkill)
                        StartCoroutine(UseSkill());
                    if (isCreatingCrystal)
                        StopCrystal();
                }
                break;
            case State.Attacking:
                break;
            case State.UsingSkill:
                break;
        }

        if (transform.childCount > 1)
            Destroy(transform.GetChild(0).gameObject);

        if (inputM.KeyDown(Keybindings.KeyList.Heal) && CanCreateCrystal())
            CreateCrystal();


    }

    private bool CanCreateCrystal()
    {
        return manaBar.HaveEnoughMana(25);
    }

    private void CreateCrystal()
    {
        isCreatingCrystal = true;
        float lookingRight;
        Vector2 dirWall;
        if (player.GetComponentInParent<MovementPlatformer>().IsFacingRight())
        {
            lookingRight = 2f;
            dirWall = Vector2.right;
        }
        else
        {
            lookingRight = -2f;
            dirWall = Vector2.left;
        }
       // float lookingRight = player.GetComponentInParent<MovementPlatformer>().IsFacingRight() ? 2f : -2f;
        Vector2 rayOrigin = new Vector2(player.transform.position.x + lookingRight, player.transform.position.y);
        RaycastHit2D rayToFloor = Physics2D.Raycast(rayOrigin, Vector2.down, 7, 1 << 8);
        RaycastHit2D rayToWall = Physics2D.Raycast(player.transform.position, dirWall, 2, 1 << 8);

        
        if (rayToFloor && !rayToWall)
        {
            Vector3 newTarget = new Vector3(rayToFloor.point.x, rayToFloor.point.y + 0.6f, 0f);
            GameObject newCrytsal = Instantiate(crystal, newTarget, Quaternion.identity, rayToFloor.transform);
            target = newCrytsal.transform;
        }
        else
        {
            RaycastHit2D rayFromPlayer = Physics2D.Raycast(player.position, Vector2.down, 7, 1 << 8);
            if (rayFromPlayer)
            {
                Vector3 newTarget = new Vector3(rayFromPlayer.point.x, rayFromPlayer.point.y + 0.6f, 0f);
                GameObject newCrytsal = Instantiate(crystal, newTarget, Quaternion.identity, rayToFloor.transform);
                target = newCrytsal.transform;
            }
            else
                isCreatingCrystal = false;
        }
        if(Vector2.Distance(transform.position, target.position) > 3f)
            currentSpeed = speedWhenFar;
    }

    private void StopCrystal()
    {
        if(isCreatingCrystal)
        {
            isCreatingCrystal = false;
            target.GetComponent<HealCrystal>().CreateCrystal();
            manaBar.UseMana(25);
            target = playerTarget;
            if (Vector2.Distance(transform.position, target.position) > 5f)
                currentSpeed = speedWhenFar;
            else
                currentSpeed = speedWhenClose;
        }
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
        AudioManager.instance.PlaySound(AudioManager.SoundList.SmokeBomb);
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
