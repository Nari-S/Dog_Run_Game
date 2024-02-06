using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;

public class GrandmaAnimationManager : MonoBehaviour
{
    private Animator animator;
    private GrandmaRushMover grandmaRushMover;
    private GrandmaBallThrower grandmaBallThrower;
    private GrandmaStaggerMover grandmaStaggerMover;
    private Dictionary<GrandmaRushMover.RushPhase, Action> rushActions;
    private Dictionary<GrandmaBallThrower.BallThrowPhase, Action> ballThrowActions;
    private Dictionary<GrandmaStaggerMover.StaggerPhase, Action> staggerActions;

    [SerializeField] private GameStatusManager gameStatusManager;

    private void Awake()
    {
        /* Startに移動
        if (!TryGetComponent(out animator)) Debug.Log("Animator is not attached to this object.");
        if (!TryGetComponent(out grandmaRushMover)) Debug.Log("GrandmaRushMover is not attached to this object.");
        if (!TryGetComponent(out grandmaBallThrower)) Debug.Log("GrandmaBallThrower is not attached to this object.");
        if (!TryGetComponent(out grandmaStaggerMover)) Debug.Log("GrandmaStaggerMover is not attached to this object.");

        rushActions = new Dictionary<GrandmaRushMover.RushPhase, Action>() {
            { GrandmaRushMover.RushPhase.OutOfRange,    () => {  animator.SetTrigger("RushEndTrigger"); } },
            { GrandmaRushMover.RushPhase.Preparation,   () => { animator.SetTrigger("RushPreparationTrigger"); animator.ResetTrigger("RushEndTrigger"); } },
            { GrandmaRushMover.RushPhase.Rushing,       () => { animator.SetTrigger("RushStartTrigger"); } },
            { GrandmaRushMover.RushPhase.Cooldown,      () => { animator.SetTrigger("RushCooldownTrigger"); } }
        };

        ballThrowActions = new Dictionary<GrandmaBallThrower.BallThrowPhase, Action>() {
            { GrandmaBallThrower.BallThrowPhase.OutOfPeriod,    () => { ; } },
            { GrandmaBallThrower.BallThrowPhase.Preparation,    async () => { animator.SetFloat("BallThrowingSpeed", 1.0f); animator.SetTrigger("BallThrowingTrigger"); await Task.Delay(300); animator.SetFloat("BallThrowingSpeed", 0f); } },
            { GrandmaBallThrower.BallThrowPhase.Throwing,       () => { animator.SetFloat("BallThrowingSpeed", 1.0f); } }
        };

        staggerActions = new Dictionary<GrandmaStaggerMover.StaggerPhase, Action>()
        {
            { GrandmaStaggerMover.StaggerPhase.OutOfPeriod,     () => {  animator.SetTrigger("StaggerEndTrigger"); } },
            { GrandmaStaggerMover.StaggerPhase.Stagger,         () => { animator.SetTrigger("StaggerStartTrigger"); animator.ResetTrigger("StaggerEndTrigger"); } }
        };
        */
    }

    private void Start()
    {
        if (!TryGetComponent(out animator)) Debug.Log("Animator is not attached to this object.");
        if (!TryGetComponent(out grandmaRushMover)) Debug.Log("GrandmaRushMover is not attached to this object.");
        if (!TryGetComponent(out grandmaBallThrower)) Debug.Log("GrandmaBallThrower is not attached to this object.");
        if (!TryGetComponent(out grandmaStaggerMover)) Debug.Log("GrandmaStaggerMover is not attached to this object.");

        rushActions = new Dictionary<GrandmaRushMover.RushPhase, Action>() {
            { GrandmaRushMover.RushPhase.OutOfRange,    () => {  animator.SetTrigger("RushEndTrigger"); } },
            { GrandmaRushMover.RushPhase.Preparation,   () => { animator.SetTrigger("RushPreparationTrigger"); animator.ResetTrigger("RushEndTrigger"); } },
            { GrandmaRushMover.RushPhase.Rushing,       () => { animator.SetTrigger("RushStartTrigger"); } },
            { GrandmaRushMover.RushPhase.Cooldown,      () => { animator.SetTrigger("RushCooldownTrigger"); } }
        };

        ballThrowActions = new Dictionary<GrandmaBallThrower.BallThrowPhase, Action>() {
            { GrandmaBallThrower.BallThrowPhase.OutOfPeriod,    () => { ; } },
            { GrandmaBallThrower.BallThrowPhase.Preparation,    async () => { animator.SetFloat("BallThrowingSpeed", 1.0f); animator.SetTrigger("BallThrowingTrigger"); /*await Task.Delay(300);*/ await UniTask.Delay(300); animator.SetFloat("BallThrowingSpeed", 0f); } },
            { GrandmaBallThrower.BallThrowPhase.Throwing,       () => { animator.SetFloat("BallThrowingSpeed", 1.0f); } }
        };

        staggerActions = new Dictionary<GrandmaStaggerMover.StaggerPhase, Action>()
        {
            { GrandmaStaggerMover.StaggerPhase.OutOfPeriod,     () => {  animator.SetTrigger("StaggerEndTrigger"); } },
            { GrandmaStaggerMover.StaggerPhase.Stagger,         () => { animator.SetTrigger("StaggerStartTrigger"); animator.ResetTrigger("StaggerEndTrigger"); } }
        };

        grandmaRushMover.rushPhaseChanged.Subscribe(x => rushActions[x]()); // GrandmaRushMoverにおける，rushPhaseChangedの初期化は宣言時に行われているが，このクラスのAwakeより後になるため，Startで記述
        grandmaBallThrower.ballThrowPhaseChanged.Subscribe(x => ballThrowActions[x]()); // 同上
        grandmaStaggerMover.staggerPhaseChanged.Subscribe(x => staggerActions[x]()); // 同上

        //animator.SetTrigger("GameStartTrigger"); // 待機状態からダッシュ状態へ

        /* ゲーム本編遷移時，ダッシュアニメーションを開始 */
        gameStatusManager.OnGameStatusChanged.Where(x => x == GameStatusManager.GameStatus.Game).Subscribe(_ => animator.SetTrigger("GameStartTrigger")).AddTo(this);
    }
}
