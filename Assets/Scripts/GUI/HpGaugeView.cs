using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HpGaugeView : MonoBehaviour
{
    [SerializeField] private Image hpGaugeImage;

    // Start is called before the first frame update
    void Awake()
    {
        hpGaugeImage = GetComponent<Image>();
    }

    public void SetHpGauge(float hpRate)
    {
        DOTween.To(() => hpGaugeImage.fillAmount,
                n => hpGaugeImage.fillAmount = n,
                hpRate,
                duration: 0.5f);
    }
}
