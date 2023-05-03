using DG.Tweening;
using Dreamteck.Splines;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController
{
    public event Action CharacterHitEvent;
    public event Action CharacterReachedEvent;

    public Vector3 StartPosition => _startPosition;
    public Vector3 DestinationPosition => _destinationPosition;
    public bool HasDrawnPath => _HasDrawnPath;
    public bool HasReachedEnd => _HasReachedEnd;
    public bool HasHit => _HasHit;

    private Vector3 _startPosition;
    private Vector3 _destinationPosition;

    private readonly CharacterView _characterView;
    private readonly DestinationPointView _destPointView;

    private bool _HasDrawnPath;
    private bool _HasReachedEnd;
    private bool _HasHit;

    private List<Vector3> _pathPoints;

    public CharacterController(
        CharacterView characterView )
    {
        _characterView = characterView;
        _destPointView = characterView.DestinationPointView;

        _destinationPosition = _destPointView.transform.position;
        _startPosition = _characterView.transform.position;
        _characterView.CharacterHitbox.CharacterHitEvent += OnCharacterHitEvent;
        _characterView.MovingCharacterFollower.onEndReached += OnCharacterReachedEnd;
    }

    private void OnCharacterReachedEnd(double obj)
    {
        _HasReachedEnd = true;
        CharacterReachedEvent?.Invoke();
    }

    private void OnCharacterHitEvent(Collider2D collider)
    {
        _characterView.MovingCharacterFollower.follow = false;
        CharacterHitEvent?.Invoke();
        _HasHit = true;
    }

    internal void PassSuccessfulPath(List<Vector3> pathPoints)
    {
        _pathPoints = pathPoints;
        SplinePoint[] splinePoints = new SplinePoint[pathPoints.Count];
        for (int i = 0; i < pathPoints.Count; i++)
        {
            splinePoints[i] = new SplinePoint(pathPoints[i]);
        }
        _characterView.SplineComputer.SetPoints(splinePoints);
        //_characterView.MovingCharacterFollower.spline = _characterView.SplineComputer;
        _HasDrawnPath = true;
    }

    internal void SetStartPoint(Vector3 point)
    {
        _characterView.PathLine.SetFirstPoint(point);
    }

    internal void UpdateLastPoint(Vector3 point)
    {
        _characterView.PathLine.UpdateLastPoint(point);
    }

    internal void AddPathPoint()
    {
        _characterView.PathLine.AddPoint();
    }

    internal void DropLine()
    {
        _pathPoints = null;
        _HasDrawnPath = false;
        _characterView.PathLine.DropLine();
    }

    public void StartMoving(float time)
    {
        _characterView.MovingCharacterFollower.followDuration = time;
        _characterView.MovingCharacterFollower.follow = true;
    }

    internal void Reset()
    {
        _HasDrawnPath = false;
        _HasReachedEnd = false;
        _HasHit = false;
        _pathPoints = null;
        DropLine();
        _characterView.MovingCharacterFollower.follow = false;
        _characterView.MovingCharacterFollower.Restart();
        _characterView.MovingCharacterFollower.transform.localPosition = Vector3.zero;
    }
}