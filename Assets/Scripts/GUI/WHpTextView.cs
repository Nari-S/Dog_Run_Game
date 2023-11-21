using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WHpTextView : MonoBehaviour
{
    [SerializeField] private Text whpText;

    void Awake()
    {
        whpText = GetComponent<Text>();
    }

    public void SetHpText(float whp)
    {
        whpText.text = whp.ToString();
    }

    public void ActivateWHpText()
    {
        whpText.enabled = true;
    }
}
