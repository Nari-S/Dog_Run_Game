using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class WHpGaugeView : MonoBehaviour
{
    [SerializeField] private Image WhpGaugeImage;

    void Awake()
    {
        WhpGaugeImage = GetComponent<Image>();
    }

    public void SetWHpGauge(float whpRate)
    {
        DOTween.To(() => WhpGaugeImage.fillAmount,
                n => WhpGaugeImage.fillAmount = n,
                whpRate,
                duration: 0.5f);
    }
}
