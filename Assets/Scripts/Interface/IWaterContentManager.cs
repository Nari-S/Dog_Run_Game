using UniRx;

interface IWaterContentManager
{
    IReadOnlyReactiveProperty<float> waterCotendChanged { get; }
    float waterContent { get; }
    float MinWaterContent { get; } // 水分量の最小値
    float MaxWaterContent { get; } // 水分量の最大値
    void UpdateWaterContent(float changeAmount);
}
