using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class AnimalDisposer : MonoBehaviour
{
    [SerializeField] private ReverseCameraManager reverseCameraManager;
    private IDisposable animalDisposer;

    private void OnEnable()
    {
        /* 手前から奥側へ向かって移動→カメラ奥側から出たときに動物を非有効化．
         * 奥側から手前へ向かって移動→カメラ手前側から出たときに動物を非有効化．*/
        if (transform.forward.z < 0) animalDisposer = this.UpdateAsObservable().Where(_ => transform.position.z < reverseCameraManager.MaxFurthestPointInViewRear).Subscribe(_ => gameObject.SetActive(false));
        else animalDisposer = this.UpdateAsObservable().Where(_ => transform.position.z > reverseCameraManager.MaxFurthestPointInViewFront).Subscribe(_ => gameObject.SetActive(false));
    }

    private void OnDisable()
    {
        animalDisposer.Dispose();
    }
}
