using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class MoveManager : MonoBehaviour
{
    [SerializeField] private GameObject standardPointObject;

    private CharacterController characterController;
    private IStraightMover straightMover;
    private IJumpMover jumpMover;
    private ISideMover sideMover;
    private IStepMover stepMover;

    [SerializeField] GameStatusManager gameStatusManager;

    //private Vector3 moveVector;

    // Start is called before the first frame update
    void Awake() 
    {
        //standardPointObject = GameObject.Find("StandardPointObject");

        /* startで実行
        characterController = GetComponent<CharacterController>();
        straightMover = GetComponent<IStraightMover>();
        jumpMover = GetComponent<IJumpMover>();
        sideMover = GetComponent<ISideMover>();
        stepMover = GetComponent<IStepMover>(); */
    }

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        straightMover = GetComponent<IStraightMover>();
        jumpMover = GetComponent<IJumpMover>();
        sideMover = GetComponent<ISideMover>();
        stepMover = GetComponent<IStepMover>();

        this.UpdateAsObservable().Where(_ => gameStatusManager.gameStatus == GameStatusManager.GameStatus.TitleToGame || gameStatusManager.gameStatus == GameStatusManager.GameStatus.Game).Subscribe(_ => 
        {
            /* 親オブジェクトの座標をダッシュ方向に進める */
            standardPointObject.transform.Translate(straightMover.GetStraightMoveVector());

            /* ジャンプの移動量取得 */
            var jumpMoveVector = jumpMover.GetJumpMoveVector();

            /* 横移動の移動量取得 */
            var sideMoveVector = sideMover.GetSideMoveVector();

            /* ステップの移動量取得 */
            var stepMoveVector = stepMover.GetStepMoveVector();

            characterController.Move(jumpMoveVector + sideMoveVector + stepMoveVector);

            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
        })
        .AddTo(this);
    }

    // Update is called once per frame
    private void Update()
    {
        /* UpdateAsObservableで実行
        /* 親オブジェクトの座標をダッシュ方向に進める 
        standardPointObject.transform.Translate(straightMover.GetStraightMoveVector());

        /* ジャンプの移動量取得 
        var jumpMoveVector = jumpMover.GetJumpMoveVector();

        /* 横移動の移動量取得 
        var sideMoveVector = sideMover.GetSideMoveVector();

        /* ステップの移動量取得 
        var stepMoveVector = stepMover.GetStepMoveVector();

        characterController.Move(jumpMoveVector + sideMoveVector + stepMoveVector);

        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
        */
    }
}
