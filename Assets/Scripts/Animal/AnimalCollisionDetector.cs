using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class AnimalCollisionDetector : MonoBehaviour, IDeletable
{
    private Subject<Unit> _OnDeleted = new Subject<Unit>();
    public IObservable<Unit> OnDeleted => _OnDeleted;
    private IDisposable notifyGameOverSubscriber;

    /// <summary>
    /// 削除イベントを通知
    /// </summary>
    public void DeleteObject()
    {
        _OnDeleted.OnNext(Unit.Default);
    }


    private void OnEnable()
    {
        /* 当たった相手が柴犬だった場合，ゲームオーバ通知と鳴き声の再生を行う． */
        notifyGameOverSubscriber = this.OnTriggerEnterAsObservable().Select(x => (obstacleReceivable: x.GetComponent<IObstacleReceivable>(), dogAudioController: x.GetComponent<DogAudioController>()))
            .Where(x => x.obstacleReceivable != null && x.dogAudioController != null).Subscribe(x =>
            {
                x.dogAudioController.PlayAudio(DogAudioController.AudioKinds.whine);
                x.obstacleReceivable.NotifyGameOverEvent();
            });
    }

    private void OnDisable()
    {
        notifyGameOverSubscriber.Dispose();
    }
}
