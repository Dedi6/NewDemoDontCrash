using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LayerSwitcher : MonoBehaviour
{
    [SerializeField]
    private TilemapCollider2D layerFar, layerClose;

    private bool switchToFar;

    [SerializeField]
    private GameObject player, brother;

    [SerializeField]
    private Color colorClose, colorFar;

    [SerializeField]
    private float height, speedFar, speedClose, jumpFar, jumpClose;

    [SerializeField]
    private Transform[] roomsFar, roomsClose;



    public void SwitchLayer()
    {
        StartCoroutine(StartSwitching());
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.N))
            StartCoroutine(StartSwitching());
    }

    private IEnumerator StartSwitching()
    {
        StartCoroutine(player.GetComponent<MovementPlatformer>().SwitchStateIgnore(0.2f));
        player.GetComponent<Animator>().SetTrigger("Heal");
        yield return new WaitForSeconds(0.2f);

        
        if (switchToFar)
        {
            SwitchData(true, 8f, speedFar, colorFar);

        }
        else
        {
            SwitchData(false, 12f, speedClose, colorClose);
        }

        switchToFar = !switchToFar;
    }

    private void SwitchData(bool switchToFar, float newSize, float newSpeed, Color newColour)
    {
        layerFar.enabled = switchToFar;
        layerClose.enabled = !switchToFar;
        SwitchCollidersOfChildren(switchToFar);
        Transform playerTransform = player.GetComponent<Transform>();
        MovementPlatformer playerScript = player.GetComponent<MovementPlatformer>();
        playerTransform.localScale = new Vector3(newSize, newSize, 0f);
        playerScript.speedMulitiplier = newSpeed;
        player.GetComponent<SpriteRenderer>().color = newColour;
        playerTransform.position = new Vector3(playerTransform.position.x, playerTransform.position.y + height, playerTransform.position.z);
        playerScript.jumpMemory = 0f; // this fixes jumping after teleporting 

        // change fall speed
        playerScript.fallSpeed = switchToFar ? 28f : 34f;
        playerScript.jumpV = switchToFar ? jumpFar : jumpClose;

        GetComponent<BlurControl>().SwitchBlur(!switchToFar);
        HandleJumpOrbs();
        StartCoroutine(SwitchBirdValues(switchToFar, newColour));
    }

    private IEnumerator SwitchBirdValues(bool switchToFar, Color newColor)
    {
        float newSize = switchToFar ? 0.8f : 1.2f;
        Transform brotherTr = brother.GetComponent<Transform>();
        SpriteRenderer brotherSprite = brother.GetComponent<SpriteRenderer>();
        yield return new WaitForSeconds(0.2f);

        brotherTr.localScale = new Vector3(1f, 1f, 0f);
        brotherSprite.color = Color.white;

        yield return new WaitForSeconds(0.2f);

        brotherTr.localScale = new Vector3(newSize, newSize, 0f);
        brotherSprite.color = newColor;
    }

    private void SwitchCollidersOfChildren(bool switchToFar)
    {
        foreach (Transform room in roomsFar)
        {
            foreach (Transform child in room)
                TrySwitchingColliders(child, switchToFar);
        }
        
        foreach (Transform room in roomsClose)
        {
            foreach (Transform child in room)
                TrySwitchingColliders(child, !switchToFar);
        }
    }

    void TrySwitchingColliders(Transform child, bool shouldEnable)
    {
        if (child.TryGetComponent(out BoxCollider2D col))
            col.enabled = shouldEnable;
        if (child.TryGetComponent(out CircleCollider2D colCircle))
            colCircle.enabled = shouldEnable;

        if(child.childCount > 0)
        {
            foreach (Transform grandChild in child)
            {
                TrySwitchingColliders(grandChild, shouldEnable);
            }
        }
    }

    private void HandleJumpOrbs()
    {
        CellOrganizer cellsO = CellOrganizer.instance;
        StartCoroutine(DisableJumpStones());

        if (cellsO.HaveOrbs())
        {
            Transform newParent = switchToFar ? roomsFar[0] : roomsClose[0];
            for (int i = 0; i <= cellsO.currentPos; i++)
            {
                GameObject currentCell = cellsO.cells[i].currentCell;
                currentCell.transform.SetParent(newParent);
                GetComponent<BlurControl>().ChangeMatOfObject(currentCell, switchToFar);
                currentCell.transform.localScale = new Vector3(1f, 1f, 0f);
            }
        }
    }

    private IEnumerator DisableJumpStones()
    {
        yield return new WaitForSeconds(0.2f);

        ShutOrbsCollider();

        yield return new WaitForSeconds(0.3f);

        ShutOrbsCollider();

    }

    private void ShutOrbsCollider()
    {
        Transform otherLayer = switchToFar ? layerFar.transform : layerClose.transform;
        foreach (Transform child in otherLayer)
        {
            if (child.TryGetComponent(out CircleCollider2D col))
                col.enabled = false;
        }
    }
}