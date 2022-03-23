using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using MyBox;
//using UnityEngine.Experimental.Rendering.Universal;


public class JumpStone : MonoBehaviour, IRespawnResetable
{
    MovementPlatformer player;
    private bool isActive;
    public Animator animator;
    public Transform pointToFollow;
    public float currentSpeed;
    public float speedClose, speedFar, radiusClose, speedOffset;
    private Vector2 originalPos;
    private Tilemap tilemap;
    private bool inTiles;
    public Color deactivatedColor;
    private SpriteRenderer sprt;

    public bool isOnFearOfHeights = false;
    [ConditionalField("isOnFearOfHeights")] public bool isOnFar;
    [ConditionalField("isOnFearOfHeights")] public LayerSwitcher layerSwitcher;

    void Start()
    {
        player = GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>();
        sprt = GetComponent<SpriteRenderer>();
        originalPos = transform.position;
        tilemap = CellOrganizer.instance.tileMap;
        if (isOnFearOfHeights && !isOnFar)
            originalPos = transform.localPosition;
    }

    private void Update()
    {
        if (isActive)
        {
            if (Vector2.Distance(transform.position, pointToFollow.position) > radiusClose)
                currentSpeed = speedFar;
            else
                currentSpeed = speedClose;

            if (inTiles)
            {
                if (CanDeactivate())
                {
                    isActive = false;
                    inTiles = false;
                }
            }

            transform.position = Vector2.MoveTowards(transform.position, pointToFollow.position, currentSpeed * Time.deltaTime);
        }
    }
    /*
    void FixedUpdate()
    {
        if(InputManager.instance.KeyDown(Keybindings.KeyList.Jump) && isActive && player.isAirborn)
        {
            CellOrganizer.instance.ReleaseLatest();
            DeactivateCell();
            player.rb.velocity = new Vector2(player.rb.velocity.x, 1 * player.jumpV);
            isActive = false;
        }
    }
    */
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == 11) // 11 is player
        {
            // isActive = true;
            ActivateCell();
            SetFloats();
        }
    }

    private bool CanDeactivate()
    {
        if (!tilemap.HasTile(tilemap.WorldToCell(transform.position)))
            return true;
        else
            return false;
    }

    void SetFloats()
    {
        float rnd = Random.Range(-speedOffset, speedOffset);
        speedClose += rnd;
        speedFar += rnd;
    }

    private void ActivateCell()
    {
        CellOrganizer org = CellOrganizer.instance;
        if (org.CanAddCell())
        {
            org.AddCell(gameObject);
            pointToFollow = org.GetCurrentPoint();
            isActive = true;
            GetComponent<CircleCollider2D>().enabled = false;
            // animator.SetBool("IsActive", true);
        }
    }

    public void DeactivateCell()
    {
        StartCoroutine(DelayDeactivation());
    }

    private IEnumerator DelayDeactivation()
    {

        if (CanDeactivate())
            isActive = false;
        else
            inTiles = true;

        //  animator.SetBool("IsActive", false);

        sprt.color = deactivatedColor;

        yield return new WaitForSeconds(1f);

        sprt.color = Color.white;
        GetComponent<CircleCollider2D>().enabled = true;
    }

    public void PlayerHasRespawned()
    {
        //  AudioManager.instance.PlaySound(AudioManager.SoundList.OrbDesrtoy);
        if (isOnFearOfHeights)
        {
            Debug.Log("isonfear");
            layerSwitcher.ResetJumpStones(gameObject, isOnFar);
            if(!isOnFar)
                transform.localPosition = originalPos;
            else
                transform.position = originalPos;
        }
        else
            transform.position = originalPos;

        isActive = false;
        GetComponent<CircleCollider2D>().enabled = true;
    }

    public void Pop()
    {
        //AudioManager.instance.PlaySound(AudioManager.SoundList.OrbDesrtoy);
        transform.position = originalPos;
        isActive = false;
        GetComponent<CircleCollider2D>().enabled = true;
    }

    private void OnDisable()
    {
        if (gameObject.activeSelf)
            Pop();
    }
}
