using Fusion;
using UnityEngine;
using TMPro;

public class NetworkPlayerNickName : NetworkBehaviour
{
    [Networked, OnChangedRender(nameof(OnNicknameChanged))]
    public string Nickname { get; set; }

    [SerializeField] private TMP_Text nicknameText;

    private  void OnNicknameChanged()
    {
        UpdateNicknameDisplay();
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            string myNickname = PlayerPrefs.GetString("nickname", "플레이어 이름름");
            RPC_SetNickname(myNickname);
        }

        UpdateNicknameDisplay();
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void RPC_SetNickname(string nickname)
    {
        Nickname = nickname;
    }

    private void UpdateNicknameDisplay()
    {
        if (nicknameText != null)
            nicknameText.text = Nickname;
    }
}
