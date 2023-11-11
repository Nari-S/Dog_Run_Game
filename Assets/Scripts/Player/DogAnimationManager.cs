using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogAnimationManager : MonoBehaviour
{
    [SerializeField] private Animator dogAnimator;

    private void Awake()
    {
        dogAnimator = GetComponent<Animator>();

        dogAnimator.SetBool("isRunning", true); //現状，ゲーム開始直後にダッシュスタート
    }

    public void StartRunning()
    {
        dogAnimator.SetBool("isRunning", true);
    }

    public void StartAscending()
    {
        dogAnimator.SetTrigger("jumpTrigger");
    }

    public void StartStep()
    {
        dogAnimator.SetTrigger("stepTrigger");
    }
}
