using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using DG.Tweening;

public class BGMManager : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private AudioClip titleBGM;
    [SerializeField] private AudioClip gameBGM;
    [SerializeField] private AudioClip endingSound;

    private float bGMVolume;
    private float soundVolume;

    private GameStatusManager gameStatusManager;

    private void Awake()
    {
        if (!TryGetComponent(out audioSource)) Debug.Log("AudioSource is not attached to this object.");
        if (!TryGetComponent(out gameStatusManager)) Debug.Log("GameStatusManager is not attached to this object.");

        bGMVolume = 0.06f;
        soundVolume = 0.5f;
    }

    private void Start()
    {
        PlayAudio(audioSource, titleBGM, bGMVolume); // シーン読み込み時にタイトルBGM再生

        /* タイトル→ゲーム本編遷移時，ゲーム本編BGMを再生 */
        gameStatusManager.OnGameStatusChanged.Where(x => x == GameStatusManager.GameStatus.TitleToGame).Subscribe(_ => PlayAudio(audioSource, gameBGM, bGMVolume)).AddTo(this);

        /* ゲーム本編→スコア時，ゲーム本編BGMをフェードアウト停止 */
        gameStatusManager.OnGameStatusChanged.Where(x => x == GameStatusManager.GameStatus.GameToScore).Subscribe(_ => StopAudio(audioSource, gameStatusManager.transitionDurationGameToScore / 1000 - 0.1f)).AddTo(this);

        /* スコア時，失敗BGM再生 */
        gameStatusManager.OnGameStatusChanged.Where(x => x == GameStatusManager.GameStatus.Score).Subscribe(_ => PlayAudio(audioSource, endingSound, soundVolume)).AddTo(this);
    }

    private void PlayAudio(AudioSource audioSource, AudioClip audioClip, float volume)
    {
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();
    }

    /// <summary>
    /// AudioSourceをフェードアウトさせたのちに停止させる
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="fadeoutDuration"></param>
    private void StopAudio(AudioSource audioSource, float fadeoutDuration)
    {
        audioSource.DOFade(0f, fadeoutDuration).OnComplete(() => audioSource.Stop());
    }
}
