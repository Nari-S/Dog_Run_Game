using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UniRx;

public class GrandmaStaggerMover : MonoBehaviour
{
    private float defaultStaggerDuration;
    private float additionalStaggerDuration;

    /* 怯みのフェーズ */
    public enum StaggerPhase
    {
        OutOfPeriod,
        Stagger
    }

    private ReactiveProperty<StaggerPhase> _staggerPhase;
    public StaggerPhase staggerPhase { get => _staggerPhase.Value; private set => _staggerPhase.Value = value; }
    public IReadOnlyReactiveProperty<StaggerPhase> staggerPhaseChanged => _staggerPhase;

    private MoveVectorRevisorIntoDisplay moveVectorRevisor;

    /// <summary>
    /// 怯み中の移動ベクトルを求める．画面外に出なければその場で静止，画面外に出そうであれば画面内に修正．
    /// </summary>
    /// <returns></returns>
    public Vector3 GetStaggerMoveVector()
    {
        return moveVectorRevisor.ReviseMoveVectorIntoDisplay(transform.position, Vector3.zero);
    }

    /* インスタンスの初期化 */
    public void Init()
    {
        if (!TryGetComponent(out moveVectorRevisor)) Debug.Log("Missing MoveVectorRevisorIntoDisplay");

        _staggerPhase = new ReactiveProperty<StaggerPhase>(StaggerPhase.OutOfPeriod);

        defaultStaggerDuration = 1.5f;
        additionalStaggerDuration = 3f;
    }

    /* 怯み終了，キャンセル時のリセット処理 */
    public void Reset()
    {
        staggerPhase = StaggerPhase.OutOfPeriod;
    }


    public async Task StartStagger(int staggerDuration, CancellationToken token)
    {
        if (staggerPhase != StaggerPhase.OutOfPeriod) return;
        staggerPhase = StaggerPhase.Stagger;

        await Task.Delay(staggerDuration, token); 

        Reset();
    }

    public async Task StartStagger(float normalizedInversionDistancePeeToPlayer, CancellationToken token)
    {
        if (staggerPhase != StaggerPhase.OutOfPeriod) return;
        staggerPhase = StaggerPhase.Stagger;

        await Task.Delay(ConvertNormalizedDistanceToStaggerDuration(normalizedInversionDistancePeeToPlayer), token);

        /* ラッシュ前後の時間で移動量を計算するため，初期化処理 */
        GetComponent<GrandmaStraightMover>().Reset();
        GetComponent<GrandmaSideMover>().Reset();

        Reset();
    }

    /// <summary>
    /// ションベンオブジェクトから敵までの正規化距離を怯み時間に変換．Task.Delayで怯み時間の間は停止するため，1000倍で距離をmsに変換
    /// </summary>
    /// <param name="distanceToPlayer">ションベンオブジェクトから敵までの距離</param>
    /// <returns></returns>
    public int ConvertNormalizedDistanceToStaggerDuration(float normalizedInversionDistancePeeToPlayer)
    {
        return (int)((defaultStaggerDuration + additionalStaggerDuration * normalizedInversionDistancePeeToPlayer) * 1000);
    }
}
