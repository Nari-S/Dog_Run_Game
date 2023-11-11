using System;
using UniRx;

interface IObstacleReceivable
{
    IObservable<Unit> OnGameOverd { get; }
    void NotifyGameOverEvent();
}

