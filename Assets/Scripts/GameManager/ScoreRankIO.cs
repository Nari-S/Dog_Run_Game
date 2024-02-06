using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using NCMB;
using NCMB.Tasks;
using System.Linq;
using Cysharp.Threading.Tasks;

public class ScoreRankIO : MonoBehaviour
{
    private string dataStoreName;
    private string scoreFieldName;

    private void Awake()
    {
        dataStoreName = "Dog_Run"; // データストア名を決定
        scoreFieldName = "Score"; // スコアを格納するフィールド名を決定
    }

    /// <summary>
    /// 引数のスコアをニフクラにアップロード
    /// </summary>
    /// <param name="score"></param>
    /// <returns></returns>
    public async Task UploadScore(int score)
    {
        NCMBObject scoreObj = new NCMBObject(dataStoreName);
        scoreObj.Add(scoreFieldName, score);
        await scoreObj.SaveTaskAsync();
    }

    /// <summary>
    /// ニフクラより引数で指定の数だけ，降順スコアの上位より取得
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    public async UniTask /*Task*/ <List<int>> DownloadTopScore(int n)
    {
        NCMBQuery<NCMBObject> query = new NCMBQuery<NCMBObject>(dataStoreName);
        query.OrderByDescending(scoreFieldName); // スコアを格納するフィールドで降順ソート
        query.Limit = 5; // スコアの取得件数を設定

        IList<NCMBObject> results = await query.FindTaskAsync(); // 非同期でクエリ発行
        return results.Select(x => Convert.ToInt32(x[scoreFieldName])).ToList(); // 結果受け取り後，List<int>にしてリターン
    }
}
