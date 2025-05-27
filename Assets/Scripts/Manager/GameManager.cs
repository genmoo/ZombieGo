using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;

public class GameManager : MonoBehaviour
{
    public enum SceneName
    {
        Lobby,
        WaitingRoom,
        LYS_NightClass
    }
    public List<CanvasGroup> Fade_img;
    public float delay = 3f;
    private string currentSceneName;
    public static GameManager Instance { get; private set; }

    // 네트워크
    public NetworkRunner runner;
    public NetworkSceneManagerDefault sceneManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            DestroyImmediate(this.gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        runner = GetComponent<NetworkRunner>();
        sceneManager = GetComponent<NetworkSceneManagerDefault>();
    }

    // 스타트 함수
    public void ChangeToNextScene()
    {
        SceneName current = GetCurrentSceneEnum();
        SceneName next = current switch
        {
            SceneName.Lobby => SceneName.WaitingRoom,
            SceneName.WaitingRoom => SceneName.LYS_NightClass,
            SceneName.LYS_NightClass => SceneName.Lobby,
            _ => SceneName.Lobby
        };
        StartCoroutine(LoadScene(next));
    }

    public static SceneName GetCurrentSceneEnum()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (System.Enum.TryParse(sceneName, out SceneName result))
        {
            return result;
        }
        else
        {
            Debug.LogError("씬 이름과 enum 값이 일치하지 않음: " + sceneName);
            return SceneName.Lobby;
        }
    }

    IEnumerator LoadScene(SceneName target)
    {
        if (target == SceneName.LYS_NightClass)
        {
            Fade_img[1].blocksRaycasts = true;
            Fade_img[1].alpha = 1f;
        }
        else
        {
            Fade_img[0].blocksRaycasts = true;
            Fade_img[0].alpha = 1f;
        }
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(target.ToString());
    }

    // 로드 된후 로딩 이미지 끄기기
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == SceneName.LYS_NightClass.ToString())
        {
            Fade_img[1].blocksRaycasts = false;
            Fade_img[1].alpha = 0f;
        }
        else
        {
            Fade_img[0].blocksRaycasts = false;
            Fade_img[0].alpha = 0f;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //  어디서든 로비로 가니깐 항상 셧다운 시켜야함 >> 그리고 러너 재생성성
    public void ChangeToLobbyScene()
    {
        Fade_img[0].blocksRaycasts = true;
        Fade_img[0].alpha = 1f;

        StartCoroutine(LoadScene(SceneName.Lobby));
    }

    // 네트워크
     public void CH()
    {
      

        var args = new StartGameArgs()
        {
            GameMode = GameMode.Shared, // Shared 모드
            SessionName = "Test",
            Scene = SceneRef.FromIndex(1), // 처음 대기방 씬
            SceneManager = sceneManager
        };

        runner.StartGame(args);
    }
}