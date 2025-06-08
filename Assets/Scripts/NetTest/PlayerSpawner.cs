using System.Collections;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : SimulationBehaviour, IPlayerLeft
{
    public GameObject PlayerPrefab;

    private bool hasSpawned = false;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void PlayerLeft(PlayerRef player)
    {
        WaitingMapManager.Instance.PlayerLeft();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        hasSpawned = false; // 씬 바뀌었으니 다시 스폰할 수 있게 초기화
        SpawnAfterDelay().Forget();
    }

    private async UniTask SpawnAfterDelay()
    {
        await UniTask.Delay(500);

        if (hasSpawned) return;

        if (Runner != null && Runner.IsRunning)
        {
            Runner.Spawn(PlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity, Runner.LocalPlayer);
            hasSpawned = true;
        }
    }

}
