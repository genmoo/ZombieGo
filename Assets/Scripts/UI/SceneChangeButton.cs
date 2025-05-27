using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class SceneChangeButton : MonoBehaviour
{
    private Button btn;

    public enum SceneChangeType
    {
        ChangeToNextScene,
        ChangeToLobbyScene,
    }

    [SerializeField]
    private SceneChangeType selectedSceneChange;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(() => {
            CallSceneChangeFunction(selectedSceneChange);
        });
    }

    private void CallSceneChangeFunction(SceneChangeType type)
    {
        switch (type)
        {
            case SceneChangeType.ChangeToNextScene:
                GameManager.Instance.ChangeToNextScene();
                break;
            case SceneChangeType.ChangeToLobbyScene:
                GameManager.Instance.ChangeToLobbyScene();
                break;
        }
    }
}
