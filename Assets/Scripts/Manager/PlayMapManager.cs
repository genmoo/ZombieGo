using UnityEngine;
using UnityEngine.Tilemaps;
using Fusion;
using TMPro;
using Cysharp.Threading.Tasks;

public class PlayMapManager : NetworkBehaviour
{
    public static PlayMapManager Instance;

    public Grid grid;
    public Tilemap wallTilemap;
    public int playerCount = 0;

    // Networked timer
    [Networked]
    private TickTimer timer { get; set; }

    [SerializeField]
    private TMP_Text timerText;

    [SerializeField]
    private float timerDuration = 70f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void Spawned()
    {
        print("Sd");
        if (HasStateAuthority)
        {
            timer = TickTimer.CreateFromSeconds(Runner, timerDuration);
        }

        UpdateTimerLoop().Forget();
    }

    private async UniTaskVoid UpdateTimerLoop()
    {
        while (true)
        {
            float remaining = timer.RemainingTime(Runner) ?? 0f;

            int min = Mathf.FloorToInt(remaining / 60f);
            int sec = Mathf.FloorToInt(remaining % 60f);

            if (timerText != null)
                timerText.text = $"{min:00}:{sec:00}";

            if (remaining <= 0f)
                break;

            await UniTask.Delay(500); // 0.5초 간격
        }

        if (timerText != null)
            timerText.text = "00:00";

        Debug.Log("타이머 종료");
    }
}
