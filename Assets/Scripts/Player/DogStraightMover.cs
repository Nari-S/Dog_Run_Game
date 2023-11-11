using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogStraightMover : MonoBehaviour, IStraightMover
{
    public float StraightMoveSpeed { get; set; }

    public float MaxSpeed { get; private set; } // 移動の最大速度
    public float MinSpeed { get; private set; } // 移動の最低速度
    private float dashSpeedAdjustmentNum; // 腹減り度から移動速度に変換するために用いる値

    private HungerManager hungerManager;

    private float timeExecutedInPrevFrame;

    private void Awake()
    {
        hungerManager = GetComponent<HungerManager>();

        MaxSpeed = 6f;
        MinSpeed = 1f;

        timeExecutedInPrevFrame = Time.time;
    }

    /* Hungerの初期化はAwakeで行われているため，Startで初期化 （スクリプトの実行順を付けることでAwakeのみにできる？）*/
    private void Start()
    {
        dashSpeedAdjustmentNum = hungerManager.MaxHunger / (MaxSpeed - MinSpeed);

        StraightMoveSpeed = ConvertHungerToSpeed();
    }

    public Vector3 GetStraightMoveVector()
    {

        StraightMoveSpeed = ConvertHungerToSpeed();

        var nowTime = Time.time;
        var deltaTimeFromPrevFrame = nowTime - timeExecutedInPrevFrame;
        timeExecutedInPrevFrame = nowTime;

        return new Vector3(0, 0, StraightMoveSpeed * deltaTimeFromPrevFrame);
    }

    private float ConvertHungerToSpeed()
    {
        //StraightMoveSpeed = (hungerManager.Hunger.Value / 10f) < minHunger ? minHunger : hungerManager.Hunger.Value / 10f;
        return hungerManager.Hunger.Value / dashSpeedAdjustmentNum + MinSpeed;
    }

    public void Reset()
    {
        timeExecutedInPrevFrame = Time.time;
    }
}