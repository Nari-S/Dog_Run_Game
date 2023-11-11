using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

interface IEnemyHungerManager
{
    IReadOnlyReactiveProperty<float> Hunger { get; }

    float MinHunger { get; } // 腹減り度合いの最小値
    float MaxHunger { get; } // 腹減り度合いの最大値
}
