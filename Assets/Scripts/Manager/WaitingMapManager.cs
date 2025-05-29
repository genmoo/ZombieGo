using UnityEngine;
using UnityEngine.Tilemaps;

public class WaitingMapManager : MonoBehaviour
{
    public static WaitingMapManager Instance;
    public Grid grid;
    public Tilemap wallTilemap;
    // public NetworkRunner runnerPrefab;
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
}

