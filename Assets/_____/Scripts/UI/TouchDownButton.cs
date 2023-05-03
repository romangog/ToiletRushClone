using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class TouchDownButton : MonoBehaviour, IPointerDownHandler,IPointerUpHandler
{
    internal UnityEvent TouchDownEvent { get; private set; } = new UnityEvent();
    internal UnityEvent TouchUpEvent { get; private set; } = new UnityEvent();

    public void OnPointerDown(PointerEventData eventData)
    {
        TouchDownEvent.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        TouchUpEvent.Invoke();
    }
}
