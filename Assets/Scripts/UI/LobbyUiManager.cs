using UnityEngine;

public class LobbyUiManager : MonoBehaviour
{
    [SerializeField]
    private GameObject exitUi;
    private GameObject creatRoomUi;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            exitUi.SetActive(!exitUi.activeSelf);
        }
    }

    public void OpenExitUI()
    {
        exitUi.SetActive(true);
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
