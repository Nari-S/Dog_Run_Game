using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;

public class AnimalDisposer : MonoBehaviour
{
    [SerializeField] private ReverseCameraManager reverseCameraManager;
    private IDisposable deleteTriggeredPositionSubscriber;

    private AnimalCollisionDetector animalCollisionDetector;
    private IDisposable deleteTriggeredEventSubscriber; // 動物を非有効化する購読
    private int durationFromEventToDelete; // 削除のイベント通知からオブジェクトを非有効化するまでの時間（ms)

    private void Awake()
    {
        durationFromEventToDelete = 3000;
    }

    private void OnEnable()
    {
        if (!TryGetComponent(out animalCollisionDetector)) Debug.Log("AnimalCollisionDetector is not attached to this gameobject");

        /* 手前から奥側へ向かって移動→カメラ奥側から出たときに動物を非有効化．
         * 奥側から手前へ向かって移動→カメラ手前側から出たときに動物を非有効化．*/
        if (transform.forward.z < 0)    deleteTriggeredPositionSubscriber = this.UpdateAsObservable().Where(_ => transform.position.z < reverseCameraManager.MaxFurthestPointInViewRear).Subscribe(_ => gameObject.SetActive(false));
        else deleteTriggeredPositionSubscriber = this.UpdateAsObservable().Where(_ => transform.position.z > reverseCameraManager.MaxFurthestPointInViewFront).Subscribe(_ => gameObject.SetActive(false));

        /* 削除イベント通知受理時，一定時間後にオブジェクトを非有効化 */
        deleteTriggeredEventSubscriber = animalCollisionDetector.OnDeleted.Subscribe(async _ =>
        {
            deleteTriggeredPositionSubscriber?.Dispose(); // カメラ外へ出た際に消去する購読を終了
            //await Task.Delay(durationFromEventToDelete); // durationFromEventToDelete後にオブジェクト非有効化
            await UniTask.Delay(durationFromEventToDelete); // durationFromEventToDelete後にオブジェクト非有効化
            gameObject.SetActive(false);
        });
    }

    private void OnDisable()
    {
        deleteTriggeredPositionSubscriber?.Dispose();
        deleteTriggeredEventSubscriber?.Dispose();
    }
}
