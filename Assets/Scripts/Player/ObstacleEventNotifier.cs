using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class ObstacleEventNotifier : MonoBehaviour, IObstacleReceivable
{
    private Subject<Unit> _OnGameOverd;
    public IObservable<Unit> OnGameOverd => _OnGameOverd;

    private void Awake()
    {
        _OnGameOverd = new Subject<Unit>();
    }

    public void NotifyGameOverEvent()
    {
        _OnGameOverd.OnNext(Unit.Default);
    }
}
