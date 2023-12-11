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
    private float startPositionZ; // 移動開始地点
    private IDisposable moverDisposer;

    private void Awake()
    {
        if (!TryGetComponent(out characterController)) Debug.Log("Character Controller is not attached to this gameobject");

        baseMoveSpeedPerSec = 8.0f;
        addedMoveSpeedPerSec = 2.0f;
    }

    private void OnEnable()
    {
        startPositionZ = transform.position.z;

        /* 手前から奥側へ向かって移動→毎秒moveSpeedPerSec + addedMoveSpeedPerSec移動．
         * 奥側から手前へ向かって移動→毎秒moveSpeedPerSec移動．*/
        moverDisposer = this.UpdateAsObservable().Subscribe(_ => characterController.Move(transform.forward * (transform.forward.z < 0 ? baseMoveSpeedPerSec : baseMoveSpeedPerSec + addedMoveSpeedPerSec) * Time.deltaTime)); 
    }

    private void OnDisable()
    {
        moverDisposer.Dispose(); // 動物を移動させるストリームの購読終了
    }
}
