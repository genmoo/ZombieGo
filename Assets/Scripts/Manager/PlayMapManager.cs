using UnityEngine;
using UnityEngine.Tilemaps;
using Fusion;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

public class PlayMapManager : NetworkBehaviour
{
    public static PlayMapManager Instance;
    public Grid grid;
    public Tilemap wallTilemap;
    public List<Net_PlayerController> players = new List<Net_PlayerController>();
    public PlayerScoreboard playerScoreboard;
    // Networked timer
    [Networked] private TickTimer timer { get; set; }
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float timerDuration = 70f;
    // Count Player
    private int ZombieCount = 0;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private GameObject endUi;
    private CancellationTokenSource cts;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        endUi.SetActive(false);
        cts = new CancellationTokenSource();
    }

    public override void Spawned()
    {
        StartRound();
    }

    private void StartRound()
    {
        if (HasStateAuthority)
        {
            timer = TickTimer.CreateFromSeconds(Runner, timerDuration);
        }
        UpdateTimerLoop(cts.Token).Forget();
        RandomChoiseZombie(cts.Token).Forget();
    }

    private async UniTaskVoid UpdateTimerLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            float remaining = timer.RemainingTime(Runner) ?? 0f;

            int min = Mathf.FloorToInt(remaining / 60f);
            int sec = Mathf.FloorToInt(remaining % 60f);

            if (timerText != null)
                timerText.text = $"{min:00}:{sec:00}";

            if (remaining <= 0f)
                break;
            await UniTask.Delay(500, cancellationToken: token); // 0.5초 간격
        }

        if (timerText != null)
            timerText.text = "00:00";
        Debug.Log("타이머 종료");

        // 시간으로 끝내기
        // EndGameUi();
        EndGameSceneChange();
    }

    private async UniTaskVoid RandomChoiseZombie(CancellationToken token)
    {
        float countdown = 10f;

        while (countdown > 0)
        {
            int sec = Mathf.CeilToInt(countdown);
            countdownText.text = $"좀비 선택까지: {sec}초";
            await UniTask.Delay(1000, cancellationToken: token);
            countdown -= 1f;
        }

        countdownText.text = "좀비 선택 중...";
        // Shared 모드 마스터 클라이언트만 실행
        if (Runner.IsSharedModeMasterClient)
        {
            if (players.Count == 0) return;

            int randomIndex = Random.Range(0, players.Count);
            Net_PlayerController chosenPlayer = players[randomIndex];

            // RPC 호출해서 모든 클라이언트가 동기화되도록
            chosenPlayer.RPC_SetAsZombie();
        }

        await UniTask.Delay(1000, cancellationToken: token);
        countdownText.text = "";
    }

    public void AddZombie()
    {
        ZombieCount++;
        EndGame();
    }

    private void EndGame()
    {
        int playerCount = players.Count;
        if (playerCount == ZombieCount)
        {
            // EndGameUi();
            EndGameSceneChange();
        }
    }

    // private void EndGameUi()
    // {
    //     endUi.SetActive(true);
    //     EndGameSceneChange().Forget();
    // }

    // private async UniTaskVoid EndGameSceneChange()
    // {
    //     await UniTask.Delay(5000);
    //     cts?.Cancel();
    //     GameManager.Instance.ChangeToWatingScene();
    // }

    private void EndGameSceneChange()
    {
        cts?.Cancel();
        GameManager.Instance.ChangeToWatingScene();
    }

    public void JoinPlayer(Net_PlayerController player)
    {
        if (!players.Contains(player))
        {
            print("join");
            players.Add(player);
        }
    }

    public void LeftPlayer(Net_PlayerController player)
    {
        if (players.Contains(player))
        {
            print("left");
            players.Remove(player);
        }
    }
}
