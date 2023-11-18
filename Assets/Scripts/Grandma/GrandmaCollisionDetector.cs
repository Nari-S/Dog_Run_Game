using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class GrandmaCollisionDetector : MonoBehaviour
{
    private IObstacleReceivable obstacleReceivable;
    private DogAudioController dogAudioController;

    private void Awake()
    {
        /* プレイヤーとの当たり判定時の処理
         * ゲームオーバーとする　*/
         /* Startで実行
        this.OnTriggerEnterAsObservable()
            .Where(x => x.TryGetComponent(out obstacleReceivable) && x.gameObject.TryGetComponent(out dogAudioController))
            .Subscribe(_ =>
            {
                dogAudioController.PlayWhineAudio();
                obstacleReceivable.NotifyGameOverEvent();
            })
            .AddTo(this);
        */
    }

    private void Start()
    {
        this.OnTriggerEnterAsObservable()
            .Where(x => x.TryGetComponent(out obstacleReceivable) && x.gameObject.TryGetComponent(out dogAudioController))
            .Subscribe(_ =>
            {
                dogAudioController.PlayWhineAudio();
                obstacleReceivable.NotifyGameOverEvent();
            })
            .AddTo(this);
    }
}
