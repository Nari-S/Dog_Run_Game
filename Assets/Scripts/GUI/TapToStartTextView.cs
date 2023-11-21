using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class TapToStartTextView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tapToStartText;

    private Tween tweenBlinkAnim;

    void Awake()
    {
        if (!TryGetComponent(out tapToStartText)) Debug.Log("TextMeshPro is not attached to this object");

        tweenBlinkAnim = tapToStartText.DOFade(0f, 1f).SetLoops(-1, LoopType.Yoyo);
    }

    public void DeactivateTapToStartText()
    {
        tapToStartText.enabled = false;

        tweenBlinkAnim.Kill();
    }
}
