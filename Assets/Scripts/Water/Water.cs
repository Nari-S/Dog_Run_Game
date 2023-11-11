using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Water : MonoBehaviour
{
    [SerializeField] private float waterCotentChangeAmount = 2;
    private IWaterContentManager waterContentManager;
    private DogAudioController dogAudioController;

    private void Awake()
    {
        /* プレイヤーとの当たり判定時の処理
         * 水の回復を適用する*/
        this.OnTriggerEnterAsObservable()
            .Where(x => x.TryGetComponent(out waterContentManager) && x.gameObject.TryGetComponent(out dogAudioController))
            .Subscribe(_ =>
            {
                waterContentManager.UpdateWaterContent(waterCotentChangeAmount);
                dogAudioController.PlayDrinkingAudio();

                //Destroy(this.gameObject); //消える際の表現
                gameObject.SetActive(false);
            })
            .AddTo(this);
    }
}