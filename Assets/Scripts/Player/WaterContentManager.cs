using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class WaterContentManager : MonoBehaviour, IWaterContentManager
{
    private FloatReactiveProperty _waterContent; // 水分量 minWaterContent ~ maxWaterContent
    public IReadOnlyReactiveProperty<float> waterCotendChanged => _waterContent;
    public float waterContent { get => _waterContent.Value; private set => _waterContent.Value = value; }

    public float MinWaterContent { get; private set; } // 水分量の最小値
    public float MaxWaterContent { get; private set; } // 水分量の最大値

    private void Awake()
    {
        MinWaterContent = 0;
        MaxWaterContent = 100;

        _waterContent = new FloatReactiveProperty(70); // 水分量の初期値設定
    }

    /// <summary>
    /// 水分量を更新するメソッド
    /// </summary>
    /// <param name="changeAmount">水分量の更新量</param>
    public void UpdateWaterContent(float changeAmount)
    {
        waterContent = Mathf.Max(Mathf.Min(MaxWaterContent, waterContent + changeAmount), MinWaterContent);
    }
}
