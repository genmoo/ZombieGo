using System.Collections;
using Cysharp.Threading.Tasks;
using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerSpawner : SimulationBehaviour
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // StartCoroutine("SpawnAfterDelay");
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




 // onBeforeSpawned: (runner, obj) =>
            // {
            //     obj.GetComponent<Net_PlayerController>().SceneGroupId = SceneManager.GetActiveScene().buildIndex;
            //     print(SceneManager.GetActiveScene().buildIndex);
            // });
// }
// IPlayerJoined, IPlayerLeft
//     public void PlayerJoined(PlayerRef player)
//     {
//         if (player == Runner.LocalPlayer)
//         {
//             Runner.Spawn(PlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity, Runner.LocalPlayer,
//    onBeforeSpawned: (runner, obj) =>
//    {
//        obj.GetComponent<Net_PlayerController>().SceneGroupId = 3;
//    });
//         }

//         WaitingMapManager.Instance.playerCount++;
//         Debug.Log($"플레이어 입장: {player} / 현재 인원: {WaitingMapManager.Instance.playerCount}");
//     }

//     public void PlayerLeft(PlayerRef player)
//     {
//         WaitingMapManager.Instance.playerCount--;
//         Debug.Log($"플레이어 퇴장: {player} / 현재 인원: {WaitingMapManager.Instance.playerCount}");
//     }
