using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory_Base : MonoBehaviour
{
    [SerializeField]
    private Vector2 slotsArraySize;
    [SerializeField]
    private GameObject slotPrefab, border;

    [SerializeField]
    private Vector2 borderOffset;

    void Start()
    {
        int slotsAmount = (int)(slotsArraySize.x * slotsArraySize.y);
        for (int i = 0; i < slotsAmount; i++)
        {
            Instantiate(slotPrefab, transform);
        }

        HandleBorderDimensions();
    }

    void HandleBorderDimensions()
    {
        RectTransform borderRect = border.GetComponent<RectTransform>();
        RectTransform slotsRect = GetComponent<RectTransform>();
        float borderScale = borderRect.localScale.x;
        float slotsScale = slotsRect.localScale.x;
        float difference = slotsScale / borderScale;

        float cellSize = GetComponent<GridLayoutGroup>().cellSize.x;
        Vector2 slotsSize = new Vector2(cellSize * slotsArraySize.x, cellSize * slotsArraySize.y);
        slotsRect.sizeDelta = slotsSize;

        borderRect.sizeDelta = new Vector2((slotsSize.x * difference) + borderOffset.x, (slotsSize.y * difference) + borderOffset.y);
    }
}
