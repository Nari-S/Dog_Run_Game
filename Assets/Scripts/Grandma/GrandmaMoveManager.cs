using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class GrandmaMoveManager : MonoBehaviour
{
    private CharacterController characterController;
    private GrandmaStraightMover straightMover;
    private GrandmaSideMover sideMover;
    private GrandmaRushMover rushMover;
    private GrandmaBallThrower ballThrower;
    private GrandmaStaggerMover staggerMover;

    [SerializeField] private GameStatusManager gameStatusManager;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        straightMover = GetComponent<GrandmaStraightMover>();
        sideMover = GetComponent<GrandmaSideMover>();
        rushMover = GetComponent<GrandmaRushMover>();
        ballThrower = GetComponent<GrandmaBallThrower>();
        staggerMover = GetComponent<GrandmaStaggerMover>();
    }

    private void Update()
    {
        if (gameStatusManager.gameStatus != GameStatusManager.GameStatus.Game) return;

        Vector3 moveVector = Vector3.zero;
        
        if(rushMover.rushPhase != GrandmaRushMover.RushPhase.OutOfRange)
        {
            rushMove();
            return;
        }

        if (ballThrower.ballThrowPhase != GrandmaBallThrower.BallThrowPhase.OutOfPeriod)
        {
            ballThrowingMove();
            return;
        }

        if(staggerMover.staggerPhase != GrandmaStaggerMover.StaggerPhase.OutOfPeriod)
        {
            staggerMove();
            return;
        }

        normalMove();

        //characterController.Move(moveVector); // いらなくね？
    }

    /* 突進・投擲中でないときの移動 */
    private void normalMove()
    {
        var straightMoveVector = straightMover.GetStraightMoveVector();

        var sideMoveVector = sideMover.GetSideMoveVector();

        characterController.Move(straightMoveVector + sideMoveVector);
    }

    /* 突進中の移動 */
    private void rushMove()
    {
        var rushMoveVector = rushMover.GetRushMoveVector();

        characterController.Move(rushMoveVector);
    }

    /* 投擲中の移動 */
    private void ballThrowingMove()
    {
        var ballThrowingMoveVector = ballThrower.GetBallThrowMoveVector();

        characterController.Move(ballThrowingMoveVector);
    }

    /* 怯み中の移動 */
    private void staggerMove()
    {
        var staggerMoveVector = staggerMover.GetStaggerMoveVector();

        characterController.Move(staggerMoveVector);
    }
}