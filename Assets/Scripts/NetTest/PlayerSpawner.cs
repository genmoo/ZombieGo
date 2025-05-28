using System.Collections;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine("SpawnAfterDelay");
    }
    
    private IEnumerator SpawnAfterDelay()
{
    // 0.1~0.5초 정도 기다려서 프리팹 로딩 시간 확보
    yield return new WaitForSeconds(0.2f);

    if (Runner != null && Runner.IsRunning)
    {
        Runner.Spawn(PlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity);
    }
}
}
