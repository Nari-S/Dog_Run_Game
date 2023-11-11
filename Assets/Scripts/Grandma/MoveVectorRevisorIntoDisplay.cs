using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MoveVectorRevisorIntoDisplay : MonoBehaviour
{
    [SerializeField] private ReverseCameraManager playerCameraManager; //Editorから設定
    [SerializeField] private float adjustIntoDisplayMoveVector; // カメラの端点からプレイヤー側に寄せる距離

    private void Awake()
    {
        adjustIntoDisplayMoveVector = 15f;
    }

    /// <summary>
    /// 敵の位置を移動ベクトルで移動後，プレイヤーの後方カメラの画面内に収まっていなければ，画面内に収まる移動ベクトルを返却．
    /// </summary>
    /// <param name="movePrevPosition">敵の位置</param>
    /// <param name="moveVector">敵の移動ベクトル</param>
    /// <returns></returns>
    public Vector3 ReviseMoveVectorIntoDisplay(Vector3 movePrevPosition, Vector3 moveVector)
    {
        var maxFurthestPointInViewRear = playerCameraManager.MaxFurthestPointInViewRear;

        return maxFurthestPointInViewRear + adjustIntoDisplayMoveVector > movePrevPosition.z + moveVector.z ? //画面内に収まっていなければtrue
            new Vector3(moveVector.x, moveVector.y, maxFurthestPointInViewRear + adjustIntoDisplayMoveVector - movePrevPosition.z) : //画面内に収まる移動ベクトルを返却
            moveVector; //元々の移動ベクトルを返却
    }
}
