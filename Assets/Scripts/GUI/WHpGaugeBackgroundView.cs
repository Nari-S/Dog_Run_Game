using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WHpGaugeBackgroundView : MonoBehaviour
{
    [SerializeField] private Image whpGaugeBackgroundImage;

    // Start is called before the first frame update
    void Awake()
    {
        whpGaugeBackgroundImage = GetComponent<Image>();
    }

    public void ActivateWHpGaugeBackground()
    {
        whpGaugeBackgroundImage.enabled = true;
    }
}
