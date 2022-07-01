using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeStick : MonoBehaviour
{
    private MovementPlatformer player;
    private bool isActive;
    private IEnumerator coroutine, coroutineRelease;
    [SerializeField]
    private float stamina;
    private float currentStamina;

    void Start()
    {
        player = GameMaster.instance.playerInstance.GetComponent<MovementPlatformer>();
    }

  
    public void Stick()
    {
        if (IsAttackingUp())
        {
            isActive = true;
            coroutine = player.PauseMovement(stamina);
            coroutineRelease = Release(stamina);
            StartCoroutine(coroutine);
            StartCoroutine(coroutineRelease);
            player.transform.position = new Vector3(transform.position.x, transform.position.y - 2f, player.transform.position.z);
            HandleCollider();
        }
    }

    private void Update()
    {
        if (isActive && InputManager.instance.KeyDown(Keybindings.KeyList.Jump))
        {
            isActive = false;
            StopCoroutine(coroutine);
            StopCoroutine(coroutineRelease);
            player.rb.constraints = ~RigidbodyConstraints2D.FreezePosition;
            HandleCollider();
        }

        if(isActive)
        {
            player.groundedMemory = 0.05f;
        }

        if (currentStamina >= 0)
        {
            currentStamina -= Time.deltaTime;

        }
    }

    private IEnumerator Release(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        isActive = false;
        HandleCollider();
    }

    void HandleCollider()
    {
        GetComponent<BoxCollider2D>().enabled = !isActive;
    }

    private bool IsAttackingUp()
    {
        float x = player.GetDirectionPressedVector().x;
        return player.GetDirectionPressedVector().Equals(new Vector2(x, 1));
    }
}
