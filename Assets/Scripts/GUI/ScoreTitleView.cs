using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreTitleView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreTitleText;

    void Awake()
    {
        if (!TryGetComponent(out scoreTitleText)) Debug.Log("TextMeshPro is not attached to this object");
    }

    public void ActivateScoreTitleText()
    {
        scoreTitleText.enabled = true;
    }
}
