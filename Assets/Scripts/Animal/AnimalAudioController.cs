using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UniRx;

public class AnimalAudioController : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip footStepAudio;
    [SerializeField] private AudioClip cryAudio;

    private AnimalCollisionDetector animalCollisionDetector;
    private IDisposable crySubscriber; // 鳴き声を再生する購読

    private void OnEnable()
    {
        if (!TryGetComponent(out audioSource)) Debug.Log("AudioSource is not attached to this object.");
        if (!TryGetComponent(out animalCollisionDetector)) Debug.Log("AnimalCollisionDetector is not attached to this object.");

        crySubscriber = animalCollisionDetector.OnDeleted.Subscribe(_ => PlayCryAudio());
    }

    /// <summary>
    /// 足音を再生
    /// </summary>
    public void PlayFootStepAudio()
    {
        audioSource.clip = footStepAudio;
        audioSource.Play();
    }

    /// <summary>
    /// 鳴き声を再生
    /// </summary>
    public void PlayCryAudio()
    {
        audioSource.clip = cryAudio;
        audioSource.Play();
    }

    private void OnDisable()
    {
        crySubscriber?.Dispose();
    }
}
