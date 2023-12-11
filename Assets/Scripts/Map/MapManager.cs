using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

/* マップ管理オブジェクトにアタッチ */
public class MapManager : MonoBehaviour
{
    private class ActiveMapChildren
    {
        public Transform mapTransform;
        public Dictionary<ObjectType, ObjectCreationInfo> objectOnRoadCreationInfos;

        public ActiveMapChildren(Transform mapTransform, Dictionary<ObjectType, ObjectCreationInfo> objectOnRoadCreationInfos)
        {
            this.mapTransform = mapTransform;
            this.objectOnRoadCreationInfos = objectOnRoadCreationInfos;
        }

        /// <summary>
        /// objectOnRoadCreationInfos内のオブジェクトを有効化する
        /// </summary>
        /// <param name="activation"></param>
        public void SetActiveObjects(bool activation)
        {
            objectOnRoadCreationInfos = objectOnRoadCreationInfos.Select(x => { x.Value.creationObject.SetActive(activation); return x; }).ToDictionary(d => d.Key, d => d.Value);
        }
    }

    private List<Transform> inactiveChildrenMapTransform;
    private List<Transform> activeChildrenMapTransform;
    private List<ActiveMapChildren> activeMapChildren; 

    [SerializeField] private Camera playerCamera;

    [SerializeField] private Terrain mapWidthReference;
    private float commonMapLength; // マップ1個あたりの奥行方向の長さ

    private Transform nextMonitoringMapForFrontCreating;
    private Transform nextMonitoringMapForRearDeleting;

    private Vector3 deactivationMapPosition;

    private Dictionary<ObjectType, ObjectCreationInfo> objectCreationInfos;

    private ObjectOnRoadGenerator objectOnRoadGenerator;

    [SerializeField] private GameObject leftWall; // マップの壁のオブジェクトを設定
    [SerializeField] private GameObject rightWall;
    private int mapWidthDivision;
    private List<float> objectsCreationPositions;
    private List<int> usedRaneNumbers;

    private int mapCreationNum; //生成したマップの数（初期作成数を除く）

    private void Awake()
    {
        deactivationMapPosition = new Vector3(0, 999, 0);

        inactiveChildrenMapTransform = new List<Transform>();
        activeChildrenMapTransform = new List<Transform>();
        activeMapChildren = new List<ActiveMapChildren>();

        UnityEngine.Random.InitState(System.DateTime.Now.Millisecond);

        /* 分割されたマップを全て取得 */
        for (int i = 0; i < transform.childCount; i++)
        {
            inactiveChildrenMapTransform.Add(transform.GetChild(i));

            /* マップを非表示にしてカメラの映らない場所へ移動 */
            transform.GetChild(i).gameObject.SetActive(false);
            transform.GetChild(i).position = deactivationMapPosition;
        }

        commonMapLength = mapWidthReference.terrainData.size.z;

        if (TryGetComponent(out objectOnRoadGenerator)) objectOnRoadGenerator.Init();
        else Debug.Log("objectOnRoadGenerator is not attached to GameObject.");

        objectCreationInfos = new Dictionary<ObjectType, ObjectCreationInfo>()
        {
            { ObjectType.HealingItem, new ObjectCreationInfo() }, { ObjectType.DamageItem, new ObjectCreationInfo() }, { ObjectType.WaterDrop, new ObjectCreationInfo() }, { ObjectType.Obstacle, new ObjectCreationInfo() }, { ObjectType.Animal, new ObjectCreationInfo() }
        };

        /* オブジェクト生成位置の取得 */
        usedRaneNumbers = new List<int>();
        var mapWidth = Mathf.Abs(leftWall.transform.position.x) + Mathf.Abs(rightWall.transform.position.x);
        mapWidthDivision = 4;
        objectsCreationPositions = Enumerable.Range(1, mapWidthDivision * 2).Where(x => x % 2 == 1).Select(x => leftWall.transform.position.x + mapWidth / mapWidthDivision / 2 * x).ToList();

        mapCreationNum = 0;
    }

    private void Start()
    {
        InitMap();
    }

    private void Update()
    {
        /* マップ上に生成するオブジェクト情報を更新 */
        objectCreationInfos = objectOnRoadGenerator.UpdateObjectCreationInfo(objectCreationInfos, mapCreationNum);

        /* マップを生成 */
        UpdateMap();
    }

    /// <summary>
    /// 前方のマップを作成，後方のマップを削除
    /// </summary>
    private void UpdateMap()
    {
        var MaxFurthestPointInViewFront = playerCamera.GetComponent<ReverseCameraManager>().MaxFurthestPointInViewFront;
        var MaxFurthestPointInViewRear = playerCamera.GetComponent<ReverseCameraManager>().MaxFurthestPointInViewRear;

        if (!IsEndOfMapOutOfCamera(MaxFurthestPointInViewRear, MaxFurthestPointInViewFront, nextMonitoringMapForFrontCreating.position + new Vector3(0, 0, commonMapLength)))
        {
            /* 最も奥に生成されているマップの後ろに新たなマップを作成 */
            var createdMapTransform = CreateMap(nextMonitoringMapForFrontCreating.position + new Vector3(0, 0, commonMapLength * 2));

            /* 新規作成マップ上にオブジェクトを作成 */
            var selectedObjectCreationInfos = CreateObjectsOnRoad(createdMapTransform, nextMonitoringMapForFrontCreating.position + new Vector3(0, 0, commonMapLength * 2), nextMonitoringMapForRearDeleting.position);

            /* マップとオブジェクト（動物除く）を有効化オブジェクトリストに追加する．マップを非有効化する際，このリストを参照してマップ上オブジェクトを非有効化する．（動物の非有効化処理は，自身で行う） */
            activeMapChildren.Add(new ActiveMapChildren(createdMapTransform, selectedObjectCreationInfos.Where(x => x.Key != ObjectType.Animal).ToDictionary(d => d.Key, d => d.Value)));

            /* 前方奥から2番目のマップを監視対象に設定（最奥のマップは，上記CreateMapメソッドで作成したもの */
            nextMonitoringMapForFrontCreating = activeChildrenMapTransform[activeChildrenMapTransform.Count - 2];

            /* マップ生成数をカウント */
            mapCreationNum++;
        }

        if (IsEndOfMapOutOfCamera(MaxFurthestPointInViewRear, MaxFurthestPointInViewFront, nextMonitoringMapForRearDeleting.position + new Vector3(0, 0, commonMapLength)))
        {
            /* 後方最も奥に生成されているマップを削除 */
            DeleteMap(nextMonitoringMapForRearDeleting);

            /* 後方最も奥のマップを監視対象に設定 */
            nextMonitoringMapForRearDeleting = activeChildrenMapTransform[0];
        }
    }

    /// <summary>
    /// マップ上にオブジェクトを配置・有効化
    /// </summary>
    /// <param name="createdMapTransform">直前に生成されたマップのTransform</param>
    /// <param name="mapPosition">直前に生成されたマップの座標</param>
    /// <returns></returns>
    private Dictionary<ObjectType, ObjectCreationInfo> CreateObjectsOnRoad(Transform createdMapTransform, Vector3 frontMapPosition, Vector3 rearMapPosition)
    {
        int randNum;
        usedRaneNumbers.Clear();

        var selectedObjectCreationInfos = objectCreationInfos
            .Where(x => x.Value.creationMapNumber == mapCreationNum)
            .Select(x => {
                switch(x.Key)
                {
                    case ObjectType.HealingItem:
                    case ObjectType.DamageItem:
                    case ObjectType.Obstacle:
                        /* 未使用のレーン番号を取得（横方向の配置が重ならないようにする） */
                        while (usedRaneNumbers.Contains(randNum = UnityEngine.Random.Range(0, objectsCreationPositions.Count))) ;
                        usedRaneNumbers.Add(randNum);

                        /* オブジェクトの位置を右記座標に指定． x座標：分割したレーンのいずれか．y座標：オブジェクトの元の座標．z座標：マップ範囲内でランダム */
                        x.Value.creationObject.transform.position = new Vector3(objectsCreationPositions[randNum], x.Value.creationObject.transform.position.y, frontMapPosition.z + commonMapLength * UnityEngine.Random.Range(0.5f,0.95f));
                        x.Value.creationObject.SetActive(true);
                        break;

                    case ObjectType.WaterDrop:
                        /* 未使用のレーン番号を取得（横方向の配置が重ならないようにする） */
                        while (usedRaneNumbers.Contains(randNum = UnityEngine.Random.Range(0, objectsCreationPositions.Count))) ;
                        usedRaneNumbers.Add(randNum);

                        /* オブジェクトの位置を右記座標に指定． x座標：分割したレーンのいずれか．y座標：オブジェクトの元の座標．z座標：マップ縦方向中心 */
                        x.Value.creationObject.transform.position = new Vector3(objectsCreationPositions[randNum], x.Value.creationObject.transform.position.y, frontMapPosition.z + 0.5f);
                        x.Value.creationObject.SetActive(true);
                        /* 子オブジェクトも有効化(以前の出現時に取得されていると非有効化されているため)　これ，WaterDropのOnDisableで実現できないか？*/ 
                        var waterDrops = x.Value.creationObject.transform.OfType<Transform>()/*.Select(z => { z.gameObject.SetActive(true); Debug.Log(z.gameObject.name); return z; })*/;
                        foreach (var waterDrop in waterDrops) waterDrop.gameObject.SetActive(true);

                        break;

                    case ObjectType.Animal:
                        /* オブジェクトの位置を右記座標に指定． x座標：分割したレーン内でランダム．y座標：オブジェクトの元の座標．z座標：フラグisAnimalCreatedFrontに基づいてマップ最奥 or 最前 */
                        /* 12/9 出現頻度の変化を実現 */
                        x.Value.creationObject.transform.position = new Vector3(UnityEngine.Random.Range(objectsCreationPositions[0], objectsCreationPositions[objectsCreationPositions.Count - 1]), x.Value.creationObject.transform.position.y, x.Value.isAnimalCreatedFront ? frontMapPosition.z : rearMapPosition.z);
                        x.Value.creationObject.transform.localEulerAngles = new Vector3(0, 180f * Convert.ToInt16(x.Value.isAnimalCreatedFront), 0); // プレイヤーの方向を向く
                        x.Value.creationObject.SetActive(true);
                        break;
                }
                return x;
            })
            .ToDictionary(x => x.Key, x => x.Value);

        return selectedObjectCreationInfos;
    }

    

    /// <summary>
    /// 初期マップ作成．カメラ端の座標計算はReverseCameraManagerのAwake()で実施されるため，マップ初期化はStart()で実施
    /// </summary>
    private void InitMap()
    {
        var MaxFurthestPointInViewFront = playerCamera.GetComponent<ReverseCameraManager>().MaxFurthestPointInViewFront;
        var MaxFurthestPointInViewRear = playerCamera.GetComponent<ReverseCameraManager>().MaxFurthestPointInViewRear;

        //Transform childMapTransform = SelectMapFromInactiveChildren();

        /* それぞれの方向に作成するマップの数を算出 */
        var frontMapCreationNum = Mathf.Abs((int)(MaxFurthestPointInViewFront / commonMapLength) + 2);
        var rearMapCreationNum = Mathf.Abs((int)(MaxFurthestPointInViewRear / commonMapLength) - 1);

        /* マップ生成 */
        for (int i = -rearMapCreationNum; i < frontMapCreationNum; i++)
        {
            var createdMapTransform = CreateMap(new Vector3(0, 0, i * commonMapLength));

            /* マップの生成・削除のために監視するマップを取得 */
            if (i == -rearMapCreationNum) nextMonitoringMapForRearDeleting = createdMapTransform;
            if (i == frontMapCreationNum - 2) nextMonitoringMapForFrontCreating = createdMapTransform;
        }
    }

    /// <summary>
    /// 非アクティブなマップリストから1個マップを選択し有効化する
    /// </summary>
    /// <param name="mapPosition"></param>
    /// <returns></returns>
    private Transform CreateMap(Vector3 mapPosition)
    {
        var selectedMapTransform = GetAndRemoveMapTransformFromInactiveChildren();

        ActivateMap(selectedMapTransform, mapPosition);

        return selectedMapTransform;
    }

    /// <summary>
    /// 引数のTransformのMapをシーンで非表示にし，有効/非有効マップのリストを更新する
    /// </summary>
    /// <param name="deleteMapTransform"></param>
    private void DeleteMap(Transform deleteMapTransform)
    {
        activeChildrenMapTransform.Remove(deleteMapTransform); /* 後方端のマップを有効化されているマップのリストから削除 */

        inactiveChildrenMapTransform.Add(deleteMapTransform);

        DeactivateMap(deleteMapTransform);

        /* マップにオブジェクトも含まれる場合，一緒に非有効化する */
        if(activeMapChildren.Count > 0 && activeMapChildren[0].mapTransform == deleteMapTransform)
        {
            activeMapChildren[0].SetActiveObjects(false);
            activeMapChildren.RemoveAt(0);
        }
    }

    /// <summary>
    /// マップを表示
    /// </summary>
    /// <param name="inactiveChildMapTransform"></param>
    /// <param name="placePosition"></param>
    private void ActivateMap(Transform inactiveChildMapTransform, Vector3 placePosition)
    {
        inactiveChildMapTransform.gameObject.SetActive(true);
        inactiveChildMapTransform.position = placePosition;
    }

    /// <summary>
    /// マップを非表示
    /// </summary>
    /// <param name="activeChildMapTransform"></param>
    private void DeactivateMap(Transform activeChildMapTransform)
    {
        activeChildMapTransform.gameObject.SetActive(false);
        activeChildMapTransform.position = deactivationMapPosition;
    }

    /// <summary>
    /// 端のマップがカメラ外に出たか判定．
    /// </summary>
    /// <param name="rearFurthestCameraPositionZ">手前側カメラ端の座標</param>
    /// <param name="frontFurthestCameraPositionZ">奥側カメラ端の座標</param>
    /// <param name="endMapPosition">最前 or 最奥側のマップ端座標</param>
    /// <returns></returns>
    private bool IsEndOfMapOutOfCamera(float rearFurthestCameraPositionZ, float frontFurthestCameraPositionZ, Vector3 endMapPosition)
    {
        return endMapPosition.z < rearFurthestCameraPositionZ || frontFurthestCameraPositionZ < endMapPosition.z ? true : false;
    }

    /// <summary>
    /// 非アクティブなマップのリストからランダムに1つ選択しリターンする（同時にアクティブ・非アクティブリストに追加）
    /// </summary>
    /// <returns></returns>
    private Transform GetAndRemoveMapTransformFromInactiveChildren()
    {
        var selectedIndex = UnityEngine.Random.Range(0, inactiveChildrenMapTransform.Count);

        var selectedTransform = inactiveChildrenMapTransform[selectedIndex];

        activeChildrenMapTransform.Add(selectedTransform);
        inactiveChildrenMapTransform.RemoveAt(selectedIndex);

        return selectedTransform;
    }
}