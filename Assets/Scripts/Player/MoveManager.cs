using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveManager : MonoBehaviour
{
    private GameObject standardPointObject;

    private CharacterController characterController;
    private IStraightMover straightMover;
    private IJumpMover jumpMover;
    private ISideMover sideMover;
    private IStepMover stepMover;

    //private Vector3 moveVector;

    // Start is called before the first frame update
    void Awake() 
    {
        standardPointObject = GameObject.Find("StandardPointObject");

        characterController = GetComponent<CharacterController>();
        straightMover = GetComponent<IStraightMover>();
        jumpMover = GetComponent<IJumpMover>();
        sideMover = GetComponent<ISideMover>();
        stepMover = GetComponent<IStepMover>();
    }

    // Update is called once per frame
    private void Update()
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
    }
}
