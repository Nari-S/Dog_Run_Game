using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using NCMB;

public class GUIPresenter : MonoBehaviour
{
    [SerializeField] HungerManager hungerManager;

    [SerializeField] HpGaugeView hpGaugeView;
    [SerializeField] HpGaugeBackgroundView hpGaugeBackgroundView;
    [SerializeField] HpGaugeFrameView hpGaugeFrameView;
    [SerializeField] HpTextView hpTextView;

    [SerializeField] WaterContentManager waterContentManager;
    [SerializeField] WHpGaugeView whpGaugeView;
    [SerializeField] WHpGaugeBackgroundView whpGaugeBackgroundView;
    [SerializeField] WHpGaugeFrameView whpGaugeFrameView;
    [SerializeField] WHpTextView whpTextView;

    [SerializeField] ScoreCounter scoreCounter;
    [SerializeField] ScoreTextView scoreTextView;
    [SerializeField] ScoreTitleView scoreTitleView;

    [SerializeField] TitleTextView titleTextView;
    [SerializeField] TapToStartTextView tapToStartTextView;

    [SerializeField] ScorePanelView scorePanelView;
    [SerializeField] CapturedTextView capturedTextView;
    [SerializeField] ScoreResultTextView scoreResultTextView;
    [SerializeField] ScoreRankingTextView scoreRankingTextView;
    [SerializeField] TapToRestartTextView tapToRestartTextView;

    [SerializeField] private GameStatusManager gameStatusManager;

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

        /* タイトル→ゲーム本編時，ゲーム本編に必要なUIを全て表示し，タイトルで表示されているUIを全て非表示にする */
        gameStatusManager.OnGameStatusChanged.Where(x => x == GameStatusManager.GameStatus.TitleToGame).Subscribe(_ => 
        {
            /* ゲーム本編のUI表示 */
            hpGaugeBackgroundView.ActivateHpGaugeBackground();
            hpGaugeFrameView.ActivateHpGaugeFrame();
            hpGaugeView.ActivateHpGauge();
            hpTextView.ActivateHpText();

            whpGaugeBackgroundView.ActivateWHpGaugeBackground();
            whpGaugeFrameView.ActivateWHpGaugeFrame();
            whpGaugeView.ActivateWHpGauge();
            whpTextView.ActivateWHpText();

            scoreTextView.ActivateScoreText();
            scoreTitleView.ActivateScoreTitleText();

            /* タイトルのUI非表示 */
            titleTextView.DeactivateTitleText();
            tapToStartTextView.DeactivateTapToStartText();
        })
        .AddTo(this);

        /* スコア画面遷移時，スコア画面のUI表示 */
        gameStatusManager.OnGameStatusChanged.Where(x => x == GameStatusManager.GameStatus.Score).Subscribe(_ =>
        {
            scorePanelView.activateScorePanelImage();
            capturedTextView.ActivateCapturedText();
            scoreResultTextView.ActivateScoreResultText(scoreCounter.totalScore);
            scoreRankingTextView.ActivateScoreRankingText();
            tapToRestartTextView.ActivateTapToRestartText();
        })
        .AddTo(this);

        /* スコアランキングダウンロードの通知をトリガーにして，スコアランキングテキストを更新 */
        scoreCounter.scoreRankUpdated.Subscribe(x => { scoreRankingTextView.SetScoreRanking(x); }).AddTo(this);
    }
}
