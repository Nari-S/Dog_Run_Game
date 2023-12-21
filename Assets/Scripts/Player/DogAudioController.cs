using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

public class DogAudioController : MonoBehaviour
{
    public enum AudioKinds
    {
        bowwow,
        whine,
        eating,
        drinking,
        growling
    }

    private AudioSource audioSource;
    [SerializeField] private List<AudioClip> bowwowAudios;
    [SerializeField] private List<AudioClip> whineAudios;
    [SerializeField] private List<AudioClip> eatingAudios;
    [SerializeField] private List<AudioClip> drinkingAudios;
    [SerializeField] private List<AudioClip> growlingAudios;
    private Dictionary<AudioKinds, List<AudioClip>> dogAllAudios;

    private HungerManager hungerManager;

    private void Start()
    {
        if (!TryGetComponent(out audioSource)) Debug.Log("AudioSource is not attached to this object.");
        if (!TryGetComponent(out hungerManager)) Debug.Log("HungerManager is not attached to this object.");

        /* Enumと音声ファイルを紐づけた辞書の作成 */
        dogAllAudios = new Dictionary<AudioKinds, List<AudioClip>>() {
            { AudioKinds.bowwow, bowwowAudios }, { AudioKinds.whine, whineAudios }, { AudioKinds.eating, eatingAudios },{ AudioKinds.drinking, drinkingAudios }, { AudioKinds.growling, growlingAudios }
        };

        /* 腹減り度が最小値になったとき，鳴き声と腹減り音を再生 */
        hungerManager.Hunger.Where(x => x <= hungerManager.MinHunger).Subscribe(_ => { PlayAudio(AudioKinds.whine, true); PlayAudio(AudioKinds.growling, true); }).AddTo(this);
    }

    /// <summary>
    /// サウンドを再生
    /// </summary>
    /// <param name="audioKind">再生するサウンドの種類を指定</param>
    /// <param name="isOneShot">複数の音源を再生するかを指定．デフォルトでは，同時再生しない</param>
    public void PlayAudio(AudioKinds audioKind, bool isOneShot = false)
    {
        var clip = dogAllAudios[audioKind].OrderBy(_ => Guid.NewGuid()).FirstOrDefault(); // リストからランダムにサウンドを抽出
        if (!isOneShot)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
        else audioSource.PlayOneShot(clip);
    }
}
