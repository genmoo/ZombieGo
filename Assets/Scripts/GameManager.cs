using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    // 씬 이름
    public enum SceneName
    {
        Lobby,
        WaitingRoom,
        LYS_NightClass
    }
    
    public CanvasGroup Fade_img;
    public float delay = 3f;

    private static GameManager instance;
    private string currentSceneName;

    public static GameManager Instance { get; private set; }




    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            DestroyImmediate(this.gameObject);
            return;
        }
        instance = this;

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    public void GoTo(SceneName target)
    {
        Fade_img.blocksRaycasts = true;
        Fade_img .alpha = 1f;
        
        // SceneManager.LoadScene(target.ToString());
        StartCoroutine(LoadScene(target));
    }

    IEnumerator LoadScene(SceneName target)
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(target.ToString());
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

        GoTo(next);
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


    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Fade_img.blocksRaycasts = false;
        Fade_img.alpha = 0f;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}