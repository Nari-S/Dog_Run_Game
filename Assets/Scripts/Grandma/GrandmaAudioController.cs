using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

public class GrandmaAudioController : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private List<AudioClip> gameStartAudios;
    [SerializeField] private List<AudioClip> rushingPreparationAudios;
    [SerializeField] private List<AudioClip> rushingAudios;
    [SerializeField] private List<AudioClip> rushingCooldownAudios;
    [SerializeField] private List<AudioClip> ballThrowingPreparationAudios;
    [SerializeField] private List<AudioClip> ballThrowingAudios;
    [SerializeField] private List<AudioClip> staggerAudios;

    private GrandmaRushMover grandmaRushMover;
    private GrandmaBallThrower grandmaBallThrower;
    private GrandmaStaggerMover grandmaStaggerMover;

    private Dictionary<GrandmaRushMover.RushPhase, Action> rushActions;
    private Dictionary<GrandmaBallThrower.BallThrowPhase, Action> ballThrowActions;
    private Dictionary<GrandmaStaggerMover.StaggerPhase, Action> staggerActions;

    [SerializeField] private GameStatusManager gameStatusManager;

    private void Awake()
    {
        /* Startで実行
        if (!TryGetComponent(out audioSource)) Debug.Log("AudioSource is not attached to this object.");
        if (!TryGetComponent(out grandmaRushMover)) Debug.Log("GrandmaRushMover is not attached to this object.");
        if (!TryGetComponent(out grandmaBallThrower)) Debug.Log("GrandmaBallThrower is not attached to this object.");
        if (!TryGetComponent(out grandmaStaggerMover)) Debug.Log("GrandmaStaggerMover is not attached to this object.");

        rushActions = new Dictionary<GrandmaRushMover.RushPhase, Action>() {
            { GrandmaRushMover.RushPhase.OutOfRange,    () => { ; } },
            { GrandmaRushMover.RushPhase.Preparation,   () => { audioSource.clip = rushingPreparationAudios.OrderBy(_ => Guid.NewGuid()).FirstOrDefault(); audioSource.Play(); } },
            { GrandmaRushMover.RushPhase.Rushing,       () => { audioSource.clip = rushingAudios.OrderBy(_ => Guid.NewGuid()).FirstOrDefault(); audioSource.Play(); } },
            { GrandmaRushMover.RushPhase.Cooldown,      () => { audioSource.clip = rushingCooldownAudios.OrderBy(_ => Guid.NewGuid()).FirstOrDefault(); audioSource.Play(); } }
        };

        ballThrowActions = new Dictionary<GrandmaBallThrower.BallThrowPhase, Action>() {
            { GrandmaBallThrower.BallThrowPhase.OutOfPeriod,    () => { ; } },
            { GrandmaBallThrower.BallThrowPhase.Preparation,    () => { audioSource.clip = ballThrowingPreparationAudios.OrderBy(_ => Guid.NewGuid()).FirstOrDefault(); audioSource.Play(); } },
            { GrandmaBallThrower.BallThrowPhase.Throwing,       () => { audioSource.clip = ballThrowingAudios.OrderBy(_ => Guid.NewGuid()).FirstOrDefault(); audioSource.Play(); } }
        };

        staggerActions = new Dictionary<GrandmaStaggerMover.StaggerPhase, Action>()
        {
            { GrandmaStaggerMover.StaggerPhase.OutOfPeriod,     () => { ; } },
            { GrandmaStaggerMover.StaggerPhase.Stagger,         () => { audioSource.clip = staggerAudios.OrderBy(_ => Guid.NewGuid()).FirstOrDefault(); audioSource.Play(); } }
        };
        */
    }

    private void Start()
    {
        if (!TryGetComponent(out audioSource)) Debug.Log("AudioSource is not attached to this object.");
        if (!TryGetComponent(out grandmaRushMover)) Debug.Log("GrandmaRushMover is not attached to this object.");
        if (!TryGetComponent(out grandmaBallThrower)) Debug.Log("GrandmaBallThrower is not attached to this object.");
        if (!TryGetComponent(out grandmaStaggerMover)) Debug.Log("GrandmaStaggerMover is not attached to this object.");

        rushActions = new Dictionary<GrandmaRushMover.RushPhase, Action>() {
            { GrandmaRushMover.RushPhase.OutOfRange,    () => { ; } },
            { GrandmaRushMover.RushPhase.Preparation,   () => { audioSource.clip = rushingPreparationAudios.OrderBy(_ => Guid.NewGuid()).FirstOrDefault(); audioSource.Play(); } },
            { GrandmaRushMover.RushPhase.Rushing,       () => { audioSource.clip = rushingAudios.OrderBy(_ => Guid.NewGuid()).FirstOrDefault(); audioSource.Play(); } },
            { GrandmaRushMover.RushPhase.Cooldown,      () => { audioSource.clip = rushingCooldownAudios.OrderBy(_ => Guid.NewGuid()).FirstOrDefault(); audioSource.Play(); } }
        };

        ballThrowActions = new Dictionary<GrandmaBallThrower.BallThrowPhase, Action>() {
            { GrandmaBallThrower.BallThrowPhase.OutOfPeriod,    () => { ; } },
            { GrandmaBallThrower.BallThrowPhase.Preparation,    () => { audioSource.clip = ballThrowingPreparationAudios.OrderBy(_ => Guid.NewGuid()).FirstOrDefault(); audioSource.Play(); } },
            { GrandmaBallThrower.BallThrowPhase.Throwing,       () => { audioSource.clip = ballThrowingAudios.OrderBy(_ => Guid.NewGuid()).FirstOrDefault(); audioSource.Play(); } }
        };

        staggerActions = new Dictionary<GrandmaStaggerMover.StaggerPhase, Action>()
        {
            { GrandmaStaggerMover.StaggerPhase.OutOfPeriod,     () => { ; } },
            { GrandmaStaggerMover.StaggerPhase.Stagger,         () => { audioSource.clip = staggerAudios.OrderBy(_ => Guid.NewGuid()).FirstOrDefault(); audioSource.Play(); } }
        };

        grandmaRushMover.rushPhaseChanged.Subscribe(x => rushActions[x]()); // GrandmaRushMoverにおける，rushPhaseChangedの初期化は宣言時に行われているが，このクラスのAwakeより後になるため，Startで記述
        grandmaBallThrower.ballThrowPhaseChanged.Subscribe(x => ballThrowActions[x]()); // 同上
        grandmaStaggerMover.staggerPhaseChanged.Subscribe(x => staggerActions[x]()); // 同上

        /* タイトル→ゲーム本編遷移時，ゲーム開始音声（まてええい）を再生 */
        gameStatusManager.OnGameStatusChanged.Where(x => x == GameStatusManager.GameStatus.TitleToGame).Subscribe(_ => 
        {
            audioSource.clip = gameStartAudios.OrderBy(__ => Guid.NewGuid()).FirstOrDefault();
            audioSource.Play();
        }).AddTo(this);
    }

}
