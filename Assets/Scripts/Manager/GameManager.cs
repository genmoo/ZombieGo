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
    public static GameManager Instance { get; private set; }
    // 네트워크
    public NetworkRunner Runner;
    public NetworkSceneManagerDefault sceneManager;
    private string currentSceneName;
    [SerializeField]
    private float delay = 3f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            DestroyImmediate(this.gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        Runner = GetComponent<NetworkRunner>();
        sceneManager = GetComponent<NetworkSceneManagerDefault>();
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    IEnumerator LoadScene(SceneName target)
    {
        if (target == SceneName.LYS_NightClass)
        {
            if (Runner.IsSceneAuthority)
            {
                Runner.LoadScene(SceneRef.FromIndex(2), LoadSceneMode.Single);
            }
        }
        else if (target == SceneName.WaitingRoom)
        {
            Fade_img[0].blocksRaycasts = true;
            Fade_img[0].alpha = 1f;

            yield return new WaitForSeconds(delay);

            if (Runner != null && Runner.IsRunning)
            {
                 if (Runner.IsSceneAuthority)
                {
                    Runner.LoadScene(SceneRef.FromIndex(1), LoadSceneMode.Single);
                }
                // //이미 러너가 실행 중이면 씬만 이동
                // Debug.Log("[Fusion] 러너 실행 중, StartGame 생략하고 씬만 전환");
                // SceneManager.LoadScene(target.ToString());
                // 대기방을 러너 씬으로 만들고 rpc로 게임방으로 보내기 다시 rpc로 대기방으로 보내기 지금은 일단 전부 보내는 방식으로 해둠 rpc 하기 귀찮아서
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
                Runner.StartGame(args);
            }
        }
        else if (target == SceneName.Lobby)
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

    // 씬 이동 버튼 함수
    public void ChangeToLobbyScene()
    {
        StartCoroutine(LoadScene(SceneName.Lobby));
    }

    public void ChangeToWatingScene()
    {
        StartCoroutine(LoadScene(SceneName.WaitingRoom));
    }

    public void ChangeToGameScene()
    {
        StartCoroutine(LoadScene(SceneName.LYS_NightClass));
    }
    // 셧 다운
    public void OnGameEndButton()
    {
        if (Runner != null)
        {
            Runner.Shutdown();
            Destroy(Runner.gameObject);
        }
    }
}












// 스타트 함수
// public void ChangeToNextScene()
// {
//     SceneName current = GetCurrentSceneEnum();
//     SceneName next = current switch
//     {
//         SceneName.Lobby => SceneName.WaitingRoom,
//         SceneName.WaitingRoom => SceneName.LYS_NightClass,
//         SceneName.LYS_NightClass => SceneName.Lobby,
//         _ => SceneName.Lobby
//     };
//     StartCoroutine(LoadScene(next));
// }

// private static SceneName GetCurrentSceneEnum()
// {
//     string sceneName = SceneManager.GetActiveScene().name;

//     if (System.Enum.TryParse(sceneName, out SceneName result))
//     {
//         return result;
//     }
//     else
//     {
//         Debug.LogError("씬 이름과 enum 값이 일치하지 않음: " + sceneName);
//         return SceneName.Lobby;
//     }
// }