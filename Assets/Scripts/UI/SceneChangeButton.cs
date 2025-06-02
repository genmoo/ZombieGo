using UnityEngine;
using UnityEngine.UI;

public class SceneChangeButton : MonoBehaviour
{
    private Button btn;

    public enum SceneChangeType
    {
        ChangeToNextScene,
        ChangeToLobbyScene,
        ChangeToWatingScene,
        ChangeToGameScene
    }

    [SerializeField]
    private SceneChangeType selectedSceneChange;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(() =>
        {
            CallSceneChangeFunction(selectedSceneChange);
        });
    }

    private void CallSceneChangeFunction(SceneChangeType type)
    {
        switch (type)
        {
            case SceneChangeType.ChangeToLobbyScene:
                GameManager.Instance.ChangeToLobbyScene();
                break;
            case SceneChangeType.ChangeToWatingScene:
                GameManager.Instance.ChangeToWatingScene();
                break;
            case SceneChangeType.ChangeToGameScene:
                GameManager.Instance.ChangeToGameScene();
                break;
        }
    }
}
