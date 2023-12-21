using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class ScoreCounter : MonoBehaviour
{
    private IntReactiveProperty _totalScoreChanged; //スコア合計値
    public int totalScore { get => _totalScoreChanged.Value; private set => _totalScoreChanged.Value = value; }
    public IReadOnlyReactiveProperty<int> totalScoreChanged => _totalScoreChanged;

    private Subject<List<int>> _scoreRankUpdated;
    public IObservable<List<int>> scoreRankUpdated => _scoreRankUpdated;

    private int countTriggerDistance = 1; // 走行スコア計算を何メートルごとに行うか
    private int runningScorePerCountTrigger = 1; // カウントトリガーごとのスコア
    private int prevScoreCountPosition; // 前回スコア合計値に反映した走行スコア
    private int maxScorePeeHitting = 100; // ションベンをぶつけたときの最大スコア

    [SerializeField] private PeeCreator peeCreator;
    [SerializeField] Transform playerTransform;

    private GameStatusManager gameStatusManager;
    private ScoreRankIO scoreRankIO;

    private void Awake()
    {
        _totalScoreChanged = new IntReactiveProperty(0);
        _scoreRankUpdated = new Subject<List<int>>();
    }

    private void Start()
    {
        if (!TryGetComponent(out gameStatusManager)) Debug.Log("GameStatusManager is not attached to this object.");
        if (!TryGetComponent(out scoreRankIO)) Debug.Log("ScoreRankIO is not attached to this object.");

        /* ションベンスコア更新．ションベン生成クラスからションベン生成通知を受領 */
        peeCreator.OnPeeCreated.Subscribe(x =>
        {
            /* ションベンオブジェクトとプレイヤーとの当たり判定通知を受領．スコアに反映． */
            x.OnPeeHit.Subscribe(y => totalScore += (int)(maxScorePeeHitting * y)).AddTo(x);
        })
        .AddTo(this);

        /* 走行スコア更新．前回のスコア更新した位置より一定距離進んだときにスコア更新． */
        this.UpdateAsObservable().Where(_ => gameStatusManager.gameStatus == GameStatusManager.GameStatus.Game || gameStatusManager.gameStatus == GameStatusManager.GameStatus.TitleToGame)
            .Where(_ => (int)playerTransform.position.z - prevScoreCountPosition >= countTriggerDistance)
            .Subscribe(_ => {
                totalScore += ((int)playerTransform.position.z - prevScoreCountPosition) * runningScorePerCountTrigger;
                prevScoreCountPosition = (int)playerTransform.position.z;
            }).AddTo(this);

        /* ゲーム本編→スコア時，スコアをニフクラにアップロード．
         * その後，ニフクラより降順スコアの上位TOP5を取得して，IObservableを介して通知*/
        gameStatusManager.OnGameStatusChanged.Where(x => x == GameStatusManager.GameStatus.GameToScore).Subscribe(async _ => 
        {
            try
            {
                await scoreRankIO.UploadScore(totalScore); // スコアをニフクラへアップロード
                Debug.Log("Score Uploading is success.");
            }
            catch (NCMB.NCMBException e)
            {
                _scoreRankUpdated.OnNext(new List<int>()); // アップロード例外発生時，nullのListの通知によりアップロード失敗としてイベント発行する
                Debug.Log("Score Uploading is missing. " + e.ErrorMessage);
                return;
            }

            try
            {
                var scoreRanking = await scoreRankIO.DownloadTopScore(5); // スコアをニフクラよりダウンロード
                _scoreRankUpdated.OnNext(scoreRanking); // ダウンロードしたスコアをイベント発行と同時に通知
                Debug.Log("Score Downloading is success.");
            }
            catch (NCMB.NCMBException e)
            {
                _scoreRankUpdated.OnNext(new List<int>()); // ダウンロード例外発生時，nullのListの通知によりアップロード失敗としてイベント発行する
                Debug.Log("Score Downloading is missing. " + e.ErrorMessage);
            }
        }).AddTo(this);
    }
}
