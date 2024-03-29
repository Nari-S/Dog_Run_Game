﻿using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

internal class RightFlickInteraction : IInputInteraction
{
    private enum TapPhase
    {
        None,
        WaitingForRelease
    }

    // フリック検出開始時のタッチ位置
    private Vector2 touchPosition;

    private TapPhase tapPhase = TapPhase.None;

    // 最長のフリック時間，最短のフリック距離の閾値
    // 実機に搭載した際は閾値の変更が必要かも？
    public float maxFlickTime = 0.2f;
    public float minFlickDistance = 60f;

    //Editorにinteractionを表示
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
#else
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
    public static void Initialize()
    {
        InputSystem.RegisterInteraction<RightFlickInteraction>();
    }

    public void Process(ref InputInteractionContext context)
    {
        // タイムアウト判定
        if (context.timerHasExpired)
        {
            context.Canceled();
            return;
        }

        var touchInfo = context.ReadValue<TouchState>();

        var prevTouchPosition = touchPosition;
        touchPosition = touchInfo.position;

        if(touchInfo.delta.magnitude > minFlickDistance)
        {
            var differenceVector = touchPosition - prevTouchPosition;
            var angleAxisToVector = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;

            if (Mathf.Abs(angleAxisToVector) > 45) return; //-45<θ<45 で右フリックとする．

            context.Started();
            context.SetTimeout(maxFlickTime);
            tapPhase = TapPhase.WaitingForRelease;
        }

        if(tapPhase == TapPhase.WaitingForRelease && touchInfo.phase == UnityEngine.InputSystem.TouchPhase.Ended)
        {
            context.Performed();
        }
    }

    public void Reset()
    {
        tapPhase = TapPhase.None;
    }
}
