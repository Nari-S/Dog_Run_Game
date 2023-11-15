using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class HungerManager : MonoBehaviour, IHungerManager
{
    private FloatReactiveProperty hunger; // 腹減り度合い minHunger ~ maxHunger
    public IReadOnlyReactiveProperty<float> Hunger => hunger;
    //public float HungerProperty { get => hunger.Value; set => hunger.Value = value; }

    /*private ReactiveProperty<float> hungerUpdateMount;
    public float HungerUpdateMount { get => hungerUpdateMount.Value; set => hungerUpdateMount.Value = value; }
    public IReadOnlyReactiveProperty<float> hungerUpdateMountChanged => hungerUpdateMount;*/

    public float MinHunger { get; private set; } // 腹減り度合いの最小値
    public float MaxHunger { get; private set; } // 腹減り度合いの最大値
    [SerializeField] private float periodChangeAmount; // 定期的な腹減り度合いの更新量
    [SerializeField] private float stepConsumedHunger; // ステップ時に消費される腹減り度合い

    [SerializeField] GameStatusManager gameStatusManager;

    /// <summary>
    /// デバッグ用．常に腹減り度を100とする
    /// </summary>
    /*private void Update()
    {
        hunger.Value = MaxHunger;
    }*/

    private void Awake()
    {
        MinHunger = 0;
        MaxHunger = 100;
        periodChangeAmount = 2;
        stepConsumedHunger = 15f;

        hunger = new FloatReactiveProperty(70); // 腹減り度合いの初期値設定
        //hungerUpdateMount = new ReactiveProperty<float>(0);

        this.UpdateAsObservable().Where(_ => gameStatusManager.gameStatus == GameStatusManager.GameStatus.Game).ThrottleFirst(TimeSpan.FromSeconds(1)).Subscribe(_ =>
        {
            UpdateHunger(-periodChangeAmount);
        })
        .AddTo(this);

        //hungerUpdateMountChanged.Subscribe(x => UpdateHunger(x)).AddTo(this);
    }

    public void UpdateHunger(float changeAmount)
    {
        if ((hunger.Value + changeAmount) < MinHunger) hunger.Value = MinHunger;
        else if ((hunger.Value + changeAmount) > MaxHunger) hunger.Value = MaxHunger;
        else hunger.Value += changeAmount;
    }

    public bool canStep()
    {
        return hunger.Value >= stepConsumedHunger;
    }

    public void UpdateHungerByStep()
    {
        UpdateHunger(-stepConsumedHunger);
    }

    public float GetMaxHunger()
    {
        return MaxHunger;
    }
}
