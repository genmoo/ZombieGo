using UnityEngine;
using UnityEngine.Tilemaps;
using Fusion;
using TMPro;


public class WaitingMapManager : NetworkBehaviour
{
    public static WaitingMapManager Instance;
    public Grid grid;
    public Tilemap wallTilemap;
    [SerializeField] private GameObject endUi;
    [SerializeField] private TMP_Text player;
    [SerializeField] private TMP_Text start;

    public int playerCount { get; set; }
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
        playerCount = 0;
    }

    public void EndUi()
    {
        endUi.SetActive(true);
    }


    public void PlayerJoin()
    {

        playerCount++;
        Count();
    }

    public void PlayerLeft()
    {

        playerCount--;
        Count();
    }

    public void Count()
    {
        player.text = $"감염 {playerCount}/8";
        startButtonSet();
    }


    private void startButtonSet()
    {
        start.text = Runner.IsSharedModeMasterClient ? "게임 시작" : "대기중";
    }

    private void Update()
    {
        print(playerCount);
        // Count();
    }
}
