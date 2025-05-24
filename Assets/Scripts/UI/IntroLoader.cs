using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro 사용

public class IntroLoader : MonoBehaviour
{
    [Header("로딩에 걸릴 시간 (초)")]
    public float totalLoadTime = 3f;

    [Header("다음 씬 이름")]
    public string nextSceneName = "Lobby";

    [Header("UI")]
    public Slider loadingSlider;
    public TextMeshProUGUI loadingText; // <- 여기 수정됨

    private float timer = 0f;

    void Start()
    {
        if (loadingSlider != null)
        {
            loadingSlider.minValue = 0;
            loadingSlider.maxValue = 100;
            loadingSlider.value = 0;
        }

        if (loadingText != null)
        {
            StartCoroutine(AnimateDots());
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        float percent = Mathf.Clamp01(timer / totalLoadTime);
        if (loadingSlider != null)
        {
            loadingSlider.value = percent * 100f;
        }

        if (timer >= totalLoadTime)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    System.Collections.IEnumerator AnimateDots()
    {
        string baseText = "게임에 필요한 데이터를 로딩중입니다";
        int dotCount = 0;

        while (true)
        {
            loadingText.text = baseText + new string('.', dotCount);
            dotCount = (dotCount + 1) % 4;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
