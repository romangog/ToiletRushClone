using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SimpleTouchInput : ITickable
{
    public Action StartedHoldingEvent;
    public Action EndedHoldingEvent;

    public bool IsHolding => _IsHolding;
    public Vector3 Distance => _distance;
    public Vector3 TickDelta => _delta;

    private bool _IsHolding;
    private Vector3 _touchDownPosition;
    private Vector3 _delta;
    private Vector3 _distance;
    private Vector3 _lastTickPos;
    private bool _IsReading;

    private SimpleTouchGraphic _graphic;

    public void StartReading()
    {
        _IsReading = true;
    }

    public void StopReading()
    {
        _IsReading = false;
        _IsHolding = false;
        _delta = Vector3.zero;
        _distance = Vector3.zero;
    }

    public void SetTouchGraphic(SimpleTouchGraphic graphic)
    {
        if (_graphic != graphic)
        {
            if (_graphic != null)
            {
                _graphic.PointerDownEvent -= OnTouchDown;
                _graphic.PointerUpEvent -= OnTouchUp;
            }
            graphic.PointerUpEvent += OnTouchUp;
            graphic.PointerDownEvent += OnTouchDown;
        }

        _graphic = graphic;
    }

    public void Tick()
    {
        if (!_IsReading) return;

        if (_graphic == null)
        {
            Debug.Log("UpdateTouchGraphic");
            UpdateTouchInput();
        }


        if (_IsHolding)
            OnTouchHold();
    }

    private void UpdateTouchInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnTouchDown();
        }

        if (Input.GetMouseButtonUp(0))
        {
            OnTouchUp();
        }
    }

    private void OnTouchDown()
    {
        _IsHolding = true;
        _touchDownPosition = GetTouchPhysicalPosition(Input.mousePosition);
        _lastTickPos = _touchDownPosition;
        StartedHoldingEvent?.Invoke();
    }

    private void OnTouchUp()
    {
        _IsHolding = false;
        EndedHoldingEvent?.Invoke();
        _delta = Vector3.zero;
        _distance = Vector3.zero;
    }

    private void OnTouchHold()
    {
        // delta move touch
        Vector3 currentPos = GetTouchPhysicalPosition(Input.mousePosition);
        _distance = currentPos - _touchDownPosition;
        _delta = currentPos - _lastTickPos;
        _lastTickPos = currentPos;

        // screen border
        Vector2 borderTurn = Vector2.zero;
        //Vector2 screenSize = GetScreenPhysicalSize();
        if (currentPos.x > 0.95f)
            borderTurn += Vector2.right;
        if (currentPos.x < 0.05f)
            borderTurn += Vector2.left;

        borderTurn *= 0.01f;
        _delta += (Vector3)borderTurn;
    }

    private Vector2 GetTouchPhysicalPosition(Vector3 mousePos)
    {
        float ratio = Screen.width / (float)Screen.height;
        float physicalDistX = mousePos.x / Screen.width;
        float physicalDistY = mousePos.y / Screen.height;
        physicalDistY /= ratio;
        Vector2 result = Vector2.zero;
        //#if UNITY_EDITOR
        result = new Vector2(physicalDistX, physicalDistY);
        //#else
        //        result = new Vector2(physicalDistX, physicalDistY) / Screen.dpi;
        //#endif
        return result;
    }

    private Vector2 GetScreenPhysicalSize()
    {
        Vector2 size = new Vector2();
        size.x = 1f;
        size.y = (float)Screen.height / Screen.width;
        Debug.Log("Screen Phys size = " + size);
        return size;
    }
}
