using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    private float speedPerSec;
    public float SpeedPerSec {
        get => speedPerSec;
        private set => speedPerSec = Mathf.Min(10f, Mathf.Max(0, value));
    }

    private float startBallPositionZ;

    /* 初期化．BallThrowerからコール想定． */
    public void Init(float size, float speedPerSec)
    {
        transform.localScale = new Vector3(size, size, size);

        this.speedPerSec = speedPerSec;

        startBallPositionZ = transform.position.z;

        if(TryGetComponent<BallHungerDamager>(out var ballHungerDamager))
        {
            ballHungerDamager.Init(15f); // 泥団子が腹減り度合いに与えるダメージを設定
        }
        else
        {
            Debug.Log("BallHungerDamager is not attached");
        }
    }

    private void Update()
    {
        transform.Translate(0, 0, speedPerSec * Time.deltaTime);

        if(MeetDestroyingBallRequirement())
        {
            Destroy(gameObject);
        }
    }

    private bool MeetDestroyingBallRequirement()
    {
        return transform.position.z - startBallPositionZ > 50f ? true : false;
    }
}
