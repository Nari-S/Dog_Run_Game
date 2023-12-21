using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class MoveManager : MonoBehaviour
{
    [SerializeField] private GameObject standardPointObject;

    private CharacterController characterController;
    private DogStraightMover straightMover;
    private DogJumpMover jumpMover;
    private DogSideMover sideMover;

    [SerializeField] GameStatusManager gameStatusManager;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        straightMover = GetComponent<DogStraightMover>();
        jumpMover = GetComponent<DogJumpMover>();
        sideMover = GetComponent<DogSideMover>();

        this.UpdateAsObservable().Where(_ => gameStatusManager.gameStatus == GameStatusManager.GameStatus.TitleToGame || gameStatusManager.gameStatus == GameStatusManager.GameStatus.Game).Subscribe(_ => 
        {
            /* 親オブジェクトの座標をダッシュ方向に進める */
            standardPointObject.transform.Translate(straightMover.GetStraightMoveVector());

            /* ジャンプの移動量取得 */
            var jumpMoveVector = jumpMover.GetJumpMoveVector();

            /* 横移動の移動量取得 */
            var sideMoveVector = sideMover.GetSideMoveVector();

            characterController.Move(jumpMoveVector + sideMoveVector);

            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
        })
        .AddTo(this);
    }
}
