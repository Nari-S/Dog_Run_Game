using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class BallHungerDamager : MonoBehaviour
{
    private float damage;
    public float Damage { get => damage; private set => damage = value >= 0 ? value : 0; }

    private IHungerManager playerHungerManager;
    private DogAudioController dogAudioController;

    [SerializeField] private ParticleSystem mudBallExplosionParticle;

    public void Init(float damage)
    {
        Damage = damage;

        /* IHungerManagerコンポーネントを保持しているオブジェクト（プレイヤーのみ）の腹減り度合いにダメージを与えて消す
         * OnTriggerEnterAsObservable:IObservable<collider>をリターン */
        this.OnTriggerEnterAsObservable()
            .Where(x => x.gameObject.TryGetComponent(out playerHungerManager) && x.gameObject.TryGetComponent(out dogAudioController))
            .Subscribe(_ => 
            {
                playerHungerManager.UpdateHunger(-damage);
                dogAudioController.PlayAudio(DogAudioController.AudioKinds.whine);

                MudBallExplosionParticlePlay();

                Destroy(this.gameObject);
            })
            .AddTo(this);
    }

    private void MudBallExplosionParticlePlay()
    {
        ParticleSystem particle = Instantiate(mudBallExplosionParticle);
        particle.transform.position = transform.position;
        particle.Play();
    }
}
