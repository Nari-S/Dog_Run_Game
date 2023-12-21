using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class DogJumpMover : MonoBehaviour
{
    public bool IsAscending { get; set; }
    public bool IsDescending { get; set; }

    private Vector3 moveVector;
    private Vector3 nowPosition;
    private Vector3 prevPosition;

    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpDuration;

    private Tween jumpTween;

    private IStepMover stepMover;
    private DogAnimationManager dogAnimationManager;
    private DogAudioController dogAudioController;

    [SerializeField] private GameStatusManager gameStatusManager;

    void Awake()
    {
        IsAscending = false;
        IsDescending = false;

        moveVector = Vector3.zero;
        nowPosition = Vector3.zero;
        prevPosition = Vector3.zero;

        jumpHeight = 1f;
        jumpDuration = 0.5f;

        /* Startに移動
         * stepMover = GetComponent<IStepMover>();
        dogAnimationManager = GetComponent<DogAnimationManager>();
        dogAudioController = GetComponent<DogAudioController>();*/
    }

    private void Start()
    {
        stepMover = GetComponent<IStepMover>();
        dogAnimationManager = GetComponent<DogAnimationManager>();
        dogAudioController = GetComponent<DogAudioController>();
    }

    public Vector3 GetJumpMoveVector()
    {
        return moveVector;
    }

    /* InputSystem の UpFlickインタラクションによりコールされるメソッド */
    public void StartAscendingJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (IsAscending || IsDescending) return;
        if (gameStatusManager.gameStatus != GameStatusManager.GameStatus.Game) return;

        IsAscending = true;

        nowPosition = new Vector3(0, transform.position.y, 0);
        prevPosition = new Vector3(0, transform.position.y, 0);

        DOTween.To(
            () => nowPosition,
            pos => 
            {
                prevPosition = nowPosition;
                nowPosition = pos;
                moveVector = nowPosition - prevPosition;
            },
            nowPosition + new Vector3(0, jumpHeight, 0),
            jumpDuration
        )
        .SetEase(Ease.OutQuad)
        .SetUpdate(UpdateType.Normal)
        .OnStart(() =>
        {
            dogAnimationManager.StartAscending();
            dogAudioController.PlayAudio(DogAudioController.AudioKinds.bowwow);
        }
        )
        .OnComplete(() =>
        {
            IsAscending = false;
            IsDescending = true;

            StartDescendingJump();
        });
    }

    private void StartDescendingJump()
    {
        prevPosition = new Vector3(0, transform.position.y, 0);
        nowPosition = new Vector3(0, transform.position.y, 0);

        DOTween.To(
            () => nowPosition,
            pos =>
            {
                prevPosition = nowPosition;
                nowPosition = pos;
                moveVector = nowPosition - prevPosition;
            },
            nowPosition + new Vector3(0, -jumpHeight, 0),
            jumpDuration
        )
        .SetEase(Ease.InQuad)
        .SetUpdate(UpdateType.Normal)
        .OnComplete(() =>
        {
            IsDescending = false;
        });
    }
}
