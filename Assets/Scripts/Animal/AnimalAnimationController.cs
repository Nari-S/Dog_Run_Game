using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;

public class AnimalAnimationController : MonoBehaviour
{
    private Animator animalAnimator;
    private AnimalCollisionDetector animalCollisionDetector;
    private IDisposable animationSubscriber;

    private void OnEnable()
    {
        if (!TryGetComponent(out animalAnimator)) Debug.Log("Animator is not attached to this object.");
        if (!TryGetComponent(out animalCollisionDetector)) Debug.Log("AnimalCollisionDetector is not attached to this object.");

        animationSubscriber = animalCollisionDetector.OnDeleted.Subscribe(_ => animalAnimator.SetTrigger("Death"));
    }

    private void OnDisable()
    {
        animationSubscriber?.Dispose();
    }
}
