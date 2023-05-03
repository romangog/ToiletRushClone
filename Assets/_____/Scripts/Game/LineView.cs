using System;
using UnityEngine;

public class LineView : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;

    public void SetFirstPoint(Vector3 point)
    {
        _lineRenderer.positionCount = 1;
        _lineRenderer.SetPosition(0, point);
    }

    internal void UpdateLastPoint(Vector3 point)
    {
        _lineRenderer.SetPosition(_lineRenderer.positionCount - 1, point);
    }

    internal void AddPoint()
    {
        _lineRenderer.positionCount++;
    }

    internal void DropLine()
    {
        _lineRenderer.positionCount = 0;
    }
}

