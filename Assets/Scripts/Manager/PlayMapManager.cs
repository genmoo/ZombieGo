using UnityEngine;
using UnityEngine.Tilemaps;
using Fusion;
using UnityEngine.SceneManagement;


public class PlayMapManager : MonoBehaviour
{
    public static PlayMapManager Instance;
    public Grid grid;
    public Tilemap wallTilemap;
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

