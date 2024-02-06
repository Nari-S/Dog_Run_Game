using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class GrandmaHungerManager : MonoBehaviour, IPeeReceivable
{
    private FloatReactiveProperty hunger; // 腹減り度合い minHunger ~ maxHunger
    public IReadOnlyReactiveProperty<float> Hunger => hunger;

    public float MinHunger { get; private set; } // 腹減り度合いの最小値
    public float MaxHunger { get; private set; } // 腹減り度合いの最大値
    [SerializeField] private float initialHunger; // 腹減り度の初期値となる最大値に対する割合

    [SerializeField] private float peeBaseDamage;
    [SerializeField] private float peeMaxAdditionalDamage;

    [SerializeField] private IList<float> hungerIncreaseFreqs; // スタミナをspeedUpAmountPerFreqの量だけ増やす頻度のリスト
    [SerializeField] private float hungerIncreaseFreqsChangeFreq; // hungerIncreaseFreqsのインデックスを増やす周期
    [SerializeField] private int speedUpAmountPerFreq; // hungerIncreaseFreqsから選ばれた頻度の度に増やす腹減り度の量

    private float gameStartTime;
    private float nowTime;

    [SerializeField] private GameStatusManager gameStatusManager;

    IDisposable disponsable;

    public void AffectPee(float normalizedInversionDistanceToEnemy)
    {
        UpdateHunger(-(peeBaseDamage + peeMaxAdditionalDamage * normalizedInversionDistanceToEnemy));
    }

    private void Awake()
    {
        MinHunger = 0;
        MaxHunger = 100;
        initialHunger = 50f;

        peeBaseDamage = 30f;
        peeMaxAdditionalDamage = 40f;

        hungerIncreaseFreqs = new List<float>() { 1.4f, 1.2f, 1.0f, 0.8f, 0.6f, 0.4f, 0.2f };
        hungerIncreaseFreqsChangeFreq = 20f;
        speedUpAmountPerFreq = 1;

        hunger = new FloatReactiveProperty(initialHunger); // 腹減り度合いの初期値設定 
    }

    private void Start()
    {
        gameStatusManager.OnGameStatusChanged.Where(x => x == GameStatusManager.GameStatus.Game).Subscribe(_ =>
        {
            gameStartTime = Time.time;

            /* hungerIncreaseFreqsChangeFreqの周期で，hungerIncreaseFreqsのインデックスを進めることで，おばさんの腹減り度上昇の頻度を早める */
            Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(hungerIncreaseFreqsChangeFreq)).Subscribe(__ => { // hungerIncreaseFreqs の周期でイベント発行
                disponsable?.Dispose(); // hungerIncreaseFreqsの周期で腹減り度を回復させていたストリームをキルする

                /* hungerIncreaseFreqsのインデックスを1進めた頻度でイベント通知するストリームを生成し，おばさんの腹減り度を上昇 */
                disponsable = Observable.Interval(TimeSpan.FromSeconds(hungerIncreaseFreqs[Mathf.Clamp((int)((Time.time - gameStartTime) / hungerIncreaseFreqsChangeFreq), 0, hungerIncreaseFreqs.Count - 1)]))
                .Subscribe(___ =>
                {
                    UpdateHunger(speedUpAmountPerFreq);
                    //Debug.Log((hungerIncreaseFreqs[Mathf.Clamp((int)((Time.time - gameStartTime) / hungerIncreaseFreqsChangeFreq), 0, hungerIncreaseFreqs.Count - 1)]));
                });
            }).AddTo(this);

        }).AddTo(this);
    }

    public void UpdateHunger(float changeAmount)
    {
        if ((hunger.Value + changeAmount) < MinHunger) hunger.Value = MinHunger;
        else if ((hunger.Value + changeAmount) > MaxHunger) hunger.Value = MaxHunger;
        else hunger.Value += changeAmount;
    }
}
