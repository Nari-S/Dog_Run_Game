using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UniRx;

public class GrandmaAttackManager : MonoBehaviour, IPeeReceivable
{
    private enum AttackStatus
    {
        None,
        Rushing,
        ThrowingBall,
        Stagger
    }

    private AttackStatus attackStatus;

    public float DistanceToStartRushing { get; private set; } // 通常移動状態から突進準備状態へ移行する距離
    public float DistanceToCancelRushing { get; private set; } // 突進準備状態から通常移動状態へ移行する距離

    private float attackCountOriginTime; // 投擲攻撃のカウント起点時間
    private float throwingCooldownDuration; // 投擲攻撃のクールダウン時間
    private float minThrowingCooldownDuration;
    private float maxThrowingCooldownDuration;

    private GrandmaRushMover grandmaRushMover;
    private GrandmaBallThrower grandmaBallThrower;
    private GrandmaStaggerMover grandmaStaggerMover;

    private CancellationTokenSource cancellationTokenSource;
    public CancellationToken Token;

    [SerializeField] private Transform playerTransform;

    private float normalizedInversionDistancePeeToEnemy;

    [SerializeField] private GameStatusManager gameStatusManager;

    /// <summary>
    /// 攻撃ステータスを怯みにする．ションベンとの当たり判定時に呼ばれる
    /// </summary>
    /// <param name="normalizedDistancePeeToEnemy">正規化されたションベンオブジェクトから敵までの距離</param>
    public void AffectPee(float normalizedInversionDistancePeeToEnemy)
    {
        this.normalizedInversionDistancePeeToEnemy = normalizedInversionDistancePeeToEnemy;

        attackStatus = AttackStatus.Stagger;
    }

    private void Awake()
    {
        attackStatus = AttackStatus.None;

        grandmaRushMover = GetComponent<GrandmaRushMover>();
        grandmaBallThrower = GetComponent<GrandmaBallThrower>();
        grandmaStaggerMover = GetComponent<GrandmaStaggerMover>();

        /* 突進攻撃パラメータの初期化．構造体orクラス化する？ */
        DistanceToStartRushing = 3f;
        DistanceToCancelRushing = 4f;
        var rushPreparationDuration = 2000;
        var cooldownDuration = 4000;
        var cooldownMoveMagnification = 0.0f;

        /* 投擲攻撃パラメータの初期化 */
        attackCountOriginTime = Time.time;
        minThrowingCooldownDuration = 2.9f;
        maxThrowingCooldownDuration = 3.0f;
        throwingCooldownDuration = Random.Range(minThrowingCooldownDuration, maxThrowingCooldownDuration); // 投擲攻撃までの時間リセット
        BallThrowParamRange ballThrowParamRange = new BallThrowParamRange(
            MinBallSize: 0.3f, MaxBallSize: 0.7f, MinBallSpeedPerSec: 10f, MaxBallSpeedPerSec: 15f, MinBallThrowingPreparationDuration: 2000,
            MaxBallThrowingPreparationDuration: 2000, MinBallThrowingDuration: 500, MaxBallThrowingDuration: 500, MinPreparationDecelerationMagnification: 0.8f,
            MaxPreparationDecelerationMagnification: 0.8f, MinThrowingDecelerationMagnification: 0.7f, MaxThrowingDecelerationMagnification: 0.7f);

        grandmaRushMover.Init(DistanceToStartRushing, rushPreparationDuration, cooldownDuration, cooldownMoveMagnification); // 突進クラス初期化
        grandmaBallThrower.Init(ballThrowParamRange);
        grandmaStaggerMover.Init();

        /* 非同期処理のキャンセルトークン生成 */
        cancellationTokenSource = new CancellationTokenSource();
        Token = cancellationTokenSource.Token;

        grandmaRushMover.rushPhaseChanged
            .Subscribe(x =>
            {
                if (x != GrandmaRushMover.RushPhase.OutOfRange) return;

                if (attackStatus == AttackStatus.Stagger) ; // 突進終了 or キャンセル後，攻撃ステータスがStaggerであれば維持する（何もしない）
                else attackStatus = AttackStatus.None; // 攻撃ステータスがStaggerでなければ攻撃判定ステータスにする

                attackCountOriginTime = Time.time; // 投擲攻撃までのカウント起点時間リセット
                throwingCooldownDuration = Random.Range(minThrowingCooldownDuration, maxThrowingCooldownDuration); // 投擲攻撃までの時間リセット
            })
            .AddTo(this);

        grandmaBallThrower.ballThrowPhaseChanged
            .Subscribe(x =>
            {
                if (x != GrandmaBallThrower.BallThrowPhase.OutOfPeriod) return;

                if (attackStatus == AttackStatus.Stagger) ; // 投擲終了 or キャンセル後，attackStatusがStaggerであれば維持する（何もしない）
                else if (grandmaRushMover.rushPhase != GrandmaRushMover.RushPhase.OutOfRange) attackStatus = AttackStatus.Rushing; // 投擲終了 or キャンセル後，突進クラスが突進状態であれば，このクラスの攻撃ステータスも突進状態とする
                else attackStatus = AttackStatus.None; // 怯み中でも突進中でもなければ攻撃判定ステータスにする

                attackCountOriginTime = Time.time; // 投擲攻撃までのカウント起点時間リセット
                throwingCooldownDuration = Random.Range(minThrowingCooldownDuration, maxThrowingCooldownDuration); // 投擲攻撃までの時間リセット
            })
            .AddTo(this);

        grandmaStaggerMover.staggerPhaseChanged
            .Subscribe(x =>
            {
                if (x != GrandmaStaggerMover.StaggerPhase.OutOfPeriod) return;

                attackStatus = AttackStatus.None;

                attackCountOriginTime = Time.time; // 投擲攻撃までのカウント起点時間リセット
                throwingCooldownDuration = Random.Range(minThrowingCooldownDuration, maxThrowingCooldownDuration); // 投擲攻撃までの時間リセット
            })
            .AddTo(this);
    }

    private void Start()
    {
        /* ゲーム本編遷移時，投擲攻撃までのカウント起点時間リセット */
        gameStatusManager.OnGameStatusChanged.Where(x => x == GameStatusManager.GameStatus.Game).Subscribe(_ => attackCountOriginTime = Time.time).AddTo(this);
    }

    private void Update()
    {
        //Debug.Log("AttackStatus: " + attackStatus + ", rushPhase: " + grandmaRushMover.rushPhase + ", ballThrowPhase:" + grandmaBallThrower.ballThrowPhase);
        //Debug.Log(grandmaStaggerMover.staggerPhase);

        if (gameStatusManager.gameStatus != GameStatusManager.GameStatus.Game) return; // ゲーム本編以外では，攻撃は行わない

        switch(attackStatus)
        {
            /* 突進と投擲の開始条件判定 */
            case AttackStatus.None:
                if (MeetStartingRequirementForRushing())
                {
                    attackStatus = AttackStatus.Rushing;

                    _ = grandmaRushMover.StartRushing(Token);
                }

                else if (MeetStartingRequirementForThrowing())
                {
                    attackStatus = AttackStatus.ThrowingBall;

                    _ = grandmaBallThrower.StartBallThrowing(grandmaBallThrower.GenerateBallThrowParam(), Token);
                }

                break;

            /* 突進待機中にプレイヤーが突進待機外へ出た場合は突進をキャンセル */
            case AttackStatus.Rushing:
                if(MeetCancellingRequirementForRushing())
                {
                    cancellationTokenSource.Cancel(); // awaitを無効化
                    cancellationTokenSource = new CancellationTokenSource(); //CancellationTokenSourceは再利用できない
                    Token = cancellationTokenSource.Token;

                    attackStatus = AttackStatus.None;

                    grandmaRushMover.Reset();
                }

                break;

            /* 投擲中に突進開始条件を満たせば，投擲をキャンセルして突進を行う */
            case AttackStatus.ThrowingBall:
                if (MeetStartingRequirementForRushing())
                {
                    cancellationTokenSource.Cancel(); // awaitを無効化
                    cancellationTokenSource = new CancellationTokenSource(); //CancellationTokenSourceは再利用できない
                    Token = cancellationTokenSource.Token;

                    grandmaBallThrower.Reset();

                    attackStatus = AttackStatus.Rushing;

                    _ = grandmaRushMover.StartRushing(Token);
                }

                break;

            /* 怯みにより，攻撃をキャンセルし静止する */
            case AttackStatus.Stagger:
                //Debug.Log("AttackStatus is Stagger");

                if (MeetStartingRequirementForStagger())
                {
                    cancellationTokenSource.Cancel(); // awaitを無効化
                    cancellationTokenSource = new CancellationTokenSource(); //CancellationTokenSourceは再利用できない
                    Token = cancellationTokenSource.Token;

                    grandmaRushMover.Reset();
                    grandmaBallThrower.Reset();

                    _ = grandmaStaggerMover.StartStagger(normalizedInversionDistancePeeToEnemy, Token);
                }

                break;
        }
    }

    private bool MeetStartingRequirementForStagger()
    {
        return grandmaStaggerMover.staggerPhase == GrandmaStaggerMover.StaggerPhase.OutOfPeriod ? true : false;
    }

    private bool MeetStartingRequirementForThrowing()
    {
        return !MeetStartingRequirementForRushing() && (grandmaBallThrower.ballThrowPhase == GrandmaBallThrower.BallThrowPhase.OutOfPeriod) && (Time.time - attackCountOriginTime >= throwingCooldownDuration) ? true : false;
    }

    private bool MeetCancellingRequirementForRushing()
    {
        return (grandmaRushMover.rushPhase == GrandmaRushMover.RushPhase.Preparation) && IsInRangeOfCancellingRushing() ? true : false; // 突進準備以外の状態（移動状態と突進中とクールダウン中），または，突進の開始条件を満たしていれば，攻撃をキャンセルできない
    }

    private bool MeetStartingRequirementForRushing()
    {
        return (grandmaRushMover.rushPhase == GrandmaRushMover.RushPhase.OutOfRange) && IsInRangeOfPreparationRushing() ? true : false;
    }

    private bool IsInRangeOfPreparationRushing()
    {
        return (playerTransform.position.z - transform.position.z) < DistanceToStartRushing ? true : false;
    }

    private bool IsInRangeOfCancellingRushing()
    {
        return (playerTransform.position.z - transform.position.z) > DistanceToCancelRushing ? true : false;
    }
}