using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UniRx;

public class PeeCreator : MonoBehaviour
{
    [SerializeField] private Vector3 peeSize;
    [SerializeField] private float peeDuration;
    [SerializeField] private float peeWaterContentConsumption;

    [SerializeField] GameObject hitBoxCube;
    private bool isAttachedPee;

    [SerializeField] WaterContentManager waterContentManager;
    private DogAudioController dogAudioController;

    private Subject<Pee> _OnPeeCreated;
    public IObservable<Pee> OnPeeCreated => _OnPeeCreated;

    [SerializeField] private GameStatusManager gameStatusManager;

    private void Awake()
    {
        peeSize = new Vector3(1.5f, 1f, 3f);
        peeDuration = 0.5f;
        peeWaterContentConsumption = 30f;

        /* startで実行
        if (!TryGetComponent(out waterContentManager)) Debug.Log("IWaterContentManager is not attached to the object.");
        isAttachedPee = hitBoxCube.TryGetComponent<Pee>(out var _);

        if (!TryGetComponent(out dogAudioController)) Debug.Log("DogAudioController is not attached to this object."); */

        _OnPeeCreated = new Subject<Pee>();
    }

    private void Start()
    {
        if (!TryGetComponent(out waterContentManager)) Debug.Log("WaterContentManager is not attached to the object.");
        if (!(isAttachedPee = hitBoxCube.TryGetComponent<Pee>(out var _))) Debug.Log("IWaterContentManager is not attached to the object.");

        if (!TryGetComponent(out dogAudioController)) Debug.Log("DogAudioController is not attached to this object.");
    }

    /// <summary>
    /// ダブルタップにより呼び出される．ションベン攻撃の開始．
    /// </summary>
    /// <param name="context"></param>
    public void TriggerPee(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (gameStatusManager.gameStatus != GameStatusManager.GameStatus.Game) return;

        if (isAttachedPee)
        {
            if (waterContentManager.waterContent >= peeWaterContentConsumption)
            {
                dogAudioController.PlayAudio(DogAudioController.AudioKinds.bowwow);

                waterContentManager.UpdateWaterContent(-peeWaterContentConsumption);

                //Instantiate(hitBoxCube).GetComponent<Pee>().Init(peeSize, GetPeeCreationPosition(), peeDuration);
                var pee = Instantiate(hitBoxCube).GetComponent<Pee>();
                pee.Init(peeSize, GetPeeCreationPosition(), peeDuration);
                _OnPeeCreated.OnNext(pee); // ションベンクラスが作られたことを通知
            }
        }
        else
        {
            Debug.Log("Pee class is not attached to the object.");
        }
    }

    /* デバッグコード
    private void Update()
    {
        /* if文内を上記のTriggerPeeにコピペすることで実機で動作 
        if(Input.GetKeyDown(KeyCode.T))
        {
            if(isAttachedPee)
            {
                if (waterContentManager.waterContent >= peeWaterContentConsumption)
                {
                    waterContentManager.UpdateWaterContent(-peeWaterContentConsumption);

                    Instantiate(hitBoxCube).GetComponent<Pee>().Init(peeSize, GetPeeCreationPosition(), peeDuration);
                }
            }
            else
            {
                Debug.Log("Pee class is not attached to the object.");
            }
        }
    }*/

    /// <summary>
    /// ションベンの当たり判定オブジェクトの生成位置
    /// </summary>
    /// <returns></returns>
    private Vector3 GetPeeCreationPosition()
    {
        var peeCreationPosition = transform.position;

        if (TryGetComponent<CapsuleCollider>(out var playerCapsuleCollider))
        {
            peeCreationPosition.z -= playerCapsuleCollider.height / 2;
        }

        peeCreationPosition.z -= peeSize.z / 2;

        return peeCreationPosition;
    }
}