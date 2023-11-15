using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UniRx;


public class DogSideMover : MonoBehaviour, ISideMover
{
    /* InputSystem?????CSensor/Accelerometer???????n????????????????????
       Code????Action?L?q */
    [SerializeField] private InputAction sideMoveAction;
    [SerializeField] private ReverseCameraManager reverseCameraManager;

    [SerializeField] private float mapWidth;
    [SerializeField] private float maxPhoneInclination;
    //[SerializeField] private float minMoveDistance;
    [SerializeField] private float maxSideMoveDistancePerSec;

    private Vector3 accelerometer;
    private Vector3 calculatedAccelerometer;

    private IStepMover stepMover;

    private Queue<float> accelerometerXQueue;
    private int queueSize;

    private float timeExecutedInPrevFrame;

    [SerializeField] private GameStatusManager gameStatusManager;

    private void Awake()
    {
        accelerometer = Vector3.zero;
        calculatedAccelerometer = Vector3.zero;

        // startで実行　stepMover = GetComponent<IStepMover>();

        accelerometerXQueue = new Queue<float>();
        queueSize = 5;

        timeExecutedInPrevFrame = Time.time;
    }

    private void Start()
    {
        stepMover = GetComponent<IStepMover>();

        sideMoveAction.performed += GetAccelerometer;
        sideMoveAction?.Enable();

        mapWidth = 3f;

        maxPhoneInclination = 0.5f;

        maxSideMoveDistancePerSec = 30f;

        /* ゲーム本編に遷移した際の時間を，前フレームに横移動が実行された時間にする */
        gameStatusManager.OnGameStatusChanged.Where(x => x == GameStatusManager.GameStatus.TitleToGame || gameStatusManager.gameStatus == GameStatusManager.GameStatus.Game).Subscribe(_ => timeExecutedInPrevFrame = Time.time).AddTo(this);
    }

    /* Startで実行
     * private void OnEnable()
    {
        sideMoveAction.performed += GetAccelerometer;
        sideMoveAction?.Enable();

        mapWidth = 3f;

        maxPhoneInclination = 0.5f;

        //minMoveDistance = 0.01f;
        maxSideMoveDistancePerSec = 30f;
    }*/

    private void OnDisable()
    {
        sideMoveAction.performed -= GetAccelerometer;
        sideMoveAction?.Disable();
    }

    public Vector3 GetSideMoveVector()
    {
        if (stepMover.IsStepping) return Vector3.zero; //?X?e?b?v??????????????????
        if (calculatedAccelerometer == accelerometer) return Vector3.zero;
        if (gameStatusManager.gameStatus != GameStatusManager.GameStatus.Game) return Vector3.zero;

        calculatedAccelerometer = accelerometer;

        /* ?????????@???????????x?Z???T?l???????? */
        accelerometerXQueue.Enqueue(accelerometer.x);
        if (accelerometerXQueue.Count > queueSize) accelerometerXQueue.Dequeue();

        var accelerometerXSum = 0f;
        foreach (var item in accelerometerXQueue)
            accelerometerXSum += item;
        var lowPassAccelemterX = accelerometerXSum / accelerometerXQueue.Count;

        /* 加速度センサ値をクランプ後，目標移動地点を設定 */
        var clampedAccelerometerX = Mathf.Clamp(lowPassAccelemterX, -maxPhoneInclination, maxPhoneInclination);
        var destinationX = Map(clampedAccelerometerX, -maxPhoneInclination, maxPhoneInclination, -mapWidth / 2, mapWidth / 2);

        var nowTime = Time.time;
        var deltaTimeFromPrevFrame = nowTime - timeExecutedInPrevFrame;
        timeExecutedInPrevFrame = nowTime;

        if (destinationX - transform.position.x > 0) return new Vector3(Mathf.Clamp(destinationX - transform.position.x, 0, maxSideMoveDistancePerSec * deltaTimeFromPrevFrame), 0, 0);
        else return new Vector3(Mathf.Clamp(destinationX - transform.position.x, -maxSideMoveDistancePerSec * deltaTimeFromPrevFrame, 0f), 0, 0);

    }

    public void GetAccelerometer(InputAction.CallbackContext context)
    {
        if(reverseCameraManager.IsFacingFlont) accelerometer = context.ReadValue<Vector3>();
        else accelerometer = -context.ReadValue<Vector3>();
    }

    /// <summary>
    /// ?n?????????l????????????????????????????
    /// </summary>
    /// <param name="value">?????????????l</param>
    /// <param name="start1">????????????????</param>
    /// <param name="stop1">????????????????</param>
    /// <param name="start2">??????????????????</param>
    /// <param name="stop2">??????????????????</param>
    /// <returns>?????????l</returns>
    private float Map(float value, float start1, float stop1, float start2, float stop2)
    {
        return start2 + (stop2 - start2) * ((value - start1) / (stop1 - start1));
    }

    public void Reset()
    {
        timeExecutedInPrevFrame = Time.time;
    }
}
