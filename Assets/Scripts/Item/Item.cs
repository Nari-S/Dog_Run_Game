using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

enum ItemType
{
    Meat,
    Fruit,
    Vegetable,
    Damage
}

public class Item : MonoBehaviour
{
    private Dictionary<ItemType, float> changeHungerAmountDic;
    [SerializeField] private ItemType itemType;
    private IHungerManager hungerManager;
    private DogAudioController dogAudioController;

    private void Awake()
    {
        /* アイテム種類ごとに回復量を設定
         * アイテム種類はエディタでプレファブごとに設定*/
        changeHungerAmountDic = new Dictionary<ItemType, float>() {
            {ItemType.Meat, 30}, {ItemType.Fruit, 20}, {ItemType.Vegetable, 10}, {ItemType.Damage, -20}
        };

        /* プレイヤーとの当たり判定時の処理
         * アイテム種類に応じて回復/ダメージを与える*/
        this.OnTriggerEnterAsObservable()
            .Where(x => x.TryGetComponent(out hungerManager) && x.gameObject.TryGetComponent(out dogAudioController))
            .Subscribe(_ =>
            {
                hungerManager.UpdateHunger(changeHungerAmountDic[itemType]);
                /* アイテム種類がダメージ系ならクゥーン，回復系ならパクッを再生 */
                if (itemType == ItemType.Damage) dogAudioController.PlayAudio(DogAudioController.AudioKinds.whine);
                else dogAudioController.PlayAudio(DogAudioController.AudioKinds.eating);

                //Destroy(this.gameObject); //消える際の表現
                gameObject.SetActive(false);
            })
            .AddTo(this);
    }

    private void Update()
    { 
        transform.RotateAround(transform.position, Vector3.up, 90 * Time.deltaTime); //毎秒Y軸中心に90度回転
    }
}