using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System;
using UniRx;

/* 投擲攻撃の呼び出しに必要な構造体 */
public class BallThrowParam
{
    public float BallSize { get; private set; }
    public float BallSpeedPerSec { get; private set; }
    public int BallThrowingPreparationDuration { get; private set; }
    public int BallThrowingDuration { get; private set; }
    public float PreparationDecelerationMagnification { get; private set; }
    public float ThrowingDecelerationMagnification { get; private set; }

    public BallThrowParam(BallThrowParamRange ballThrowParamRange)
    {
        this.BallSize = UnityEngine.Random.Range(ballThrowParamRange.MinBallSize, ballThrowParamRange.MaxBallSize);
        this.BallSpeedPerSec = UnityEngine.Random.Range(ballThrowParamRange.MinBallSpeedPerSec, ballThrowParamRange.MaxBallSpeedPerSec);
        this.BallThrowingPreparationDuration = UnityEngine.Random.Range(ballThrowParamRange.MinBallThrowingPreparationDuration, ballThrowParamRange.MaxBallThrowingPreparationDuration);
        this.BallThrowingDuration = UnityEngine.Random.Range(ballThrowParamRange.MinBallThrowingDuration, ballThrowParamRange.MaxBallThrowingDuration);
        this.PreparationDecelerationMagnification = UnityEngine.Random.Range(ballThrowParamRange.MinPreparationDecelerationMagnification, ballThrowParamRange.MaxPreparationDecelerationMagnification);
        this.ThrowingDecelerationMagnification = UnityEngine.Random.Range(ballThrowParamRange.MinThrowingDecelerationMagnification, ballThrowParamRange.MaxThrowingDecelerationMagnification);
    }
}

/* 投擲攻撃クラスの初期化時に必要な構造体．投擲攻撃時に以下変数の範囲で乱数を生成するメソッドを作る． */
public class BallThrowParamRange
{
    private float minBallSize;
    private float maxBallSize;
    private float minBallSpeedPerSec;
    private float maxBallSpeedPerSec;
    private int minBallThrowingPreparationDuration;
    private int maxBallThrowingPreparationDuration;
    private int minBallThrowingDuration;
    private int maxBallThrowingDuration;
    private float minPreparationDecelerationMagnification;
    private float maxPreparationDecelerationMagnification;
    private float minThrowingDecelerationMagnification;
    private float maxThrowingDecelerationMagnification;

    public float MinBallSize { get => minBallSize; private set => minBallSize = Math.Max(0f, value); }
    public float MaxBallSize { get => maxBallSize; private set => maxBallSize = Math.Max(MinBallSize, value); }
    public float MinBallSpeedPerSec { get => minBallSpeedPerSec; private set => minBallSpeedPerSec = Math.Max(0f, value); }
    public float MaxBallSpeedPerSec { get => maxBallSpeedPerSec; private set => maxBallSpeedPerSec = Math.Max(MinBallSpeedPerSec, value); }
    public int MinBallThrowingPreparationDuration { get => minBallThrowingPreparationDuration; private set => minBallThrowingPreparationDuration = Math.Max(0, value); }
    public int MaxBallThrowingPreparationDuration { get => maxBallThrowingPreparationDuration; private set => maxBallThrowingPreparationDuration =  Math.Max(MinBallThrowingPreparationDuration, value); }
    public int MinBallThrowingDuration { get => minBallThrowingDuration; private set => minBallThrowingDuration = Math.Max(0, value); }
    public int MaxBallThrowingDuration { get => maxBallThrowingDuration; private set => maxBallThrowingDuration = Math.Max(MinBallThrowingDuration, value); }
    public float MinPreparationDecelerationMagnification { get => minPreparationDecelerationMagnification; private set => minPreparationDecelerationMagnification = Math.Max(0, value); }
    public float MaxPreparationDecelerationMagnification { get => maxPreparationDecelerationMagnification; private set => maxPreparationDecelerationMagnification = Math.Max(MinPreparationDecelerationMagnification, value); }
    public float MinThrowingDecelerationMagnification { get => minThrowingDecelerationMagnification; private set => minThrowingDecelerationMagnification = Math.Max(0, value); }
    public float MaxThrowingDecelerationMagnification { get => maxThrowingDecelerationMagnification; private set => maxThrowingDecelerationMagnification = Math.Max(MinThrowingDecelerationMagnification, value); }

    public BallThrowParamRange(float MinBallSize, float MaxBallSize, float MinBallSpeedPerSec, float MaxBallSpeedPerSec, int MinBallThrowingPreparationDuration,
        int MaxBallThrowingPreparationDuration, int MinBallThrowingDuration, int MaxBallThrowingDuration, float MinPreparationDecelerationMagnification,
        float MaxPreparationDecelerationMagnification, float MinThrowingDecelerationMagnification, float MaxThrowingDecelerationMagnification)
    {
        this.MinBallSize = MinBallSize;
        this.MaxBallSize = MaxBallSize;
        this.MinBallSpeedPerSec = MinBallSpeedPerSec;
        this.MaxBallSpeedPerSec = MaxBallSpeedPerSec;
        this.MinBallThrowingPreparationDuration = MinBallThrowingPreparationDuration;
        this.MaxBallThrowingPreparationDuration = MaxBallThrowingPreparationDuration;
        this.MinBallThrowingDuration = MinBallThrowingDuration;
        this.MaxBallThrowingDuration = MaxBallThrowingDuration;
        this.MinPreparationDecelerationMagnification = MinPreparationDecelerationMagnification;
        this.MaxPreparationDecelerationMagnification = MaxPreparationDecelerationMagnification;
        this.MinThrowingDecelerationMagnification = MinThrowingDecelerationMagnification;
        this.MaxThrowingDecelerationMagnification = MaxThrowingDecelerationMagnification;
    }
}

public class GrandmaBallThrower : MonoBehaviour
{
    /* 投擲攻撃のフェーズ */
    public enum BallThrowPhase
    {
        OutOfPeriod,
        Preparation,
        Throwing,
    }

    //public BallThrowPhase ballThrowPhase;
    private ReactiveProperty<BallThrowPhase> _ballThrowPhase;
    public BallThrowPhase ballThrowPhase { get => _ballThrowPhase.Value; private set => _ballThrowPhase.Value = value; }
    public IReadOnlyReactiveProperty<BallThrowPhase> ballThrowPhaseChanged => _ballThrowPhase;

    private BallThrowParamRange ballThrowParamRange;

    private float preparationDecelerationMagnification;
    private float PreparationDecelerationMagnification { get => preparationDecelerationMagnification; set => preparationDecelerationMagnification = Math.Max(0f, Math.Min(1f, value)); }

    private float throwingDecelerationMagnification;
    private float ThrowingDecelerationMagnification { get => throwingDecelerationMagnification; set => throwingDecelerationMagnification = Math.Max(0f, Math.Min(1f, value)); }

    private Vector3 moveVector;

    private Dictionary<BallThrowPhase, Action> moveActionDic;

    [SerializeField] private GameObject mudBall; //Editorから設定

    [SerializeField] private GrandmaStraightMover grandmaStraightMover;
    [SerializeField] private GrandmaSideMover grandmaSideMover;

    public BallThrowParam GenerateBallThrowParam()
    {
        return new BallThrowParam(this.ballThrowParamRange);
    }

    /* ステータスに応じて移動ベクトルを求める */
    public Vector3 GetBallThrowMoveVector()
    {
        moveActionDic[ballThrowPhase]();

        return moveVector;
    }

    /* インスタンスの初期化 */
    public void Init(BallThrowParamRange ballThrowParamRange)
    {
        // StartThrowingで処理
        //PreparationDecelerationMagnification = preparationDecelerationMagnification;
        //ThrowingDecelerationMagnification = throwingDecelerationMagnification;

        UnityEngine.Random.InitState(DateTime.Now.Millisecond);

        this.ballThrowParamRange = ballThrowParamRange;

        moveVector = Vector3.zero;

        _ballThrowPhase = new ReactiveProperty<BallThrowPhase>(BallThrowPhase.OutOfPeriod);

        moveActionDic = new Dictionary<BallThrowPhase, Action>();
        moveActionDic.Add(BallThrowPhase.Preparation, GetThrowingPreparationMoveVector);
        moveActionDic.Add(BallThrowPhase.Throwing, GetThrowingMoveVector);

        if (!TryGetComponent(out grandmaStraightMover)) Debug.Log("Missing IEnemyStraightMover");
        if (!TryGetComponent(out grandmaSideMover)) Debug.Log("Missing IEnemySideMover");
    }

    /* 投擲終了，キャンセル時のリセット処理 */
    public void Reset()
    {
        moveVector = Vector3.zero;

        ballThrowPhase = BallThrowPhase.OutOfPeriod;

        /* ラッシュ前後の時間で移動量を計算するため，初期化処理 */
        GetComponent<GrandmaStraightMover>().Reset();
        GetComponent<GrandmaSideMover>().Reset();
    }

    /* 投擲準備中の移動ベクトル生成．並進移動量に減速倍率を乗算，横移動量は通常通り． */
    private void GetThrowingPreparationMoveVector()
    {
        moveVector = grandmaStraightMover.GetStraightMoveVector();

        moveVector *= PreparationDecelerationMagnification;

        moveVector += grandmaSideMover.GetSideMoveVector();
    }

    /* 投擲中の移動ベクトル生成．並進移動量に減速倍率（準備中の倍率とは別の値）を乗算，横移動量は通常通り． */
    private void GetThrowingMoveVector()
    {
        moveVector = grandmaStraightMover.GetStraightMoveVector();

        moveVector *= ThrowingDecelerationMagnification;

        moveVector += grandmaSideMover.GetSideMoveVector();
    }

    /// <summary>
    /// 投擲開始から終了までの攻撃ステータスを制御する処理．投擲開始時に一度だけコール．
    /// </summary>
    /// <param name="ballSize">投擲する泥団子のサイズ．</param>
    /// <param name="ballSpeedPerSec">移動量/秒</param>
    /// <param name="ballThrowingPreparationDuration">投擲準備の待機時間</param>
    /// <param name="ballThrowingDuration">投擲の時間</param>
    /// <param name="token">CancelationToken</param>
    //public async Task StartBallThrowing(float ballSize, float ballSpeedPerSec, int ballThrowingPreparationDuration, int ballThrowingDuration, CancellationToken token)
    public async Task StartBallThrowing(BallThrowParam ballThrowParam, CancellationToken token)
    {
        if (ballThrowPhase != BallThrowPhase.OutOfPeriod) return;

        PreparationDecelerationMagnification = ballThrowParam.PreparationDecelerationMagnification;
        ThrowingDecelerationMagnification = ballThrowParam.ThrowingDecelerationMagnification;

        await ThrowBallPreparation(ballThrowParam.BallThrowingPreparationDuration, token);

        await ThrowBall(ballThrowParam.BallSize, ballThrowParam.BallSpeedPerSec, ballThrowParam.BallThrowingDuration,token);
    }

    /* 投擲準備中の開始から終了のステータス制御 */
    private async Task ThrowBallPreparation(int ballThrowingPreparationDuration, CancellationToken token)
    {
        ballThrowPhase = BallThrowPhase.Preparation;

        await Task.Delay(ballThrowingPreparationDuration, token);
    }

    /* 投擲中の開始から終了のステータス制御 */
    private async Task ThrowBall(float ballSize, float ballSpeedPerSec, int ballThrowingDuration, CancellationToken token)
    {
        if (ballThrowPhase != BallThrowPhase.Preparation) return;

        ballThrowPhase = BallThrowPhase.Throwing;

        /* Ball生成処理．AttackManagerで乱数生成してサイズとスピード調整が必要 */
        //Debug.Log(ballSize + ballSpeedPerSec);
        CreateBall(ballSize, ballSpeedPerSec);

        /* ballThrowingDuration秒後，通常移動へ遷移 */
        await Task.Delay(ballThrowingDuration, token);

        Reset();
    }

    /* 泥団子生成 */
    private void CreateBall(float ballSize, float ballSpeedPerSec)
    {
        var ballGenerationPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z + 0.5f);

        var ballPrefab = Instantiate(mudBall, ballGenerationPosition, Quaternion.identity);

        BallController ballController;

        if (ballPrefab.TryGetComponent(out ballController))
        {
            ballController.Init(ballSize, ballSpeedPerSec);
        }
    }
}
