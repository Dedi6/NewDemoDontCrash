using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowTrap : MonoBehaviour
{
    public Transform spawnPoint;
    public float spawnArrowPer, arrowSpeed;
    public GameObject arrowPrefab;
    public Side side;
    private bool isActive = true, emitSound;
    Vector2 dir;
    private Coroutine shoot_Coroutine;

    public enum Side
    {
        FaceUp,
        FaceLeft,
        FaceRight,
        FaceDown,
    }

    void Start()
    {
        Rotate();
        shoot_Coroutine = StartCoroutine(ShootArrow());
    }



    private IEnumerator ShootArrow()
    {
        if (emitSound)
            AudioManager.instance.PlaySound(AudioManager.SoundList.ArrowShoot);
        GameObject arrow = Instantiate(arrowPrefab, spawnPoint.position, Quaternion.identity, transform);
        arrow.GetComponent<Action_TriggerHitPlayer>().SetMovement(dir, arrowSpeed);

        yield return new WaitForSeconds(spawnArrowPer);

        shoot_Coroutine = StartCoroutine(ShootArrow());
    }

    private void OnEnable()
    {
        if (!isActive)
        {
            isActive = true;
            shoot_Coroutine = StartCoroutine(ShootArrow());
        }
    }

    private void OnDisable()
    {
        StopCoroutine(shoot_Coroutine);
        isActive = false;
    }

    void Rotate()
    {
        switch (side)
        {
            case Side.FaceDown:
                dir = Vector2.down;
                transform.Rotate(0.0f, 0.0f, 270.0f);
                RotateCollider();
                break;
            case Side.FaceLeft:
                dir = Vector2.left;
                transform.Rotate(0.0f, 0.0f, 180.0f);
                break;
            case Side.FaceRight:
                dir = Vector2.right;
                break;
            case Side.FaceUp:
                dir = Vector2.up;
                transform.Rotate(0.0f, 0.0f, 90.0f);
                RotateCollider();
                break;
        }
    }

    void RotateCollider()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();

        float x = col.size.x;
        float y = col.size.y;
        col.size = new Vector2(y, x);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            emitSound = true;
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            emitSound = false;
        }
    }
}
