using UnityEngine;
using UnityEngine.Tilemaps;
using Fusion;

public class WaitingMapManager : MonoBehaviour
{
    public static WaitingMapManager Instance;
    public Grid grid;
    public Tilemap wallTilemap;
    public NetworkRunner runnerPrefab;
    public int playerCount = 0;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    async void Start()
    {
        var runner = Instantiate(runnerPrefab);
        runner.ProvideInput = true;

        await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = "Test",
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
    }
}

