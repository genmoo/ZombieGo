using UnityEngine;
using UnityEngine.Tilemaps;
using Fusion;
using TMPro;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;


public class WaitingMapManager : NetworkBehaviour
{
    public static WaitingMapManager Instance;
    public Grid grid;
    public Tilemap wallTilemap;
    [SerializeField] private GameObject endUi;
    [SerializeField] private TMP_Text player;
    [Networked] public int playerCount{ get; set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        endUi.SetActive(false);
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

