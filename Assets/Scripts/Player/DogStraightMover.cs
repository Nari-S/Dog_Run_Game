using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class DogStraightMover : MonoBehaviour, IStraightMover
{
    public float StraightMoveSpeed { get; set; }
    public float MaxSpeed { get; private set; } // 移動の最大速度
    public float MinSpeed { get; private set; } // 移動の最低速度
    private float dashSpeedAdjustmentNum; // 腹減り度から移動速度に変換するために用いる値

    private HungerManager hungerManager;

    private float timeExecutedInPrevFrame;

    private DogAudioController dogAudioController;
    private DogAnimationManager dogAnimationManager;

    [SerializeField] private GameStatusManager gameStatusManager;

    private void Awake()
    {
        // Startで実行 hungerManager = GetComponent<HungerManager>();

        MaxSpeed = 10f;
        MinSpeed = 5f;

        timeExecutedInPrevFrame = Time.time;
    }

    private void Start()
    {
        hungerManager = GetComponent<HungerManager>();
        dogAudioController = GetComponent<DogAudioController>();
        dogAnimationManager = GetComponent<DogAnimationManager>();

        dashSpeedAdjustmentNum = hungerManager.MaxHunger / (MaxSpeed - MinSpeed);

        StraightMoveSpeed = ConvertHungerToSpeed();

        /* ゲーム本編に遷移した際の時間を，前フレームに横移動が実行された時間にする */
        gameStatusManager.OnGameStatusChanged.Where(x => x == GameStatusManager.GameStatus.TitleToGame).Subscribe(_ => 
        {
            dogAnimationManager.StartRunning();
            dogAudioController.PlayAudio(DogAudioController.AudioKinds.bowwow);
            timeExecutedInPrevFrame = Time.time;
        })
        .AddTo(this);
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