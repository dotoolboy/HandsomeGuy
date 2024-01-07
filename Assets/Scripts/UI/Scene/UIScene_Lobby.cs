using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Vivox;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Progress;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class UIScene_Lobby : UIScene, IChatable
{
    #region enums
    private enum Objects
    {
        SceneLobbyScale,
        Users,
        ChatRect,
        ChatItems,
        InputField,
    }
    private enum Buttons
    {
        StartButton,
        ReadyButton,
        ExitButton,
    }
    //private enum Images
    //{
    //    User_1_Img,
    //    User_2_Img,
    //    User_3_Img,
    //    User_4_Img,
    //    User_5_Img,
    //    User_6_Img,
    //}
    //private enum Texts
    //{
    //    User_1_Nickname,
    //    User_2_Nickname,
    //    User_3_Nickname,
    //    User_4_Nickname,
    //    User_5_Nickname,
    //    User_6_Nickname,
    //    User_1_SkillList,
    //    User_2_SkillList,
    //    User_3_SkillList,
    //    User_4_SkillList,
    //    User_5_SkillList,
    //    User_6_SkillList,
    //}
    #endregion

    #region Fields

    private TMP_InputField _inputField;
    private Transform _userPos;
    private Transform _textPos; 
    private ScrollRect _chatRect;

    private List<User> Users = new();
    private User _me;
    #endregion

    async void Start()
    {
        await Main.VivoxManager.InitializeAsync();
        await Main.VivoxManager.LoginToVivoxAsync();
        await Main.VivoxManager.JoinGroupChannelAsync();
    }
    public override bool Init()
    {
        if (!base.Init()) return false;

        BindButton(typeof(Buttons), true);
        //BindImage(typeof(Images), true);
        //BindText(typeof(Texts), true);
        BindObject(typeof(Objects),true);


        Main.UIManager.Appear(GetObject((int)Objects.SceneLobbyScale).gameObject);
        _inputField = GetObject((int)Objects.InputField).GetComponent<TMP_InputField>();
        _userPos = GetObject((int)Objects.Users).GetComponent<Transform>();
        _textPos = GetObject((int)Objects.ChatItems).GetComponent<Transform>();
        _chatRect = GetObject((int)Objects.ChatRect).GetComponent<ScrollRect>();


        _inputField.onEndEdit.AddListener((string text) => { EnterKeyOnTextField(); });

        if (PhotonNetwork.IsMasterClient)
        {
            GetButton((int)Buttons.StartButton).gameObject.SetActive(true);
            AddUIEvent(GetButton((int)Buttons.StartButton).gameObject, OnButtonStart);
        }
        AddUIEvent(GetButton((int)Buttons.ReadyButton).gameObject, OnButtonReady);
        AddUIEvent(GetButton((int)Buttons.ExitButton).gameObject, OnButtonExitAsync);

        return true;
    }

    private void OnButtonStart(PointerEventData data)
    {
        print("시작 버튼");
        if (IsAllReady())
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel("testScene");
            }
        }
        else
        {
            print("아직 모든 유저가 준비하지 않았습니다.");
        }
    }

    private void OnButtonReady(PointerEventData data)
    {
        print("준비 버튼");
        Main.VivoxManager.OnReady();
    }
    public void ReadyCheck(VivoxMessage message)
    {
        User sender = Users.FirstOrDefault(p => p.Participant.PlayerId == message.SenderPlayerId);
        sender.ToggleReady();        
    }
    private void OnButtonExitAsync(PointerEventData data)
    {
        print("나가기 버튼");
        StartCoroutine(CoExit());
    }
    private async Task OnExitAsync()
    {
        await Main.VivoxManager.LeaveEchoChannelAsync();
        await Main.VivoxManager.LogoutOfVivoxAsync();
    }
    void EnterKeyOnTextField()
    {
        if (!Input.GetKeyDown(KeyCode.Return))
        {
            return;
        }
        SendMessage();
    }
    public void SendMessage()
    {
        if (string.IsNullOrEmpty(_inputField.text))
        {
            return;
        }
        Main.VivoxManager.SendChatMessage(_inputField.text);
        ClearTextField();
        SetScrollToBottom();
    }
    void ClearTextField()
    {
        _inputField.text = string.Empty;
        _inputField.Select();
        _inputField.ActivateInputField();
    }
    public void SetScrollToBottom()
    {
        StartCoroutine(SendScrollRectToBottom());
    }
    public IEnumerator SendScrollRectToBottom()
    {
        yield return new WaitForEndOfFrame();

        _chatRect.normalizedPosition = new Vector2(0, 0);

        yield return null;
    }

    public void InputUser(VivoxParticipant participant)
    {
        var tmp = Main.ResourceManager.Instantiate("User.prefab", _userPos);        
        User newItem = tmp.GetOrAddComponent<User>();
        if (participant.IsSelf)
            _me = newItem;
        Users.Add(newItem);

        participant.SetLocalVolume(0);
        newItem.SetupItem(participant);
    }
    public void DeleteUser(VivoxParticipant participant)
    {
        User removedItem = Users.FirstOrDefault(p => p.Participant.PlayerId == participant.PlayerId);
        if (removedItem != null)
        {
            Users.Remove(removedItem);
            Destroy(removedItem.gameObject);
        }
    }
    public void InputChat(string str)
    {
        var tmp = Main.ResourceManager.Instantiate("ChatItem.prefab", _textPos);
        tmp.GetComponentInChildren<TMP_Text>().text = str;
    }
    public bool IsAllReady()
    {
        if (Users.All(p => p.ready.activeSelf))
            return true;
        else
            return false;
    }
    IEnumerator CoExit()
    {
        Main.UIManager.Hide(GetObject((int)Objects.SceneLobbyScale).gameObject);
        OnExitAsync();
        yield return new WaitForSeconds(1.0f);
        Main.SceneManagerEx.LoadScene(Define.Scene.StartScene);

    }

}
