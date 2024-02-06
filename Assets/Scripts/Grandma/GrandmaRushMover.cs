using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading;
using System.Threading.Tasks;
using System;
using UniRx;
using Cysharp.Threading.Tasks;

public class GrandmaRushMover : MonoBehaviour
{
    /* 突進攻撃のフェーズ */
    public enum RushPhase
    {
        OutOfRange, // 
        Preparation,
        Rushing,
        Cooldown,
    }

    private float keepDistanceToPlayer; // 突進チャージ中にプレイヤーと維持する距離

    private Vector3 nowPosition;
    private Vector3 prevPosition;
    private Vector3 moveVector;

    [SerializeField] private GameObject player;

    private IStraightMover playerStraightMover;

    [SerializeField] private GrandmaStraightMover grandmaStraightMover;
    [SerializeField] private GrandmaSideMover grandmaSideMover;
    [SerializeField] private GrandmaHungerManager grandmaHungerManager;

    //public RushPhase rushPhase { get; private set; }
    private ReactiveProperty<RushPhase> _rushPhase;
    public RushPhase rushPhase { get => _rushPhase.Value; private set => _rushPhase.Value = value; }
    public IReadOnlyReactiveProperty<RushPhase> rushPhaseChanged => _rushPhase;

    [SerializeField] private int rushPreparationDuration;
    [SerializeField] private int coolDownDuration;
    [SerializeField] private float cooldownMoveMagnification;

    private Dictionary<RushPhase, Action> moveActionDic;

    public void Init(float keepDistanceToPlayer, int rushPreparationDuration, int cooldownDuration, float cooldownMoveMagnification)
    {
        this.keepDistanceToPlayer = keepDistanceToPlayer;
        this.rushPreparationDuration = rushPreparationDuration;
        this.coolDownDuration = cooldownDuration;
        this.cooldownMoveMagnification = cooldownMoveMagnification;

        //rushPhase = RushPhase.OutOfRange;
        _rushPhase = new ReactiveProperty<RushPhase>(RushPhase.OutOfRange);

        playerStraightMover = player.GetComponent<IStraightMover>();

        grandmaStraightMover = GetComponent<GrandmaStraightMover>();
        grandmaSideMover = GetComponent<GrandmaSideMover>();
        grandmaHungerManager = GetComponent<GrandmaHungerManager>();

        /* rushPhaseごとに呼び出すメソッドを設定 */
        moveActionDic = new Dictionary<RushPhase, Action>();
        moveActionDic.Add(RushPhase.Preparation, GetPreparationRushMoveVector);
        moveActionDic.Add(RushPhase.Rushing, GetRushingMoveVector);
        moveActionDic.Add(RushPhase.Cooldown, GetCooldownRushMoveVector);
    }

    public void Reset()
    {
        moveVector = Vector3.zero;

        rushPhase = RushPhase.OutOfRange;
    }

    /* rushPhaseがOutOfRange以外のときのラッシュ移動の制御を行う */
    public Vector3 GetRushMoveVector()
    {
        moveActionDic[rushPhase]();

        return moveVector;
    }

    /* ラッシュ準備中の移動．ラッシュ開始までプレイヤーとの距離を固定する */
    private void GetPreparationRushMoveVector()
    {
        moveVector = grandmaStraightMover.GetStraightMoveVector();
        moveVector += grandmaSideMover.GetSideMoveVector();

        /* 通常移動で維持する距離以内に入らないようにする */
        if (player.transform.position.z - (transform.position.z + moveVector.z) < keepDistanceToPlayer)
        {
            moveVector.z = player.transform.position.z - keepDistanceToPlayer - transform.position.z;
        }

        return;
    }

    /* ラッシュ中の移動．DoTweenでmoveVectorは更新されているため，ここでは何も行わない...orz */
    private void GetRushingMoveVector()
    {
        return;
    }

    /* ラッシュクールダウン中の移動．通常移動に1.0以下の倍率を乗算して減速 */
    private void GetCooldownRushMoveVector()
    {
        moveVector = grandmaStraightMover.GetStraightMoveVector();

        moveVector.z *= cooldownMoveMagnification;
    }

    public async UniTask StartRushing(CancellationToken token)
    {
        if (rushPhase != RushPhase.OutOfRange) return;

        await RushPreparation(token);

        await Rushing(token);
    }

    private async UniTask RushPreparation(CancellationToken token)
    {
        rushPhase = RushPhase.Preparation;

        /* rushPreparationDuration秒後，ラッシュ開始 */
        //await Task.Delay(rushPreparationDuration, token);
        await UniTask.Delay(rushPreparationDuration, cancellationToken: token);

        rushPhase = RushPhase.Rushing;
    }

    /* 突進開始．突進チャージ時間終了後に呼ぶ． */
    private async UniTask Rushing(CancellationToken token) 
    {
        if (rushPhase != RushPhase.Rushing) return;

        nowPosition = new Vector3(0, 0, transform.position.z);
        prevPosition = new Vector3(0, 0, transform.position.z);

        DOTween.To(
            () => nowPosition,
            pos =>
            {
                prevPosition = nowPosition;
                nowPosition = pos;
                moveVector = nowPosition - prevPosition;
            },
            nowPosition + new Vector3(0, 0, GetAverageRushSpeed() * GetAttackDuration()),
            GetAttackDuration()
        )
        .SetEase(Ease.OutQuad)
        .SetUpdate(UpdateType.Normal)
        .OnStart(() =>
        {
            //dogAnimationManager.StartAscending();

            rushPhase = RushPhase.Rushing;
        }
        )
        .OnComplete(async () =>
        {
            /*IsAscending = false;
            IsDescending = true;

            StartDescendingJump();*/

            /* ラッシュ前後の時間で移動量を計算するため，初期化処理 */
            GetComponent<GrandmaStraightMover>().Reset();
            GetComponent<GrandmaSideMover>().Reset();

            /* ラッシュ終了後は，coolDownDurationの間，クールダウン状態 */
            rushPhase = RushPhase.Cooldown;
            //await Task.Delay(coolDownDuration, token);

            try
            {
                //await Task.Delay(coolDownDuration, token);
                await UniTask.Delay(coolDownDuration, cancellationToken: token);
            }
            catch (Exception ex) when (ex is OperationCanceledException)
            {
                return;
            }

            Reset(); // ラッシュ終了時の初期化
        });
    }

    /* 敵が柴犬を追い越して突進開始前時点でのお互いの距離分，柴犬の前に出る間隔を求める */
    private float GetAttackDuration()
    {
        return keepDistanceToPlayer / (GetAverageRushSpeed() - playerStraightMover.StraightMoveSpeed) * 2;
    }

    /* 突進を通しての平均速度Vaveを決定
     * プレイヤーの最大速度Vmaxとすると，Vaveの最低値は(Vmax + 0.5)，最大値は(Vmax * 1.5 + 0.5) である．検証後，要調整？*/
    private float GetAverageRushSpeed()
    {
        return playerStraightMover.MaxSpeed * (grandmaHungerManager.Hunger.Value / 2 / grandmaHungerManager.MaxHunger + 1) + 0.5f;
    }
}
