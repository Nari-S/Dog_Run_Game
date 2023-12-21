using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Obstacle : MonoBehaviour
{
    private IObstacleReceivable obstacleReceivable;
    private DogAudioController dogAudioController;

    private void Awake()
    {
        /* プレイヤーとの当たり判定時の処理
         * ゲームオーバーとする　*/
        this.OnTriggerEnterAsObservable()
            .Where(x => x.TryGetComponent(out obstacleReceivable) && x.gameObject.TryGetComponent(out dogAudioController))
            .Subscribe(_ =>
            {
                dogAudioController.PlayAudio(DogAudioController.AudioKinds.whine);
                obstacleReceivable.NotifyGameOverEvent();
            })
            .AddTo(this);
    }
}
