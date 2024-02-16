using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Chat;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using ExitGames.Client.Photon;
//THIS SCRIPT IS FOR MANAGING THE CHAT AND SENDING MESSAGES
//refered from this youtube playlist : https://www.youtube.com/watch?v=6I2aAwpfTh0&list=PLWeGoBm1YHVjY3vZ2qMFb95rAmyiVtBdz&ab_channel=InfoGamer
public class PhotonChatManager : MonoBehaviourPunCallbacks, IChatClientListener
{
    [HideInInspector]
    public ChatClient chatClient;
    [SerializeField]
    string userID, chatRoomName;

    [SerializeField] TMP_Text msgInputText, chatArea;
    [SerializeField] TMP_InputField msgInputField;
    [SerializeField] PhotonView PV;
    public static PhotonChatManager instance;
    void Start()
    {
        instance = this;
        if (string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat))
        {
            Debug.LogError("You need to set the chat app ID in the PhotonServerSettings file in order to continue.");
            return;
        }
        chatRoomName = PhotonNetwork.CurrentRoom.Name;
        chatClient = new ChatClient(this);
        chatClient.PrivateChatHistoryLength = 0;
        chatClient.UseBackgroundWorkerForSending = true;
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.AppVersion, new AuthenticationValues(PhotonNetwork.NickName));
    }
    public void DisconnectChat()
    {
        chatClient.Unsubscribe(new string[] { chatRoomName });
        chatClient.Disconnect();
    }
    public void DebugReturn(DebugLevel level, string message)
    {
    }

    public void OnChatStateChange(ChatState state)
    {

    }
    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        DisconnectChat();
    }
    public void OnConnected()
    {
        this.chatClient.Subscribe(new string[] { chatRoomName });
        this.chatClient.SetOnlineStatus(ChatUserStatus.Online);
    }

    public void OnDisconnected()
    {
        if (chatClient != null) 
        {
            chatClient.UseBackgroundWorkerForSending = false;
            chatClient.Disconnect(); 
        }
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        string msgs = "";
        for (int i = 0; i < senders.Length; i++)
        {
            msgs = string.Format("{0}{1}={2}, ", msgs, senders[i], messages[i]);
            //chatArea.text += senders[i] + " : " + messages[i]+ '\n';
            chatArea.text += ""+ messages[i] + '\n';
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        // Debug.Log("Subscribed to a new channel - set online status!" + results);
        //foreach (string channel in channels)
        //{
        //    this.chatClient.PublishMessage(channel, "joined");
        //    string s = chatArea.text;
        //    if(PhotonNetwork.IsMasterClient)
        //        PV.RPC("SendJoinedMessageToOthers", RpcTarget.OthersBuffered,s);
        //}
    }

    //this function is used for sending messages
    public void SendAnswerToChat(string answer)
    {
        //string temp = "answered " + answer;
        this.chatClient.PublishMessage(chatRoomName, answer);
    }
    public void OnUnsubscribed(string[] channels)
    {
    }

    public void OnUserSubscribed(string channel, string user)
    {
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
    }
    private void OnApplicationQuit()
    {
        if (chatClient != null) { chatClient.Disconnect(); }
    }
    // Update is called once per frame
    void Update()
    {
        if(this.chatClient!=null)
            this.chatClient.Service();
    }
}
