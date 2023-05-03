using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    public DrawingScreen DrawingScreen => _drawingScreen;
    public SimpleTouchGraphic DrawGraphic => _drawGraphic;
    public FinishScreen FinishScreen => _finishScreen;
    public TMP_Text LevelNumText => _levelNumText;
    
    [SerializeField] private TMP_Text _levelNumText;
    [SerializeField] private DrawingScreen _drawingScreen;
    [SerializeField] private FinishScreen _finishScreen;
    [SerializeField] private SimpleTouchGraphic _drawGraphic;
}
