using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrandmaSideMover : MonoBehaviour, IEnemySideMover
{
    [SerializeField] private Transform dogTransform;

    [SerializeField] private float maxSideMoveDistancePerSec;

    private float timeExecutedInPrevFrame;

    private void Awake()
    {
        maxSideMoveDistancePerSec = 0.5f; // 1秒間の最大移動距離

        timeExecutedInPrevFrame = Time.time;
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
