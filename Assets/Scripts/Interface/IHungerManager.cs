using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

interface IHungerManager
{
    IReadOnlyReactiveProperty<float> Hunger { get; } //腹減り度合い

    //float HungerUpdateMount { get; set; } //腹減り度合いの更新量
    //IReadOnlyReactiveProperty<float> hungerUpdateMountChanged { get; }

    float MinHunger { get; } // 腹減り度合いの最小値
    float MaxHunger { get; } // 腹減り度合いの最大値

    void UpdateHunger(float changeAmount);
}
