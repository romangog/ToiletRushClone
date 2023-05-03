using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinishScreen : MonoBehaviour
{
    public event Action NextButtonPressedEvent;

    [SerializeField] private SimpleTouchGraphic _nextButton;

    private void Start()
    {
        _nextButton.PointerDownEvent += OnNextButtonPressed;
    }

    private void OnNextButtonPressed()
    {
        NextButtonPressedEvent?.Invoke();
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
