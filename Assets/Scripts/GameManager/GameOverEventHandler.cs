using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class GameOverEventHandler : MonoBehaviour
{
    [SerializeField] GameObject player;
    private IObstacleReceivable obstacleReceivable;

    /// <summary>
    /// obstacleReceivable.OnGameOverd の初期化がAwakeで行われるため，SubscribeをStartで実施
    /// </summary>
    private void Start()
    {
        if (!player.TryGetComponent(out obstacleReceivable)) Debug.Log("IObstacleReceivable is not attached to player");

        obstacleReceivable.OnGameOverd
            .Subscribe(_ => Debug.Log("Game Over Event will be handled in this event."))
            .AddTo(this);
    }
}
