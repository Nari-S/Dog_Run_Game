using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreResultTextView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreResultText;

    void Awake()
    {
        if (!TryGetComponent(out scoreResultText)) Debug.Log("TextMeshPro is not attached to this object");

    }

    public void ActivateScoreResultText(int score)
    {
        scoreResultText.text = "スコア     " + score.ToString();

        scoreResultText.enabled = true;
    }
}
