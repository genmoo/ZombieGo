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
    [Networked] public int playerCount{ get; set; }
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

        if (Runner.IsSharedModeMasterClient)
        {
            start.text = $"게임 시작";
        }
        else
        {
            start.text = $"대기중";
        }
    }

    public void EndUi()
    {
        endUi.SetActive(true);
    }

    public void PlayerJoin()
    {
            playerCount++;
            player.text = $"감염 {playerCount}/8";
    }
    
    public void PlayerLeft()
    {
            playerCount--;
            player.text = $"감염 {playerCount}/8";
    }
}

