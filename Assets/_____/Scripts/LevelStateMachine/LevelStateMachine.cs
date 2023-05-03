using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.Linq;
using TMPro;

public enum LevelStateType
{
    Start,
    Draw,
    Move,
    Finish
}

public class LevelStateMachine : ITickable, IInitializable, IFixedTickable
{
    private ILevelState _currentState;

    private ILevelState[] _states;

    public class DataPack
    {
        public DataPack(CharacterController[] characterControllers, SimpleTouchInput simpleTouchInput, GameSettings gameSettings, UI ui, SLS.Snapshot snapshot, SceneLoaderWrapper sceneLoader)
        {
            CharacterControllers = characterControllers;
            SimpleTouchInput = simpleTouchInput;
            GameSettings = gameSettings;
            Ui = ui;
            Snapshot = snapshot;
            SceneLoader = sceneLoader;
        }

        public CharacterController[] CharacterControllers { get; }
        public SimpleTouchInput SimpleTouchInput { get; }
        public GameSettings GameSettings { get; }
        public UI Ui { get; }
        public SLS.Snapshot Snapshot { get; }
        public SceneLoaderWrapper SceneLoader { get; }
    }

    public LevelStateMachine(
        EventBus eventBus,
        CamerasController camerasController,
        GameSettings gameSettings,
        SimpleTouchInput simpleTouchInput,
        BlackScreen blackScreen,
        SLS.Snapshot snapshot,
        Prefabs prefabs,
        SceneLoaderWrapper sceneLoader,
        Money money,
        CharacterView[] characters,
        UI ui
        )
    {
        CharacterController[] characterControllers = new CharacterController[characters.Length];

        for (int i = 0; i < characterControllers.Length; i++)
        {
            characterControllers[i] = new CharacterController(characters[i]);
        }

        DataPack dataPack = new DataPack(
            characterControllers,
            simpleTouchInput,
            gameSettings,
            ui,
            snapshot,
            sceneLoader);

        StartLevelState startLevelState = new StartLevelState(dataPack);
        DrawLevelState drawLevelState = new DrawLevelState(dataPack);
        MoveLevelState moveLevelState = new MoveLevelState(dataPack);
        FinishLevelState finishLevelState = new FinishLevelState(dataPack);
        _states = new ILevelState[]
        {
            startLevelState,
            drawLevelState,
            moveLevelState,
            finishLevelState
        };

        foreach (var state in _states)
        {
            state.CalledForStateChangeEvent += ChangeState;
        }
    }

    public void ChangeState(LevelStateType stateType)
    {
        if (_currentState != null && _currentState.Type != stateType)
        {
            _currentState.Stop();
        }
        _currentState = _states[(int)stateType];

        _currentState.Start();
    }

    public void FixedTick()
    {
        _currentState?.FixedUpdate();
    }

    public void Initialize()
    {
        ChangeState(LevelStateType.Start);
    }

    public void Tick()
    {
        _currentState?.Update();
    }
}

public interface ILevelState
{
    event Action<LevelStateType> CalledForStateChangeEvent;
    LevelStateType Type { get; }
    void Start();
    void Update();
    void FixedUpdate();
    void Stop();
}

public class StartLevelState : ILevelState
{
    private TMP_Text _levelNumText;
    private SLS.Snapshot _snapshot;

    public LevelStateType Type => LevelStateType.Start;

    public event Action<LevelStateType> CalledForStateChangeEvent;

    public StartLevelState(LevelStateMachine.DataPack dataPack)
    {
        _levelNumText =  dataPack.Ui.LevelNumText;
        _snapshot =  dataPack.Snapshot;
    }

    public void FixedUpdate()
    {
        
    }

    public void Start()
    {
        _levelNumText.text = "LEVEL " + (_snapshot.CurrentLevel + 1);
        CalledForStateChangeEvent(LevelStateType.Draw);
    }

    public void Stop()
    {
        
    }

    public void Update()
    {
        
    }
}

public class DrawLevelState : ILevelState
{
    public event Action<LevelStateType> CalledForStateChangeEvent;
    public LevelStateType Type => LevelStateType.Draw;

    private readonly SimpleTouchInput _simpleTouchInput;
    private readonly CharacterController[] _characterControllers;
    private readonly GameSettings _gameSettings;
    private readonly DrawingScreen _drawScreen;
    private List<Vector3> _pathPoints = new List<Vector3>();
    private Plane _gameFieldPlane;
    private int _chosenCharacterIndex;
    private bool _isDrawing;
    private bool _AllPathsDrawn;

    public DrawLevelState(LevelStateMachine.DataPack dataPack)
    {
        _simpleTouchInput = dataPack.SimpleTouchInput;
        _characterControllers = dataPack.CharacterControllers;
        _gameSettings = dataPack.GameSettings;
        _drawScreen = dataPack.Ui.DrawingScreen;

        _gameFieldPlane = new Plane(Vector3.forward, 0f);
        _simpleTouchInput.SetTouchGraphic(dataPack.Ui.DrawGraphic);
    }

    public void Start()
    {
        _drawScreen.Show();
        _drawScreen.HideStartButton();
        _drawScreen.StartButtonPressedEvent += OnStartButtonPressed;

        _simpleTouchInput.StartedHoldingEvent += OnStartedHolding;
        _simpleTouchInput.EndedHoldingEvent += OnEndedHolding;
        _simpleTouchInput.StartReading();
    }

    private void OnStartButtonPressed()
    {
        CalledForStateChangeEvent(LevelStateType.Move);
    }

    private void OnStartedHolding()
    {
        if (StartedOnCharacter(out int characterIndex))
        {
            _chosenCharacterIndex = characterIndex;
            if(_characterControllers[_chosenCharacterIndex].HasDrawnPath)
            {
                DropLine();
            }
            StartDrawingLine();
        }
        else
        {
            DropLine();
        }
    }

    private void StartDrawingLine()
    {
        _isDrawing = true;
        Vector3 startPoint = ProjectScreenToPlane(Input.mousePosition);
        _characterControllers[_chosenCharacterIndex].SetStartPoint(startPoint);
        AddPathPoint(startPoint);
        _characterControllers[_chosenCharacterIndex].UpdateLastPoint(startPoint);
    }

    private void DropLine()
    {
        _isDrawing = false;
        _AllPathsDrawn = false;
        _drawScreen.HideStartButton();

        _pathPoints.Clear();
        _characterControllers[_chosenCharacterIndex].DropLine();
    }

    private bool StartedOnCharacter(out int characterIndex)
    {
        Vector3 point = ProjectScreenToPlane(Input.mousePosition);
        for (int i = 0; i < _characterControllers.Length; i++)
        {
            float sqrMagnitude = Vector3.SqrMagnitude(point - _characterControllers[i].StartPosition);
            if (sqrMagnitude < _gameSettings.DetectCharacterTouchSqrDistance)
            {
                characterIndex = i;
                return true;
            }
        }
        characterIndex = -1;
        return false;
    }

    private bool EndedOnDestinationPoint(out int destPointIndex)
    {
        Vector3 point = ProjectScreenToPlane(Input.mousePosition);
        for (int i = 0; i < _characterControllers.Length; i++)
        {
            if (Vector3.SqrMagnitude(point - _characterControllers[i].DestinationPosition) < _gameSettings.DetectCharacterTouchSqrDistance)
            {
                destPointIndex = i;
                return true;
            }
        }
        destPointIndex = -1;
        return false;
    }

    private void OnEndedHolding()
    {
        if (EndedOnDestinationPoint(out int destPointIndex)
            && destPointIndex == _chosenCharacterIndex)
        {
            DrawLineSuccessful();
        }
        else
        {
            DropLine();
        }
    }

    private void DrawLineSuccessful()
    {
        _isDrawing = false;
        _characterControllers[_chosenCharacterIndex].PassSuccessfulPath(_pathPoints);
        _pathPoints = new List<Vector3>();

        if(_characterControllers.All((x) => x.HasDrawnPath))
        {
            _AllPathsDrawn = true;
            _drawScreen.ShowStartButton();
        }
    }

    private Vector3 ProjectScreenToPlane(Vector3 pos)
    {
        Ray ray = Camera.main.ScreenPointToRay(pos);

        _gameFieldPlane.Raycast(ray, out float distance);

        return ray.GetPoint(distance);
    }

    public void Stop()
    {
        _drawScreen.Hide();
        _drawScreen.StartButtonPressedEvent -= OnStartButtonPressed;
        _simpleTouchInput.StartedHoldingEvent -= OnStartedHolding;
        _simpleTouchInput.EndedHoldingEvent -= OnEndedHolding;
        _simpleTouchInput.StopReading();
    }

    public void Update()
    {
        if (_isDrawing)
        {
            Vector3 newPoint = ProjectScreenToPlane(Input.mousePosition);
            if (Vector3.SqrMagnitude(_pathPoints[_pathPoints.Count - 1] - newPoint) > _gameSettings.NextPointSetSqrDistance)
            {
                AddPathPoint(newPoint);
            }
            _characterControllers[_chosenCharacterIndex].UpdateLastPoint(newPoint);
        }
    }

    private void AddPathPoint(Vector3 point)
    {
        _characterControllers[_chosenCharacterIndex].AddPathPoint();
        _pathPoints.Add(point);
    }

    public void FixedUpdate()
    {

    }
}
public class MoveLevelState : ILevelState
{
    private readonly CharacterController[] _characterControllers;
    private readonly GameSettings _gameSettings;

    public LevelStateType Type => LevelStateType.Move;

    public event Action<LevelStateType> CalledForStateChangeEvent;


    public MoveLevelState(LevelStateMachine.DataPack dataPack)
    {
        _characterControllers = dataPack.CharacterControllers;
        _gameSettings = dataPack.GameSettings;
    }

    public void FixedUpdate()
    {
        
    }

    public void Start()
    {
        foreach (var characterController in _characterControllers)
        {
            characterController.CharacterHitEvent += OnCharacterHit;
            characterController.CharacterReachedEvent += OnCharacterReachedEnd;
            characterController.StartMoving(_gameSettings.MoveTime);
        }
    }

    private void OnCharacterReachedEnd()
    {
        if(_characterControllers.All((x) => x.HasReachedEnd))
        {
            CalledForStateChangeEvent(LevelStateType.Finish);
        }
    }

    private void OnCharacterHit()
    {
        if (_characterControllers.Any((x) => x.HasHit)) return;

        DOTween.Sequence()
            .PrependInterval(1f)
            .OnComplete(ResetLevel);
    }

    private void ResetLevel()
    {
        foreach (var character in _characterControllers)
        {
            character.Reset();
        }
        CalledForStateChangeEvent(LevelStateType.Draw);
    }

    public void Stop()
    {
        foreach (var characterController in _characterControllers)
        {
            characterController.CharacterHitEvent -= OnCharacterHit;
            characterController.CharacterReachedEvent -= OnCharacterReachedEnd;
        }
    }

    public void Update()
    {
        
    }
}


public class FinishLevelState : ILevelState
{
    private FinishScreen _finishScreen;
    private SLS.Snapshot _snapshot;
    private SceneLoaderWrapper _loader;

    public LevelStateType Type => LevelStateType.Finish;

    public event Action<LevelStateType> CalledForStateChangeEvent;

    public FinishLevelState(LevelStateMachine.DataPack dataPack)
    {
        _finishScreen = dataPack.Ui.FinishScreen;
        _snapshot = dataPack.Snapshot;
        _loader = dataPack.SceneLoader;

        _finishScreen.NextButtonPressedEvent += OnNextButtonPressed;
    }

    private void OnNextButtonPressed()
    {
        _snapshot.CurrentLevel++;
        _loader.LoadLevel(_snapshot.CurrentLevel % 3 + 1);
    }

    public void FixedUpdate()
    {
    }

    public void Start()
    {
        _finishScreen.Show();
    }

    public void Stop()
    {
        _finishScreen.Hide();
    }

    public void Update()
    {
    }
}
