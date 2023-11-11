using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class GrandmaStraightMover : MonoBehaviour, IEnemyStraightMover
{
    public float StraightMoveSpeed { get; set; }

    public float MaxSpeed { get; private set; } // 移動の最大速度
    public float MinSpeed { get; private set; } // 移動の最低速度
    private float dashSpeedAdjustmentNum; // 腹減り度から移動速度に変換するために用いる値

    private IEnemyHungerManager hungerManager;
    private MoveVectorRevisorIntoDisplay moveVectorRevisor;

    private float timeExecutedInPrevFrame;

    private void Awake()
    {
        //hungerManager = GetComponent<IEnemyHungerManager>();
        if (!TryGetComponent(out hungerManager)) Debug.Log("IEnemyHungerManager is not attached");
        if (!TryGetComponent(out moveVectorRevisor)) Debug.Log("MoveVectorRevisorIntoDisplay is not attached");

        MaxSpeed = 6f;
        MinSpeed = 1f;

        timeExecutedInPrevFrame = Time.time;
    }

    /* Hungerの初期化はAwakeで行われているため，Startで初期化 （スクリプトの実行順を付けることでAwakeのみにできる？）*/
    private void Start()
    {
        dashSpeedAdjustmentNum = hungerManager.MaxHunger / (MaxSpeed - MinSpeed);

        hungerManager.Hunger
            .Subscribe(hungerValue => {
                StraightMoveSpeed = ConvertHungerToSpeed(hungerValue);
            })
            .AddTo(this);
    }

    public Vector3 GetStraightMoveVector()
    {
        //Debug.Log("Now time: " + Time.time + ", ExecutedInPrevFrame: " + timeExecutedInPrevFrame);

        var nowTime = Time.time;
        var deltaTimeFromPrevFrame = nowTime - timeExecutedInPrevFrame;
        timeExecutedInPrevFrame = nowTime;

        return moveVectorRevisor.ReviseMoveVectorIntoDisplay(transform.position, new Vector3(0, 0, StraightMoveSpeed * deltaTimeFromPrevFrame));
    }

    private float ConvertHungerToSpeed(float hungerValue)
    {
        return hungerValue / dashSpeedAdjustmentNum + MinSpeed;
    }

    public void Reset()
    {
        timeExecutedInPrevFrame = Time.time; // ラッシュ終了後のGetStraightMoveVector()では，ラッシュ前後の時間で移動量を計算するため，リセット

        //Debug.Log("StraightMover is Reset. Time is " + timeExecutedInPrevFrame);
    }
}
