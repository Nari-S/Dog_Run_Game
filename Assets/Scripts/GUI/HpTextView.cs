using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpTextView : MonoBehaviour
{
    [SerializeField] private Text hpText;

    // Start is called before the first frame update
    void Awake()
    {
        hpText = GetComponent<Text>();
    }

    public void SetHpText(float hp)
    {
        hpText.text = hp.ToString();
    }
}
