using System.Collections;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerSpawner : SimulationBehaviour, IPlayerLeft
{
    public GameObject PlayerPrefab;

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
        SpawnAfterDelay().Forget();
    }

    private async UniTask SpawnAfterDelay()
    {
        await UniTask.DelayFrame(1);
        if (Runner != null && Runner.IsRunning)
        {
            Runner.Spawn(PlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity, Runner.LocalPlayer);
        }
    }
}
