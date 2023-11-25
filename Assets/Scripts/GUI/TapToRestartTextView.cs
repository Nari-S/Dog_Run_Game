using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class TapToRestartTextView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tapToRestartText;

    private Tween tweenBlinkAnim;

    void Awake()
    {
        if (!TryGetComponent(out tapToRestartText)) Debug.Log("TextMeshPro is not attached to this object");
    }

    public void ActivateTapToRestartText()
    {
        tapToRestartText.enabled = true;

        tweenBlinkAnim = tapToRestartText.DOFade(0f, 1f).SetLoops(-1, LoopType.Yoyo);
    }

    private void OnDisable()
    {
        tweenBlinkAnim.Kill();
    }
}
