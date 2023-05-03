using Dreamteck.Splines;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterView : MonoBehaviour
{
    public LineView PathLine => _pathLine;
    public DestinationPointView DestinationPointView => _destinationPointView;
    public SplineComputer SplineComputer => _splineComputer;
    public SplineFollower MovingCharacterFollower => _movingCharacterFollower;
    public CharacterHitbox CharacterHitbox => _characterHitbox;

    [SerializeField] private LineView _pathLine;
    [SerializeField] private DestinationPointView _destinationPointView;
    [SerializeField] private SplineComputer _splineComputer;
    [SerializeField] private SplineFollower _movingCharacterFollower;
    [SerializeField] private CharacterHitbox _characterHitbox;
}

