using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UniRx;
using UniRx.Triggers;


public class DogSideMover : MonoBehaviour
{
    [SerializeField] private ReverseCameraManager reverseCameraManager;

    [SerializeField] private float mapWidth;
    [SerializeField] private float maxPhoneInclination;
    //[SerializeField] private float minMoveDistance;
    [SerializeField] private float maxSideMoveDistancePerSec;

    private Vector3 accelerometer;
    private Vector3 calculatedAccelerometer;

    private Queue<float> accelerometerXQueue;
    private int queueSize;

    private float timeExecutedInPrevFrame;

    [SerializeField] private GameStatusManager gameStatusManager;

    private void Awake()
    {
        accelerometer = Vector3.zero;
        calculatedAccelerometer = Vector3.zero;

        accelerometerXQueue = new Queue<float>();
        queueSize = 5;

        timeExecutedInPrevFrame = Time.time;
    }

    private void Start()
    {
        mapWidth = 3f;

        maxPhoneInclination = 0.5f;

        maxSideMoveDistancePerSec = 30f;

        /* ゲーム本編に遷移した際の時間を，前フレームに横移動が実行された時間にする */
        gameStatusManager.OnGameStatusChanged.Where(x => x == GameStatusManager.GameStatus.TitleToGame || gameStatusManager.gameStatus == GameStatusManager.GameStatus.Game).Subscribe(_ => timeExecutedInPrevFrame = Time.time).AddTo(this);

        /* ゲーム本編のみ，加速度を取得する */
        this.UpdateAsObservable().Where(_ => gameStatusManager.gameStatus == GameStatusManager.GameStatus.Game).Subscribe(_ => GetAccelerometer()).AddTo(this);

        /* カメラが後ろ向きから前向きに切り替えた際，横移動は前フレームに実行されたとする．これで，前向きになった際に横方向の瞬間移動を防ぐ． */
        reverseCameraManager.OnFacingFlont.Where(x => x).Subscribe(_ => timeExecutedInPrevFrame = Time.time).AddTo(this);
    }

    public Vector3 GetSideMoveVector()
    {
        if (calculatedAccelerometer == accelerometer) return Vector3.zero; // 加速度センサ値が更新された後の移動処理は，次回のセンサ値更新まで1回のみ受け付ける
        if (!reverseCameraManager.IsFacingFlont) return Vector3.zero; // 後ろを向いているときは横移動しない
        if (gameStatusManager.gameStatus != GameStatusManager.GameStatus.Game) return Vector3.zero; // ゲーム本編でのみ横移動する

        calculatedAccelerometer = accelerometer;

        /* 最新の加速度センサ値をキューに追加し，最も古い値をデキューする */
        accelerometerXQueue.Enqueue(accelerometer.x);
        if (accelerometerXQueue.Count > queueSize) accelerometerXQueue.Dequeue();

        /* キュー内のセンサ値の平均値を求める（センサ値の平滑化） */
        var accelerometerXSum = 0f;
        foreach (var item in accelerometerXQueue)
            accelerometerXSum += item;
        var lowPassAccelemterX = accelerometerXSum / accelerometerXQueue.Count;

        /* 加速度センサ値をクランプ後，目標移動地点を設定 */
        var clampedAccelerometerX = Mathf.Clamp(lowPassAccelemterX, -maxPhoneInclination, maxPhoneInclination);
        var destinationX = Map(clampedAccelerometerX, -maxPhoneInclination, maxPhoneInclination, -mapWidth / 2, mapWidth / 2);

        /* 前回，横移動ベクトルを生成した際の時間から現フレームの時間を引く．下記の移動距離の算出の際に使用 */
        var nowTime = Time.time;
        var deltaTimeFromPrevFrame = nowTime - timeExecutedInPrevFrame;
        timeExecutedInPrevFrame = nowTime;

        if (destinationX - transform.position.x > 0) return new Vector3(Mathf.Clamp(destinationX - transform.position.x, 0, maxSideMoveDistancePerSec * deltaTimeFromPrevFrame), 0, 0);
        else return new Vector3(Mathf.Clamp(destinationX - transform.position.x, -maxSideMoveDistancePerSec * deltaTimeFromPrevFrame, 0f), 0, 0);

    }

    public void GetAccelerometer()
    {
        accelerometer = Input.acceleration;
    }

    /// <summary>
    /// 渡された数値をある範囲から別の範囲に変換
    /// </summary>
    /// <param name="value">変換する入力値</param>
    /// <param name="start1">現在の範囲の下限</param>
    /// <param name="stop1">現在の範囲の上限</param>
    /// <param name="start2">変換する範囲の下限</param>
    /// <param name="stop2">変換する範囲の上限</param>
    /// <returns>変換後の値</returns>
    private float Map(float value, float start1, float stop1, float start2, float stop2)
    {
        return start2 + (stop2 - start2) * ((value - start1) / (stop1 - start1));
    }

    public void Reset()
    {
        timeExecutedInPrevFrame = Time.time;
    }
}
