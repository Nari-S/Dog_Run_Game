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

    private int countTriggerDistance = 1; // 走行スコア計算を何メートルごとに行うか
    private int runningScorePerCountTrigger = 1; // カウントトリガーごとのスコア
    private int prevScoreCountPosition; // 前回スコア合計値に反映した走行スコア
    private int maxScorePeeHitting = 100; // ションベンをぶつけたときの最大スコア

    [SerializeField] private PeeCreator peeCreator;
    [SerializeField] Transform playerTransform;

    /* peeCreatorのOnPeeCreatedがAwakeで呼ばれているためStartで購読 */
    private void Start()
    {
        _totalScoreChanged = new IntReactiveProperty(0);

        /* ションベンスコア更新．ションベン生成クラスからションベン生成通知を受領 */
        peeCreator.OnPeeCreated.Subscribe(x =>
        {
            /* ションベンオブジェクトとプレイヤーとの当たり判定通知を受領．スコアに反映． */
            x.OnPeeHit.Subscribe(y => totalScore += (int)(maxScorePeeHitting * y)).AddTo(x);
        })
        .AddTo(this);

        /* 走行スコア更新．前回のスコア更新した位置より一定距離進んだときにスコア更新． */
        this.UpdateAsObservable().Where(_ => (int)playerTransform.position.z - prevScoreCountPosition >= countTriggerDistance).Subscribe(_ => {
            totalScore += ((int)playerTransform.position.z - prevScoreCountPosition) * runningScorePerCountTrigger;
            prevScoreCountPosition = (int)playerTransform.position.z;
        }).AddTo(this);
    }
}
