using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpGaugeFrameView : MonoBehaviour
{
    [SerializeField] private Image hpGaugeFrameImage;

    // Start is called before the first frame update
    void Awake()
    {
        hpGaugeFrameImage = GetComponent<Image>();
    }

    public void ActivateHpGaugeFrame()
    {
        hpGaugeFrameImage.enabled = true;
    }
}
