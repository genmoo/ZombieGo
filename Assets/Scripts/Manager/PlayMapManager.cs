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
    public List<PlayerController> players = new List<PlayerController>();
    public PlayerScoreboard playerScoreboard;
    // Networked timer
    [Networked] private TickTimer timer { get; set; }
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private float timerDuration = 70f;
    // Count Player
    private int ZombieCount = 0;
    [SerializeField] private TMP_Text countdownText;
    private CancellationTokenSource cts;

    [SerializeField] private GameObject ZombieEndUi;
    [SerializeField] private GameObject HumanEndUi;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        cts = new CancellationTokenSource();
    }

    public override void Spawned()
    {
        StartRound();
    }

    private void StartRound()
    {
        RandomChoiseZombie(cts.Token).Forget();
    }

    private async UniTaskVoid UpdateTimerLoop(CancellationToken token)
    {
        if (HasStateAuthority)
        {
            timer = TickTimer.CreateFromSeconds(Runner, timerDuration);
        }

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


        HumanEndUi.SetActive(true);
        // 시간으로 끝내기
        EndGameSceneChange();
    }

    private async UniTaskVoid RandomChoiseZombie(CancellationToken token)
    {
        countdownText.text = $"잠시뒤에 숙주좀비가 결정됩니다.\n서로 멀리 떨어지세요!";
        await UniTask.Delay(3000, cancellationToken: token);

        float countdown = 10f;

        while (countdown > 0)
        {
            int sec = Mathf.CeilToInt(countdown);
            countdownText.text = $"{sec}초 남았습니다!";
            await UniTask.Delay(800, cancellationToken: token);
            countdownText.text = $" ";
            await UniTask.Delay(200, cancellationToken: token);
            countdown -= 1f;
        }

        countdownText.text = "좀비 선택 중...";
        // Shared 모드 마스터 클라이언트만 실행
        if (Runner.IsSharedModeMasterClient)
        {
            if (players.Count == 0) return;

            int randomIndex = Random.Range(0, players.Count);
            PlayerController chosenPlayer = players[randomIndex];

            // RPC 호출해서 모든 클라이언트가 동기화되도록
            chosenPlayer.RPC_SetAsZombie();
        }

        await UniTask.Delay(1000, cancellationToken: token);
        countdownText.text = "";

        UpdateTimerLoop(cts.Token).Forget();
    }

    public void AddZombie()
    {
        ZombieCount++;
        print("dasfg");
        EndGame();
    }

    private void EndGame()
    {
        int playerCount = players.Count;
        if (playerCount == ZombieCount)
        {
            ZombieEndUi.SetActive(true);
            EndGameSceneChange();
        }
    }


    private void EndGameSceneChange()
    {
        cts?.Cancel();

        var player = GameObject.FindWithTag("Player");
        if (player != null)
            Destroy(player);

        GameManager.Instance.ChangeToWatingScene();
    }

    public void JoinPlayer(PlayerController player)
    {
        if (!players.Contains(player))
        {
            print("join");
            players.Add(player);
        }
    }

    public void LeftPlayer(PlayerController player)
    {
        if (players.Contains(player))
        {
            print("left");
            players.Remove(player);
        }
    }
}
