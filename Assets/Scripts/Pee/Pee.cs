using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class Pee : MonoBehaviour
{
    private float peeDuration;
    private float PeeDuration { get => peeDuration; set => peeDuration = Mathf.Max(0.1f, value); }
    private float maxDistanceInHitBox;

    [SerializeField] ParticleSystem peeParticle;

    private Subject<float> _OnPeeHit;
    public IObservable<float> OnPeeHit => _OnPeeHit;

    public void Init(Vector3 peeSize, Vector3 peePosition, float peeDuration)
    {
        transform.localScale = peeSize;
        transform.position = peePosition;
        PeeDuration = peeDuration;

        maxDistanceInHitBox = new Vector2(peeSize.x, peeSize.z).magnitude;

        IPeeReceivable[] peeReceivables = default;

        _OnPeeHit = new Subject<float>();

        /* ションベンが別オブジェクトに当たった際，相手のコンポーネントがIPeeReceivableを実装していれば，ションベンと衝突オブジェクト間の距離を引数にIPeeReceivable.AffectPee()（ダメージを与える処理）を呼ぶ */
        this.OnTriggerEnterAsObservable()
            .Where(x => {
                peeReceivables = x.GetComponents<IPeeReceivable>();
                return peeReceivables != null;
            })
            .Subscribe(x => {
                /* ションベンオブジェクトの対角線の長さに対する敵までの距離の比率を求めた後，比率が最大の時に0，最小の時に1となる変数
                 * つまり，プレイヤーから敵に近ければ1，遠ければ0になる */
                var normalizedInversionDistanceToEnemy = 1f - Mathf.Min(CalcDistanceToEnemy(x.transform.position) / maxDistanceInHitBox, 1f);

                _OnPeeHit.OnNext(normalizedInversionDistanceToEnemy); // ションベンオブジェクトの衝突イベントを，敵までの距離を正規化・反転した値と共に通知

                /* IpeeReceivableを実装した全てのクラスのAffectPee()メソッドを呼ぶ */
                foreach(var peeReceivable in peeReceivables)
                {
                    peeReceivable.AffectPee(normalizedInversionDistanceToEnemy);
                }
            })
            .AddTo(this);

        /* ションベンが別オブジェクトに当たった際，相手のコンポーネントのいずれかがIDeletableを実装していれば，IDeletable.DeleteObject()（削除する処理）を呼ぶ */
        this.OnTriggerEnterAsObservable().Select(x => x.GetComponent<IDeletable>()).Where(x => x != null).Subscribe(x => x.DeleteObject()).AddTo(this);

        PeeParticlePlay(); // パーティクルシステムでションベンエフェクト再生

        Destroy(this.gameObject, peeDuration); // ションベンオブジェクトは一定時間後に消える
    }

    /// <summary>
    /// 敵に与えるダメージの元であるションベンオブジェクトから敵までの距離を計算．
    /// 敵の腹減り度と停止時間に影響．
    /// </summary>
    /// <param name="enemyPosition">敵の座標</param>
    /// <returns></returns>
    private float CalcDistanceToEnemy(Vector3 enemyPosition)
    {
        var peeOriginPosition = transform.position + new Vector3(0, 0, transform.localScale.z / 2); //ションベンオブジェクトの距離計算起点を，x座標はオブジェクトの中心，z座標は端点（プレイヤー側）とする

        return Vector2.Distance(new Vector2(peeOriginPosition.x, peeOriginPosition.z), new Vector2(enemyPosition.x, enemyPosition.z));
    }

    private void PeeParticlePlay()
    {
        ParticleSystem particle = Instantiate(peeParticle);
        particle.transform.position = TryGetComponent<CapsuleCollider>(out var playerCapsuleCollider) ?
            transform.position - new Vector3(0f, 0f, playerCapsuleCollider.height / 2) :
            transform.position + new Vector3(0f, 0f, transform.localScale.z / 2);

        particle.Play();
    }
}
