using Fusion;
using UnityEngine;
public class PlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    public GameObject PlayerPrefab;
    private NetworkRunner runner;

    void Awake()
    {
        runner = GetComponent<NetworkRunner>();
    }

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

    // public void SceneLoadSpawn()
    // {
    //     if (runner == null)
    //     {
    //         Debug.LogWarning("Runner is null");
    //         return;
    //     }

    //     foreach (PlayerRef player in runner.ActivePlayers)
    //     {
    //         Runner.Spawn(PlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity, player);
    //         PlayMapManager.Instance.playerCount++;
    //         Debug.Log($"플레이어 입장: {player} / 현재 인원: {PlayMapManager.Instance.playerCount}");
    //     }
    // }
    
    // 이거 해야힘ㅋㅋㅋ
}
