using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class GUIPresenter : MonoBehaviour
{
    [SerializeField] HungerManager hungerManager;
    [SerializeField] HpGaugeView hpGaugeView;
    [SerializeField] HpTextView hpTextView;

    [SerializeField] WaterContentManager waterContentManager;
    [SerializeField] WHpGaugeView whpGaugeView;
    [SerializeField] WHpTextView whpTextView;

    [SerializeField] ScoreCounter scoreCounter;
    [SerializeField] ScoreTextView scoreTextView;

    /* HungerとwaterContendChanged がAwakeにて初期化されるため，Startで呼ぶ */
    void Start()
    {
        /* 腹減り度ゲージのPresenter */
        hungerManager.Hunger
            .Subscribe(x =>
            {
                hpGaugeView.SetHpGauge(x / hungerManager.GetMaxHunger());
                hpTextView.SetHpText(x);
            })
            .AddTo(this);

        /* 水分量ゲージのPresenter */
        waterContentManager.waterCotendChanged
            .Subscribe(x =>
            {
                whpGaugeView.SetWHpGauge(x / waterContentManager.MaxWaterContent);
                whpTextView.SetHpText(x);
            })
            .AddTo(this);

        /* スコアのPresenter */
        scoreCounter.totalScoreChanged.Subscribe(x => scoreTextView.SetScoreText(x)).AddTo(this);
    }
}
