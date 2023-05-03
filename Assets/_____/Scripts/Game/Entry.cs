using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Entry : MonoBehaviour
{
    private SceneLoaderWrapper _sceneLoader;
    private SLS.Snapshot _snapshot;
    [Inject]
    private void Construct(SceneLoaderWrapper loader,
        SLS.Snapshot snapshot)
    {
        _sceneLoader = loader;
        _snapshot = snapshot;
    }

    private void Start()
    {
        _sceneLoader.LoadLevel(_snapshot.CurrentLevel%3 +1);
    }
}
