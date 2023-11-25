using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class ScoreRankingTextView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreRankingText;

    void Awake()
    {
        if (!TryGetComponent(out scoreRankingText)) Debug.Log("TextMeshPro is not attached to this object");

        scoreRankingText.text = "ランキング読み込み中...";
    }

    public void ActivateScoreRankingText()
    {
        scoreRankingText.enabled = true;
    }

    /// <summary>
    /// スコアランキングを作成してテキストに設定
    /// </summary>
    /// <param name="scoreRanking"></param>
    public void SetScoreRanking(List<int> scoreRanking)
    {
        /* アップロード or ダウンロード失敗時のエラーハンドリング */
        if(scoreRanking == null)
        {
            scoreRankingText.text = "ネットワークエラーが発生しました";
            return;
        }

        /* スコアランキング作成 */
        scoreRankingText.text = "";

        foreach(var (score, index) in scoreRanking.Select((value, index) => (value, index + 1))) { // IEnumerable.Select((a, b) => ... は，a:要素値, b:要素のインデックスというオーバーロード
            scoreRankingText.text += index.ToString() + "位     " + score.ToString() + "\n\n";
        }
    }
}
