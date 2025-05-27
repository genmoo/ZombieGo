using UnityEngine;
using UnityEngine.Tilemaps;
using Fusion;
using UnityEngine.SceneManagement;


public class PlayMapManager : MonoBehaviour
{
    public static PlayMapManager Instance;
    public Grid grid;
    public Tilemap wallTilemap;
    // public NetworkRunner runnerPrefab;
    public int playerCount = 0;
    // private string[] persistentScenes = { "WaitingRoom", "LYS_NightClass" };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // string currentScene = SceneManager.GetActiveScene().name;

        // foreach (var sceneName in persistentScenes)
        // {
        //     if (currentScene == sceneName)
        //     {
        //         DontDestroyOnLoad(gameObject);
        //         return;
        //     }
        // }

        // Destroy(gameObject);

    }
    
    

    // async void Start()
    // {
    //     var runner = Instantiate(runnerPrefab);
    //     runner.ProvideInput = true;

    //     await runner.StartGame(new StartGameArgs
    //     {
    //         GameMode = GameMode.Shared,
    //         SessionName = "TestRoom",
    //         SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
    //     });
    // }

    // void Update()
    // {
    //     print(playerCount);
    // }
}

