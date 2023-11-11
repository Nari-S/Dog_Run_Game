using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DogAudioController : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private List<AudioClip> bowwowAudios;
    [SerializeField] private List<AudioClip> whineAudios;
    [SerializeField] private List<AudioClip> eatingAudios;
    [SerializeField] private List<AudioClip> drinkingAudios;

    private void Awake()
    {
        if (!TryGetComponent(out audioSource)) Debug.Log("AudioSource is not attached to this object.");
    }

    public void PlayBowwowAudio()
    {
        audioSource.clip = bowwowAudios.OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
        audioSource.Play();
    }

    public void PlayWhineAudio()
    {
        audioSource.clip = whineAudios.OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
        audioSource.Play();
    }

    public void PlayEatingAudio()
    {
        audioSource.clip = eatingAudios.OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
        audioSource.Play();
    }

    public void PlayDrinkingAudio()
    {
        audioSource.clip = drinkingAudios.OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
        audioSource.Play();
    }
}
