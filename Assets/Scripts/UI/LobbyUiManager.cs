using UnityEngine;
using Fusion;

public class LobbyUiManager : NetworkBehaviour
{
    [SerializeField]
    private GameObject exitUi;
    [SerializeField]
    private GameObject creatRoomUi;
    [SerializeField]
    private NetworkRunner GameManager;

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
