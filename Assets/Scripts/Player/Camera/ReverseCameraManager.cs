using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[ExecuteAlways]
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
