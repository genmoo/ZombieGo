using UnityEngine;
using UnityEngine.Tilemaps;
using Fusion;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    public Grid grid;
    public Tilemap wallTilemap;
    public NetworkRunner runnerPrefab;

    void Awake()
    {
        Instance = this;

    }

//     async void Start()
//     {
//         var runner = Instantiate(runnerPrefab);
//         runner.ProvideInput = true;

//         await runner.StartGame(new StartGameArgs
//         {
//             GameMode = GameMode.Shared,
//             SessionName = "TestRoom",
//             SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
//         });
//     }
}

