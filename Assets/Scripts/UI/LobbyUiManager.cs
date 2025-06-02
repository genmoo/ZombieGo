using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;
using System.Collections;

public class LobbyUiManager : MonoBehaviour
{
    [SerializeField]
    private GameObject exitUi;
    [SerializeField]
    private GameObject creatRoomUi;
    [SerializeField]
    private NetworkRunner GameManager; 
    private void Awake()
    {
        SceneManager.sceneLoaded += SpawnRunnder;
    }

    private void SpawnRunnder(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine("DelayRunner");
    }

    IEnumerator DelayRunner()
    {
        yield return new WaitForSeconds(0.2f);
        if (FindObjectOfType<NetworkRunner>() == null)
        {
            var runner = Instantiate(GameManager);
        }
    }

     void OnDestroy()
    {
        SceneManager.sceneLoaded -= SpawnRunnder;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (creatRoomUi.activeSelf)
            {
                creatRoomUi.SetActive(false);
            }
            else
            {
                exitUi.SetActive(!exitUi.activeSelf);
            }

        }
    }

    public void OpenExitUI()
    {
        if (!creatRoomUi.activeSelf)
        {
            exitUi.SetActive(!exitUi.activeSelf);
        }
        else
        {
            creatRoomUi.SetActive(false);
        }

    }

    public void CloseExitUI()
    {
        exitUi.SetActive(false);
    }

    public void OpenCrRoomUI()
    {
        creatRoomUi.SetActive(true);
    }

    public void CloseCrRoomUI()
    {
        creatRoomUi.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
