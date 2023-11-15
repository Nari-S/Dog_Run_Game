using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameStatusManager : MonoBehaviour
{
    public enum GameStatus
    {
        Title,
        TitleToGame,
        Game,
        GameToScore,
        Score
    }

    private ReactiveProperty<GameStatus> _OnGameStatusChanged;
    public IReadOnlyReactiveProperty<GameStatus> OnGameStatusChanged => _OnGameStatusChanged;
    public GameStatus gameStatus { get => _OnGameStatusChanged.Value; private set => _OnGameStatusChanged.Value = value; }

    [SerializeField] GameObject player;
    private IObstacleReceivable obstacleReceivable;

    public int transitionDurationTitleToGame; // タイトル→ゲーム本編からゲーム本編への遷移までにかかる時間(ms)
    public int transitionDurationGameToScore; // ゲーム本編からゲーム本編→スコア画面への遷移までにかかる時間(ms)

    private void Awake()
    {
        _OnGameStatusChanged = new ReactiveProperty<GameStatus>(GameStatus.Title);

        transitionDurationTitleToGame = 3000;
        transitionDurationGameToScore = 3000;

        /* ゲームステータスがTitleToGameになった後，n秒待ってゲーム本編に遷移 */
        this.UpdateAsObservable().Where(_ => gameStatus == GameStatus.TitleToGame).Subscribe(async _ =>
        {
            await Task.Delay(transitionDurationTitleToGame);
            gameStatus = GameStatus.Game;
        })
        .AddTo(this);
    }

    private void Start()
    {
        if (!player.TryGetComponent(out obstacleReceivable)) Debug.Log("IObstacleReceivable is not attached to player");

        /* 柴犬オブジェクトよりゲームオーバ通知を受け取ったとき，ゲーム本編からゲーム本編→スコア画面に遷移 */
        obstacleReceivable.OnGameOverd.Where(_ => gameStatus == GameStatus.Game).Subscribe(_ =>gameStatus = GameStatus.GameToScore).AddTo(this);

        /* ゲームステータスがGameToScoreになった後，m秒待ってスコア画面に遷移 */
        this.UpdateAsObservable().Where(_ => gameStatus == GameStatus.GameToScore).Subscribe(async _ =>
        {
            await Task.Delay(transitionDurationGameToScore);
            gameStatus = GameStatus.Score;
        })
        .AddTo(this);
    }

    /// <summary>
    /// タイトル画面でタップを検出した際，ゲームステータスをTitleToGameに更新
    /// </summary>
    /// <param name="context"></param>
    public void ChangeGameStatusTitleToGame(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (gameStatus != GameStatus.Title) return;

        gameStatus = GameStatus.TitleToGame;
    }

    private void Update()
    {
        Debug.Log(gameStatus);
    }
}
