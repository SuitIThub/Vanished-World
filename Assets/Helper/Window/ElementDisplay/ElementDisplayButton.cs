using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ElementDisplayButton : MonoBehaviour, IPointerClickHandler
{
    public delegate void clickMethod();
    public clickMethod method;
    public void OnPointerClick(PointerEventData eventData)
    {
        method?.Invoke();
    }
}
