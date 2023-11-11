using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class GrandmaHungerManager : MonoBehaviour, IEnemyHungerManager, IPeeReceivable
{
    private FloatReactiveProperty hunger; // 腹減り度合い minHunger ~ maxHunger
    public IReadOnlyReactiveProperty<float> Hunger => hunger;

    public float MinHunger { get; private set; } // 腹減り度合いの最小値
    public float MaxHunger { get; private set; } // 腹減り度合いの最大値
    [SerializeField] private float initialHunger; // 腹減り度の初期値となる最大値に対する割合

    [SerializeField] private float peeBaseDamage;
    [SerializeField] private float peeMaxAdditionalDamage;

    public void AffectPee(float normalizedInversionDistanceToEnemy)
    {
        UpdateHunger(-(peeBaseDamage + peeMaxAdditionalDamage * normalizedInversionDistanceToEnemy));
    }

    private void Awake()
    {
        MinHunger = 0;
        MaxHunger = 100;
        initialHunger = 70f;

        peeBaseDamage = 30f;
        peeMaxAdditionalDamage = 40f;

        hunger = new FloatReactiveProperty(initialHunger); // 腹減り度合いの初期値設定
    }

    public void UpdateHunger(float changeAmount)
    {
        if ((hunger.Value + changeAmount) < MinHunger) hunger.Value = MinHunger;
        else if ((hunger.Value + changeAmount) > MaxHunger) hunger.Value = MaxHunger;
        else hunger.Value += changeAmount;

        Debug.Log("grandmaHungerChangeAmount: " + changeAmount + ", afterChangedHunger: " + hunger.Value);
    }
}
