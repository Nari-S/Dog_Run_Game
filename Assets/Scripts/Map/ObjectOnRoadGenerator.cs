using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public enum ObjectType
{
    HealingItem,
    DamageItem,
    WaterDrop,
    Obstacle
}

public struct ObjectCreationInfo
{
    public int creationMapNumber;
    public int nextUpdatingMapNumber;
    public GameObject creationObject;
}

/// <summary>
/// 生成の重複を禁止する組み合わせ．primaryが優先するタイプ．
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

/* オブジェクトのタイプがItemTypeとObjectTypeの2つに分かれているため，実装がごちゃごちゃになっている
 * どちらも「肉・果物・野菜・ダメージ・水・障害物」で実装すれば簡単にできそう．．．*/
public class ObjectOnRoadGenerator : MonoBehaviour
{
    Dictionary<ObjectType, int> appearanceFrequencys; //オブジェクトタイプごとの出現頻度
    Dictionary<ObjectType, List<ObjectType>> appearanceConflictions; //オブジェクトタイプごとの生成Conflictリスト
    List<ItemType> preAppearanceItemTypes; //未出現の回復アイテム（肉・果物・野菜）リスト
    [SerializeField] private List<GameObject> meatItems;
    [SerializeField] private List<GameObject> fruitItems;
    [SerializeField] private List<GameObject> vegetableItems;
    [SerializeField] private List<GameObject> damageItems;
    [SerializeField] private List<GameObject> waterDrops;
    [SerializeField] private List<GameObject> obstacles;
    //private Dictionary<ItemType, List<GameObject>> itemGameObjects;
    private Dictionary<Enum, List<GameObject>> gameObjects;
    List<int> conflictMapNumbers;

    /// <summary>
    /// ObjectOnRoadGeneratorの初期化
    /// </summary>
    /// <param name="appearanceFrequencys">ObjectTypeごとの出現頻度</param>
    /// <param name="appearanceConflictions">生成の重複を禁止する組み合わせのリスト．primaryが優先．</param>
    public void Init(Dictionary<ObjectType, int> appearanceFrequencys, List<AppearanceConfliction> appearanceConflictions)
    {
        this.appearanceConflictions = new Dictionary<ObjectType, List<ObjectType>>();
        foreach(var objectType in Enum.GetValues(typeof(ObjectType)).Cast<ObjectType>())
        {
            this.appearanceConflictions.Add(objectType, new List<ObjectType>());
        }
        conflictMapNumbers = new List<int>();
        preAppearanceItemTypes = new List<ItemType>() { ItemType.Meat, ItemType.Fruit, ItemType.Vegetable };
        //itemGameObjects = new Dictionary<ItemType, List<GameObject>>() { { ItemType.Meat, meatItems }, { ItemType.Fruit, fruitItems }, { ItemType.Vegetable, vegetableItems } };
        gameObjects = new Dictionary<Enum, List<GameObject>>() {
            { ItemType.Meat, meatItems }, { ItemType.Fruit, fruitItems }, { ItemType.Vegetable, vegetableItems }, { ObjectType.DamageItem, damageItems }, { ObjectType.WaterDrop, waterDrops }, { ObjectType.Obstacle, obstacles }
        };

        /* 引数の処理 */
        this.appearanceFrequencys = appearanceFrequencys;
        foreach(var appearanceConfliction in appearanceConflictions) //生成Conflictリストを追加
        {
            this.appearanceConflictions[appearanceConfliction.objectType1].Add(appearanceConfliction.objectType2);
            this.appearanceConflictions[appearanceConfliction.objectType2].Add(appearanceConfliction.objectType1);
        }
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

        /* 10/8 key値だけ取得するように変更してみる */
        foreach (var objectCreationInfoKey in objectCreationInfos.Keys.ToList())
        {
            tempObjectCreationInfo = objectCreationInfos[objectCreationInfoKey];

            /* オブジェクトの生成マップを決めるタイミングでなければ引数をそのまま返り値とする */
            if (tempObjectCreationInfo.nextUpdatingMapNumber != mapCount) continue;

            /* 現在のマップ番号 ～ 現在のマップ番号 + 出現頻度)の範囲内で生成するマップの番号を決定 */
            var creationMapNumber = UnityEngine.Random.Range(mapCount, mapCount + appearanceFrequencys[objectCreationInfoKey]);

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
                    for(int tempMapNumber = creationMapNumber + 1; tempMapNumber < mapCount + appearanceFrequencys[objectCreationInfoKey]; tempMapNumber++)
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
                    tempObjectCreationInfo.creationObject = GetRandomInactiveGameObject(gameObjects[objectCreationInfoKey]);
                    break;
            }
            
            /* 次の更新タイミングを設定 */
            tempObjectCreationInfo.nextUpdatingMapNumber = tempObjectCreationInfo.nextUpdatingMapNumber + appearanceFrequencys[objectCreationInfoKey];

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
