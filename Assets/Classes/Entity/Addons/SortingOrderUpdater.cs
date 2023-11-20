using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(SpriteRenderer))]
public class SortingOrderUpdater : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public int sortingOffset = 0;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        spriteRenderer.sortingOrder = 32767 - (int)(transform.position.y * 10) - sortingOffset;
    }
}
