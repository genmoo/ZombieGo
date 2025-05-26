using Fusion;
using UnityEngine;
using System.Collections.Generic;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    public GameObject PlayerPrefab;

    public void PlayerJoined(PlayerRef player)
    {
        if (player == Runner.LocalPlayer)
        {
            Runner.Spawn(PlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity);
        }

        WaitingMapManager.Instance.playerCount++;
        Debug.Log($"플레이어 입장: {player} / 현재 인원: {WaitingMapManager.Instance.playerCount}");
    }


    public void PlayerLeft(PlayerRef player)
    {
        WaitingMapManager.Instance.playerCount--;
        Debug.Log($"플레이어 퇴장: {player} / 현재 인원: {WaitingMapManager.Instance.playerCount}");
    }
    
    public void SceneLoadSp()
    {
        print("0");
        if (!Runner.IsServer) return;
        print("1");
        foreach (var player in Runner.ActivePlayers)
        {
            Runner.Spawn(PlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity, player);
        }
    }

}
