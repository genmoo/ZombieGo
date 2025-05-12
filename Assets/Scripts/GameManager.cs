using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CanvasGroup Fade_img;
    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }
    private static GameManager instance;

    void Start()
    {
        if (instance != null)
        {
            DestroyImmediate(this.gameObject);
            return;
        }
        instance = this;

        DontDestroyOnLoad(gameObject);
    }
}
