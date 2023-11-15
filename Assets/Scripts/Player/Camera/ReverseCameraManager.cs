using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UniRx;
using DG.Tweening;

// ExecuteAlwaysとUpdateのコメントアウトを外すことで実行前でもカメラの前後を切り替えられる
//[ExecuteAlways]
public class ReverseCameraManager : MonoBehaviour
{
    [SerializeField] private GameObject standardPointObject;

    [SerializeField] private Camera camera;

    [SerializeField] private Vector3 flontCameraPosition;
    [SerializeField] private Vector3 flontCameraRotation;

    [SerializeField] private Vector3 reverseCameraPosition;
    [SerializeField] private Vector3 reverseCameraRotation;

    [SerializeField] private bool ReverseFlag_Debug;

    [SerializeField] public bool IsFacingFlont { get; private set; }

    /* カメラ端の座標 */
    public float MaxFurthestPointInViewFront { get; private set; }
    public float MaxFurthestPointInViewRear { get; private set; }

    private float prevRotation;
    //private float cameraRotationDuration;

    [SerializeField] private GameStatusManager gameStatusManager;

    

    private void Awake()
    {
        camera = GetComponent<Camera>();

        flontCameraPosition = new Vector3(0, 5.5f, -4.5f);
        flontCameraRotation = new Vector3(36.765f, 0, 0);

        reverseCameraPosition = new Vector3(0, 5.5f, 4.5f);
        reverseCameraRotation = new Vector3(36.765f, 180f, 0);

        MaxFurthestPointInViewFront = flontCameraPosition.y / Mathf.Tan((flontCameraRotation.x - camera.fieldOfView / 2f) * Mathf.Deg2Rad) + flontCameraPosition.z + standardPointObject.transform.position.z;
        MaxFurthestPointInViewRear = - reverseCameraPosition.y / Mathf.Tan((reverseCameraRotation.x - camera.fieldOfView / 2f) * Mathf.Deg2Rad) + reverseCameraPosition.z + standardPointObject.transform.position.z;

        IsFacingFlont = true;
    }

    private void Start()
    {
        gameStatusManager.OnGameStatusChanged.Where(x => x == GameStatusManager.GameStatus.TitleToGame).Subscribe(_ =>
        {
            var dotweenSequence = DOTween.Sequence();

            var cameraRotationDuration = gameStatusManager.transitionDurationTitleToGame / 1000f - 0.5f; // タイトル→ゲーム本編になってからカメラ回転が完了するまでに要する時間
            var firstCameraPosition = camera.transform.position;
            /* カメラの回転軸ベクトルを求める
             * タイトルのカメラ位置とゲーム本編のカメラ位置間のベクトルを求めて，このベクトルのy座標だけ変更したベクトルとの垂線を求める
             * y座標だけ変更するのは，x-z平面を回るようにして柴犬の周囲をカメラが回るようにするため */
            var perpendicularFootVector = GetPerpendicularFoot(flontCameraPosition, firstCameraPosition, (flontCameraPosition + firstCameraPosition) / 2f + new Vector3(0, 0.01f, 0));

            dotweenSequence.Append(DOTween.To(
                () => 0,
                nowAngle =>
                {
                    var rotationCenterPoint = ((flontCameraPosition + standardPointObject.transform.position) + (firstCameraPosition + standardPointObject.transform.position)) / 2f; //回転の中心座標
                    RotateAroundPrc(nowAngle, camera.gameObject, rotationCenterPoint, perpendicularFootVector); // 指定した中心点，回転軸ベクトルに対してオブジェクトを公転させる
                }, 180, cameraRotationDuration))
                .SetEase(Ease.Linear);
            dotweenSequence.Join(camera.transform.DOLocalRotate(flontCameraRotation, cameraRotationDuration)).SetEase(Ease.Linear); // カメラの初期方向から最終方向にゆっくり向かせる
            dotweenSequence.Play();
        })
        .AddTo(this);
    }

    /// <summary>
    /// 点Pから直線ABに下ろした垂線の足の座標を求める
    /// その座標から点Pまでの垂線ベクトルを返す
    /// </summary>
    /// <param name="a">ベクトルABを成す点A</param>
    /// <param name="b">ベクトルABを成す点A</param>
    /// <param name="p">ベクトルABに対して垂直な線上を通る点P</param>
    /// <returns>点Pから直線ABに下ろした垂線の足の座標</returns>
    private Vector3 GetPerpendicularFoot(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 perpendicularFootPoint = a + Vector3.Project(p - a, b - a); // 点Pから直線ABに下ろした垂線の足の座標

        return p - perpendicularFootPoint; // 垂線の足の座標から点Pまでの垂線ベクトルを返す

    }

    /// <summary>
    /// 公転処理
    /// </summary>
    /// <param name="value"></param>
    /// <param name="revolvingObject"></param>
    /// <param name="originObject"></param>
    private void RotateAroundPrc(float value, GameObject revolvingObject, Vector3 originPoint, Vector3 axis)
    {
        // 前回との差分を計算
        float delta = value - prevRotation;

        // Y軸周りに公転運動
        revolvingObject.transform.RotateAround(originPoint, axis, delta);

        // 前回の角度を更新
        prevRotation = value;
    }

    private void Update()
    {
        MaxFurthestPointInViewFront = flontCameraPosition.y / Mathf.Tan((flontCameraRotation.x - camera.fieldOfView / 2f) * Mathf.Deg2Rad) + flontCameraPosition.z + standardPointObject.transform.position.z;
        MaxFurthestPointInViewRear = - reverseCameraPosition.y / Mathf.Tan((reverseCameraRotation.x - camera.fieldOfView / 2f) * Mathf.Deg2Rad) + reverseCameraPosition.z + standardPointObject.transform.position.z;

        /* 以下ifとelseifのコメントアウトを消すと，下フリックによるカメラの向き変更が効くようになる */
        /*if (ReverseFlag_Debug)
        {
            IsFacingFlont = false;

            camera.transform.localPosition = reverseCameraPosition;
            camera.transform.localEulerAngles = reverseCameraRotation;
        }

        else if (!ReverseFlag_Debug)
        {
            IsFacingFlont = true;

            camera.transform.localPosition = flontCameraPosition;
            camera.transform.localEulerAngles = flontCameraRotation;
        }*/
    }

    public void ReverseCamera(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (gameStatusManager.gameStatus != GameStatusManager.GameStatus.Game) return;

        if (IsFacingFlont)
        {
            IsFacingFlont = false;

            camera.transform.localPosition = reverseCameraPosition;
            camera.transform.localEulerAngles = reverseCameraRotation;
        }
        else
        {
            IsFacingFlont = true;

            camera.transform.localPosition = flontCameraPosition;
            camera.transform.localEulerAngles = flontCameraRotation;
        }
    }
}
