using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CapturedTextView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI capturedText;

    void Awake()
    {
        if (!TryGetComponent(out capturedText)) Debug.Log("TextMeshPro is not attached to this object");

    }

    public void ActivateCapturedText()
    {
        capturedText.enabled = true;
    }
}
