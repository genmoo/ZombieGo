using UnityEngine;

public class LobbyUiManager : MonoBehaviour
{
    [SerializeField]
    private GameObject uiRoot;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            uiRoot.SetActive(!uiRoot.activeSelf);
        }
    }

    public void OpenUI()
    {
        uiRoot.SetActive(true);
    }

    public void CloseUI()
    {
        uiRoot.SetActive(false);
    }

    public void QuitGame()
{
    Application.Quit();
}
}
