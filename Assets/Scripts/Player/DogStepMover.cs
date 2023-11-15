using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class DogStepMover : MonoBehaviour, IStepMover
{
    public bool IsStepping{ get; private set; }

    private Vector3 moveVector;
    private Vector3 nowPosition;
    private Vector3 prevPosition;

    [SerializeField] private float stepDistance;
    [SerializeField] private float stepDuration;

    [SerializeField] private InputActionAsset inputActionAsset;
    [SerializeField] private ReverseCameraManager reverseCameraManager;

    private DogAnimationManager dogAnimationManager;
    private HungerManager hungerManager;

    [SerializeField] private GameStatusManager gameStatusManager;

    void Awake()
    {
        IsStepping = false;

        moveVector = Vector3.zero;
        nowPosition = Vector3.zero;
        prevPosition = Vector3.zero;

        stepDistance = 1f;
        stepDuration = 0.5f;

        /* Startで実行
        dogAnimationManager = GetComponent<DogAnimationManager>();

        hungerManager = GetComponent<HungerManager>();*/
    }

    private void Start()
    {
        dogAnimationManager = GetComponent<DogAnimationManager>();

        hungerManager = GetComponent<HungerManager>();
    }

    public Vector3 GetStepMoveVector()
    {
        return moveVector;
    }

    /* InputSystem ?? UpFlick?C???^???N?V???????????R?[???????????\?b?h */
    public void StartStep(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (IsStepping) return;
        if (!hungerManager.canStep()) return;
        if (gameStatusManager.gameStatus != GameStatusManager.GameStatus.Game) return;

        if (context.interaction.ToString() == "RightFlickInteraction") stepDistance = Mathf.Abs(stepDistance) * GetCameraDirectionInt();
        else if (context.interaction.ToString() == "LeftFlickInteraction") stepDistance = -Mathf.Abs(stepDistance) * GetCameraDirectionInt();
        else throw new System.Exception("Right/Left Flick Interaction is not inputted.");

        hungerManager.UpdateHungerByStep(); //ステップ移動する場合，腹減り度合いを更新

        IsStepping = true;

        nowPosition = new Vector3(transform.position.x, 0, 0);
        prevPosition = new Vector3(transform.position.x, 0, 0);

        DOTween.To(
            () => nowPosition,
            pos =>
            {
                prevPosition = nowPosition;
                nowPosition = pos;
                moveVector = nowPosition - prevPosition;
            },
            nowPosition + new Vector3(stepDistance, 0, 0),
            stepDuration
        )
        .SetEase(Ease.OutQuad)
        .SetUpdate(UpdateType.Normal)
        .OnStart(() =>
        {
            dogAnimationManager.StartStep();
        })
        .OnComplete(() =>
        {
            moveVector = Vector3.zero;
            IsStepping = false;
        });
    }

    private int GetCameraDirectionInt()
    {
        if (reverseCameraManager.IsFacingFlont) return 1;
        else return -1;
    }
}
