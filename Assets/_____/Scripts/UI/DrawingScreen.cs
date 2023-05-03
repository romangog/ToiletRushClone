using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingScreen : MonoBehaviour
{
    public event Action StartButtonPressedEvent;

    [SerializeField] private SimpleTouchGraphic _startButton;

    private void Start()
    {
        _startButton.PointerDownEvent += OnStartButtonPressedEvent;
        _startButton.gameObject.SetActive(false);
    }

    private void OnStartButtonPressedEvent()
    {
        StartButtonPressedEvent?.Invoke();
    }

    public void ShowStartButton()
    {
        _startButton.gameObject.SetActive(true);
    }

    public void HideStartButton()
    {
        _startButton.gameObject.SetActive(false);
    }

    internal void Show()
    {
        gameObject.SetActive(true);
    }

    internal void Hide()
    {
        gameObject.SetActive(false);
    }
}
