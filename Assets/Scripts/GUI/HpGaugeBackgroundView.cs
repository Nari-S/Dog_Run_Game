using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpGaugeBackgroundView : MonoBehaviour
{
    [SerializeField] private Image hpGaugeBackgroundImage;

    // Start is called before the first frame update
    void Awake()
    {
        hpGaugeBackgroundImage = GetComponent<Image>();
    }

    public void ActivateHpGaugeBackground()
    {
        hpGaugeBackgroundImage.enabled = true;
    }
}
