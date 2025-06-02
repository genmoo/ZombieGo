using UnityEngine;
using UnityEngine.Tilemaps;
using Fusion;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;


public class PlayMapManager : NetworkBehaviour
{
    public static PlayMapManager Instance;
    public Grid grid;
    public Tilemap wallTilemap;
    public int playerCount = 0;

    // timer
    [SerializeField] private TMP_Text text;

    [SerializeField] private float time;
    [SerializeField] private float curTime;

    private NetworkRunner Runner;

    int minute;
    int second;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (Runner == null)
            Runner = FindObjectOfType<NetworkRunner>();
    }

    private void Start()
    {
        if (Runner.IsSharedModeMasterClient)
        {
            Invoke("StartTimer", 3f);
        }
    }

    public void StartTimer()
    {
        time = 70;
        Timer().Forget();
    }
    
    private async UniTask Timer()
    {
        curTime = time;
        while (curTime > 0)
        {
            curTime -= Time.deltaTime;
            minute = (int)curTime / 60;
            second = (int)curTime % 60;
            text.text = minute.ToString("00") + ":" + second.ToString("00");
            await UniTask.Yield();
        }
        Debug.Log("시간 종료");
        curTime = 0;
    }
}



