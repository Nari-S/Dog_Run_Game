using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UniRx;

public enum ObjectType
{
    HealingItem,
    DamageItem,
    WaterDrop,
    Obstacle,
    Animal
}

public struct ObjectCreationInfo
{
    public int creationMapNumber;
    public int nextUpdatingMapNumber;
    public GameObject creationObject;
    public bool isAnimalCreatedFront;
}

/// <summary>
/// 生成の重複を禁止する組み合わせ．
/// </summary>
public struct AppearanceConfliction
{
    public ObjectType objectType1;
    public ObjectType objectType2;

    public AppearanceConfliction(ObjectType objectType1, ObjectType objectType2)
    {
        this.objectType1 = objectType1;
        this.objectType2 = objectType2;
    }
}

public class ObjectOnRoadGenerator : MonoBehaviour
{
    Dictionary<ObjectType, int> nowAppearanceFrequencys; //現在のオブジェクトタイプごとの出現頻度
    Dictionary<ObjectType, List<int>> updatingAppearanceFrequencys; // オブジェクトタイプごとの出現頻度の更新リスト（時間経過で出現頻度がList<int>の値に更新される
    Dictionary<ObjectType, List<ObjectType>> appearanceConflictions; //オブジェクトタイプごとの生成Conflictリスト（同じマップに生成できないオブジェクトタイプの組み合わせのリスト）
    List<ItemType> preAppearanceItemTypes; //未出現の回復アイテム（肉・果物・野菜）リスト
    /* エディタから指定するゲームオブジェクトのリスト */
    [SerializeField] private List<GameObject> meatItems;
    [SerializeField] private List<GameObject> fruitItems;
    [SerializeField] private List<GameObject> vegetableItems;
    [SerializeField] private List<GameObject> damageItems;
    [SerializeField] private List<GameObject> waterDrops;
    [SerializeField] private List<GameObject> obstacles;
    [SerializeField] private List<GameObject> animals;
    private Dictionary<Enum, List<GameObject>> gameObjects; // オブジェクトのタイプとゲームオブジェクトの紐づけ
    List<int> conflictMapNumbers;

    [SerializeField] private GameStatusManager gameStatusManager;
    private float gameStartTime;
    private float appearanceFreqsChangeFreq;

    /// <summary>
    /// ObjectOnRoadGeneratorの初期化
    /// </summary>
    public void Init()
    {
        this.appearanceConflictions = new Dictionary<ObjectType, List<ObjectType>>();
        foreach(var objectType in Enum.GetValues(typeof(ObjectType)).Cast<ObjectType>())
        {
            this.appearanceConflictions.Add(objectType, new List<ObjectType>());
        }
        conflictMapNumbers = new List<int>();
        preAppearanceItemTypes = new List<ItemType>() { ItemType.Meat, ItemType.Fruit, ItemType.Vegetable };

        /* オブジェクト種類とゲームオブジェクトの紐づけを辞書に保存 */
        gameObjects = new Dictionary<Enum, List<GameObject>>() {
            { ItemType.Meat, meatItems }, { ItemType.Fruit, fruitItems }, { ItemType.Vegetable, vegetableItems }, { ObjectType.DamageItem, damageItems }, { ObjectType.WaterDrop, waterDrops }, { ObjectType.Obstacle, obstacles }, { ObjectType.Animal, animals }
        };

        /* 生成Conflictリストの生成 */
        var appearanceConflictionList = new List<AppearanceConfliction>() {
            { new AppearanceConfliction(ObjectType.HealingItem, ObjectType.DamageItem) },
            { new AppearanceConfliction(ObjectType.HealingItem, ObjectType.WaterDrop) },
            { new AppearanceConfliction(ObjectType.DamageItem, ObjectType.WaterDrop) },
        };
        foreach (var appearanceConfliction in appearanceConflictionList)
        {
            this.appearanceConflictions[appearanceConfliction.objectType1].Add(appearanceConfliction.objectType2);
            this.appearanceConflictions[appearanceConfliction.objectType2].Add(appearanceConfliction.objectType1);
        }

        /* オブジェクトごとの生成頻度決定 */
        nowAppearanceFrequencys = new Dictionary<ObjectType, int>() {
            { ObjectType.HealingItem, 5 }, { ObjectType.DamageItem, 5}, { ObjectType.WaterDrop, 12 }, { ObjectType.Obstacle, 8 }, { ObjectType.Animal, 11 }
        };

        appearanceFreqsChangeFreq = 20f; // オブジェクト生成頻度の更新間隔
        /* オブジェクトの出現頻度の更新リスト作成 */
        updatingAppearanceFrequencys = new Dictionary<ObjectType, List<int>>()
        {
            { ObjectType.HealingItem, new List<int>() { 5 } },
            { ObjectType.DamageItem, new List<int>() { 5 } },
            { ObjectType.WaterDrop, new List<int>() { 12 } },
            { ObjectType.Obstacle, new List<int>() { 8, 7, 6, 5, 4, 3, 2 } },
            { ObjectType.Animal, new List<int>() { 11, 10, 9, 8, 7, 6, 5 } }
        };

        gameStatusManager.OnGameStatusChanged.Where(x => x == GameStatusManager.GameStatus.Game).Subscribe(_ =>
        {
            gameStartTime = Time.time;

            /* appearanceFreqsChangeFreqの周期で，updatingAppearanceFrequencysのインデックスを進めることで，オブジェクトの出現頻度を早める */
            Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(appearanceFreqsChangeFreq)).Subscribe(__ => { // appearanceFreqsChangeFreq の周期でイベント発行
                /* updatingAppearanceFrequencys のList<int>内にて，現在のインデックスから1進めた値をオブジェクトの生成頻度として設定 */
                nowAppearanceFrequencys = Enum.GetValues(typeof(ObjectType)).Cast<ObjectType>().Select(x => // オブジェクトタイプをIEnumerable<ObjectType>で取得
                {
                    /* updatingAppearanceFrequencys の List<int>のインデックスを appearanceFreqsChangeFreq の周期で1増やす */
                    return (x, updatingAppearanceFrequencys[x][Mathf.Clamp((int)((Time.time - gameStartTime) / appearanceFreqsChangeFreq), 0, updatingAppearanceFrequencys[x].Count - 1)]);
                })
                .ToDictionary(tuple => tuple.x, tuple => tuple.Item2);
            }).AddTo(this);

        }).AddTo(this);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="objectCreationInfos"></param>
    /// <param name="mapCount"></param>
    /// <returns></returns>
    public Dictionary<ObjectType, ObjectCreationInfo> UpdateObjectCreationInfo(Dictionary<ObjectType, ObjectCreationInfo> objectCreationInfos, int mapCount) 
    {
        //Dictionary<ObjectType, ObjectCreationInfo> objectCreationInfosLocal = objectCreationInfos;
        ObjectCreationInfo tempObjectCreationInfo;

        /* 引数のobjectCreationInfos内のObjectCreationInfoが更新タイミングだった際，次回の生成マップ番号，次回の生成マップ番号の更新タイミング，オブジェクトの選択を行い，objectCreationInfosに反映 */
        foreach (var objectCreationInfoKey in objectCreationInfos.Keys.ToList())
        {
            tempObjectCreationInfo = objectCreationInfos[objectCreationInfoKey];

            /* オブジェクトの生成マップを決めるタイミングでなければ引数をそのまま返り値とする */
            if (tempObjectCreationInfo.nextUpdatingMapNumber != mapCount) continue;

            /* 現在のマップ番号 ～ 現在のマップ番号 + 出現頻度)の範囲内で生成するマップの番号を決定 */
            var creationMapNumber = UnityEngine.Random.Range(mapCount, mapCount + nowAppearanceFrequencys[objectCreationInfoKey]);

            /* Conflictリスト内のObjectTypeの生成マップ番号と一致しているか確認．一定している場合は，一致しない値に変更する */
            if (appearanceConflictions[objectCreationInfoKey] != null && appearanceConflictions[objectCreationInfoKey].Count > 0)
            {
                conflictMapNumbers.Clear();
                foreach(var conflictObjectType in appearanceConflictions[objectCreationInfoKey])
                {
                    conflictMapNumbers.Add(objectCreationInfos[conflictObjectType].creationMapNumber);
                }

                if(conflictMapNumbers.Contains(creationMapNumber)) //Conflictリスト内に生成マップ番号が同じ要素があった場合
                {
                    /* 生成マップ番号 + 1 ~ 該当のObjectTypeの生成マップ番号の上限の範囲で，新たな生成マップ番号を決める． */
                    for(int tempMapNumber = creationMapNumber + 1; tempMapNumber < mapCount + nowAppearanceFrequencys[objectCreationInfoKey]; tempMapNumber++)
                    {
                        if (!conflictMapNumbers.Contains(creationMapNumber)) break; // 更新後，Conflictリストに生成マップの重複がなければその番号を確定
                        if (conflictMapNumbers.Contains(tempMapNumber)) continue; // イテレーションを回して再度重複している場合はさらにループ

                        creationMapNumber = tempMapNumber; // 生成マップ番号更新
                    }

                    /* （上のループで決まらなかった場合）生成マップ番号 - 1 ~ 該当のObjectTypeの生成マップ番号の下限の範囲で，新たな生成マップ番号を決める． */
                    for (int tempMapNumber = creationMapNumber - 1; tempMapNumber >= tempObjectCreationInfo.nextUpdatingMapNumber; tempMapNumber--)
                    {
                        if (!conflictMapNumbers.Contains(creationMapNumber)) break; // 更新後，Conflictリストに生成マップの重複がなければその番号を確定
                        if (conflictMapNumbers.Contains(tempMapNumber)) continue; // イテレーションを回して再度重複している場合はさらにループ

                        creationMapNumber = tempMapNumber; // 生成マップ番号更新
                    }
                }
            }
            tempObjectCreationInfo.creationMapNumber = creationMapNumber;

            /* GameObjectの選択 */
            switch(objectCreationInfoKey)
            {
                case ObjectType.HealingItem:
                    if (preAppearanceItemTypes.Count == 0)
                    {
                        preAppearanceItemTypes.Add(ItemType.Meat);
                        preAppearanceItemTypes.Add(ItemType.Fruit);
                        preAppearanceItemTypes.Add(ItemType.Vegetable);
                    }

                    var selectedItemIndex = UnityEngine.Random.Range(0, preAppearanceItemTypes.Count);
                    var healingItemType = preAppearanceItemTypes[selectedItemIndex]; //リストからアイテムを1つ抜き出す
                    preAppearanceItemTypes.RemoveAt(selectedItemIndex);

                    tempObjectCreationInfo.creationObject = GetRandomInactiveGameObject(gameObjects[healingItemType]);

                    //Debug.Log("GameObjectName: " + tempObjectCreationInfo.creationObject.name);

                    /*if (healingItemType == ItemType.Meat)       tempObjectCreationInfo.creationObject = meatItems[UnityEngine.Random.Range(0, meatItems.Count)];
                    if (healingItemType == ItemType.Fruit)      tempObjectCreationInfo.creationObject = fruitItems[UnityEngine.Random.Range(0, meatItems.Count)];
                    if (healingItemType == ItemType.Vegetable)  tempObjectCreationInfo.creationObject = vegetableItems[UnityEngine.Random.Range(0, meatItems.Count)];*/

                    break;
                case ObjectType.DamageItem:
                case ObjectType.WaterDrop:
                case ObjectType.Obstacle:
                    tempObjectCreationInfo.creationObject = GetRandomInactiveGameObject(gameObjects[objectCreationInfoKey]); // keyに紐づくゲームオブジェクトのリストから，非有効化されている要素を1個選択
                    break;

                case ObjectType.Animal:
                    tempObjectCreationInfo.creationObject = GetRandomInactiveGameObject(gameObjects[objectCreationInfoKey]); // keyに紐づくゲームオブジェクトのリストから，非有効化されている要素を1個選択

                    tempObjectCreationInfo.isAnimalCreatedFront = Convert.ToBoolean(UnityEngine.Random.Range(0, 2)); // 0,1の乱数生成してfalse,trueに変換(正面から走ってくるか，後ろから走ってくるか)
                    break;
            }
            
            /* 次の更新タイミングを設定 */
            tempObjectCreationInfo.nextUpdatingMapNumber = tempObjectCreationInfo.nextUpdatingMapNumber + nowAppearanceFrequencys[objectCreationInfoKey];

            /* 返り値となる辞書を更新 */
            objectCreationInfos[objectCreationInfoKey] = tempObjectCreationInfo;
        }

        return objectCreationInfos;
    }

    private GameObject GetRandomInactiveGameObject(List<GameObject> gameObjects)
    {
        int randNum = UnityEngine.Random.Range(0, gameObjects.Count);

        for (int tempIndex = randNum + 1; tempIndex < gameObjects.Count; tempIndex++)
        {
            if (!gameObjects[randNum].activeInHierarchy) break; // 更新後，Conflictリストに生成マップの重複がなければその番号を確定
            if (gameObjects[tempIndex].activeInHierarchy) continue; // イテレーションを回して再度重複している場合はさらにループ

            randNum = tempIndex; // 生成マップ番号更新
        }

        for (int tempIndex = randNum - 1; tempIndex >= 0; tempIndex--)
        {
            if (!gameObjects[randNum].activeInHierarchy) break; // 更新後，Conflictリストに生成マップの重複がなければその番号を確定
            if (gameObjects[tempIndex].activeInHierarchy) continue; // イテレーションを回して再度重複している場合はさらにループ

            randNum = tempIndex; // 生成マップ番号更新
        }

        return gameObjects[randNum];
    }

}
