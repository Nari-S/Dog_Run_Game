using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class AnimalMover : MonoBehaviour
{
    private CharacterController characterController;
    private float baseMoveSpeedPerSec; // 毎秒移動する距離
    private float addedMoveSpeedPerSec; // 手前側から奥側へ向かって移動する動物の移動速度への加算速度
    private IDisposable moveSubscriber; // 動物を移動させるの購読

    private AnimalCollisionDetector animalCollisionDetector;
    private IDisposable stopSubscriber; // 動物を移動停止させる購読

    private void Awake()
    {
        baseMoveSpeedPerSec = 6.0f;
        addedMoveSpeedPerSec = 5.0f;
    }

    private void OnEnable()
    {
        if (!TryGetComponent(out characterController)) Debug.Log("Character Controller is not attached to this gameobject");
        if (!TryGetComponent(out animalCollisionDetector)) Debug.Log("AnimalCollisionDetector is not attached to this gameobject");

        /* 手前から奥側へ向かって移動→毎秒moveSpeedPerSec + addedMoveSpeedPerSec移動．
         * 奥側から手前へ向かって移動→毎秒moveSpeedPerSec移動．*/
        moveSubscriber = this.UpdateAsObservable().Subscribe(_ => characterController.Move(transform.forward * (transform.forward.z < 0 ? baseMoveSpeedPerSec : baseMoveSpeedPerSec + addedMoveSpeedPerSec) * Time.deltaTime));

        stopSubscriber = animalCollisionDetector.OnDeleted.Subscribe(_ => moveSubscriber?.Dispose()); // ションベンにぶつかると移動停止する
    }

    private void OnDisable()
    {
        moveSubscriber?.Dispose(); // 動物を移動させるストリームの購読終了
        stopSubscriber?.Dispose(); // ションベンにぶつかるストリームの購読終了
    }
}
