using UnityEngine;
using TMPro;

public class NicknameUI : MonoBehaviour
{
    public TMP_InputField nicknameInput;
    string saved;


    private void Start()
    {
        nicknameInput.onSubmit.AddListener(OnConfirmButton);
    }
    
    public void OnConfirmButton(string input)
    {
        string nickname = nicknameInput.text.Trim();

        if (!string.IsNullOrEmpty(nickname))
        {
            PlayerPrefs.SetString("nickname", nickname);
            saved = PlayerPrefs.GetString("nickname", "저장안됨");
        }
    }

    public void Update()
    {
        Debug.Log("저장된 닉네임: " + saved);
    }
}
