using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreTextView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    void Awake()
    {
        if (!TryGetComponent(out scoreText)) Debug.Log("TextMeshPro is not attached to this object");
    }

    public void SetScoreText(int score)
    {
        scoreText.text = score.ToString();
    }
}
