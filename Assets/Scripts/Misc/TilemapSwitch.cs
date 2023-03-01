using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapSwitch : MonoBehaviour
{
    [SerializeField] private GameObject newTilemap;
    [SerializeField] private TilemapManager _TilemapManager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 11) // 11 is player
        {
            newTilemap.SetActive(true);
            GetComponentInParent<TilemapManager>().currentTilemap = newTilemap.GetComponent<Tilemap>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 11) // 11 is player
        {
            newTilemap.SetActive(false);
        }
    }
}
