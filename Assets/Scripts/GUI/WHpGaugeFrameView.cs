using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WHpGaugeFrameView : MonoBehaviour
{
    [SerializeField] private Image whpGaugeFrameImage;

    // Start is called before the first frame update
    void Awake()
    {
        whpGaugeFrameImage = GetComponent<Image>();
    }

    public void ActivateWHpGaugeFrame()
    {
        whpGaugeFrameImage.enabled = true;
    }
}
