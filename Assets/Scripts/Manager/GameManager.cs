using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Fusion;

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
    public NetworkRunner runnerPrefab;

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
            SceneManager.LoadScene(target.ToString());
        }
        else if (target == SceneName.WaitingRoom)
        {
            Fade_img[0].blocksRaycasts = true;
            Fade_img[0].alpha = 1f;

            yield return new WaitForSeconds(delay);

            if (runner != null && runner.IsRunning)
            {
                //이미 러너가 실행 중이면 씬만 이동
                Debug.Log("[Fusion] 러너 실행 중, StartGame 생략하고 씬만 전환");
                SceneManager.LoadScene(target.ToString());
            }
            else
            {
                //러너가 없거나 종료된 상태면 StartGame 호출 (Fusion이 씬도 관리함)
                var args = new StartGameArgs()
                {
                    GameMode = GameMode.Shared,
                    SessionName = "Test",
                    Scene = SceneRef.FromIndex(1), // WaitingRoom 씬 인덱스
                    SceneManager = sceneManager
                };

                runner.StartGame(args);

            }
        }
        else
        {
            Fade_img[0].blocksRaycasts = true;
            Fade_img[0].alpha = 1f;

            var player = GameObject.FindWithTag("Player");
            if (player != null)
                Destroy(player);

            yield return new WaitForSeconds(delay);
            yield return SceneManager.LoadSceneAsync(target.ToString());
            OnGameEndButton();
        }

    }

    // 로드 된후 로딩 이미지 끄기기
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Fade_img[0].blocksRaycasts = false;
        Fade_img[0].alpha = 0f;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //  어디서든 로비로 가니깐 항상 셧다운 시켜야함 >> 그리고 러너 재생성성
    public void ChangeToLobbyScene()
    {
        StartCoroutine(LoadScene(SceneName.Lobby));
    }

    public void ChangeToWatingScene()
    {
        StartCoroutine(LoadScene(SceneName.WaitingRoom));
    }

    public async void OnGameEndButton()
    {
        if (runner != null)
        {
            await runner.Shutdown();
            // Destroy(runner.gameObject);
        }
    }


    public void OnStartGameButton()
    {
        if (runner.IsSceneAuthority)
        {
            runner.LoadScene(SceneRef.FromIndex(2), LoadSceneMode.Single);
        }
    }
    
}