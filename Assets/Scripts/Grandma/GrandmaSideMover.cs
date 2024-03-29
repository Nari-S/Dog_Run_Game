using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class GrandmaSideMover : MonoBehaviour
{
    [SerializeField] private Transform dogTransform;

    [SerializeField] private float maxSideMoveDistancePerSec;

    private float timeExecutedInPrevFrame;

    [SerializeField] private GameStatusManager gameStatusManager;

    private void Awake()
    {
        maxSideMoveDistancePerSec = 0.5f; // 1秒間の最大移動距離

        timeExecutedInPrevFrame = Time.time;
    }

    private void Start()
    {
        /* ゲーム本編遷移時，横移動が行われた時刻を現在時刻に更新 */
        gameStatusManager.OnGameStatusChanged.Where(x => x == GameStatusManager.GameStatus.Game).Subscribe(_ => timeExecutedInPrevFrame = Time.time).AddTo(this);
    }

    public Vector3 GetSideMoveVector()
    {
        var deltaXDogGrandma = dogTransform.position.x - transform.position.x;

        var nowTime = Time.time;
        var deltaTimeFromPrevFrame = nowTime - timeExecutedInPrevFrame;
        timeExecutedInPrevFrame = nowTime;

        return new Vector3(Mathf.Clamp(deltaXDogGrandma, -maxSideMoveDistancePerSec * deltaTimeFromPrevFrame, maxSideMoveDistancePerSec * deltaTimeFromPrevFrame), 0, 0);
    }

    public void Reset()
    {
        timeExecutedInPrevFrame = Time.time;
    }

}
