using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisconnectRecovery : MonoBehaviourPunCallbacks
{
    [Tooltip("Whether or not attempt a rejoin without doing any checks.")]
    [SerializeField]
    private bool skipRejoinChecks;
    [Tooltip("Whether or not realtime webhooks are configured with persistence enabled")]
    [SerializeField]
    private bool persistenceEnabled;

    private bool rejoinCalled;

    private int minTimeRequiredToRejoin = 0; // TODO: set dynamically based on PhotonNetwork.NetworkingClient.LoadBalancingPeer.RoundTripTime

    private DisconnectCause lastDisconnectCause;
    private bool wasInRoom;

    private bool reconnectCalled;

    private bool pendingAction;

    [SerializeField] GameObject DisconnectedPanel;
    [SerializeField] TMPro.TMP_Text DCText;

    //public override void OnEnable()
    //{
       
    //    base.OnEnable();
    //    PhotonNetwork.NetworkingClient.StateChanged += this.OnStateChanged;
    //}

    //public override void OnDisable()
    //{
    //    base.OnDisable();
    //    PhotonNetwork.NetworkingClient.StateChanged -= this.OnStateChanged;
    //}


    //private void OnStateChanged(ClientState fromState, ClientState toState)
    //{
    //    if (toState == ClientState.Disconnected)
    //    {
    //        Debug.LogFormat("OnStateChanged from {0} to {1}, PeerState={2}", fromState, toState,
    //            PhotonNetwork.NetworkingClient.LoadBalancingPeer.PeerState);

    //        Debug.Log("Pending action raised");
    //        pendingAction = true;

    //    }
    //}

    //private void Update()
    //{
    //    if (pendingAction)
    //    {
    //        pendingAction = false;
    //        Debug.Log("handle disconnect now");
    //        this.HandleDisconnect();
    //    }
    //}

    public override void OnDisconnected(DisconnectCause cause)
    {
        //Debug.LogFormat("OnDisconnected(cause={0}) ClientState={1} PeerState={2}",
        //    cause,
        //    PhotonNetwork.NetworkingClient.State,
        //    PhotonNetwork.NetworkingClient.LoadBalancingPeer.PeerState);
        //if (rejoinCalled)
        //{
        //    Debug.LogError("Rejoin failed, client disconnected");
        //    rejoinCalled = false;
        //    return;
        //}

        //if (reconnectCalled)
        //{
        //    Debug.LogError("Reconnect failed, client disconnected");
        //    reconnectCalled = false;
        //    return;
        //}
        //lastDisconnectCause = cause;
        //wasInRoom = true;
        //if (PhotonNetwork.NetworkingClient.State == ClientState.Disconnected)
        //{
        //    Debug.Log("Pending action raised");
        //    pendingAction = true;
        //}

        LobbyController.selfDisconnected = true;
        if (PhotonNetwork.IsConnected)
            ResetDataOnDisconnect(PhotonNetwork.LocalPlayer);
    }

    //private void HandleDisconnect()
    //{
    //    switch (lastDisconnectCause)
    //    {
    //        case DisconnectCause.Exception:
    //        case DisconnectCause.ServerTimeout:
    //        case DisconnectCause.ClientTimeout:
    //        case DisconnectCause.DisconnectByServerLogic:
    //        case DisconnectCause.AuthenticationTicketExpired:
    //        case DisconnectCause.DisconnectByServerReasonUnknown:
    //            if (wasInRoom)
    //            {
    //                Debug.Log("CheckAndRejoin called");
    //                this.CheckAndRejoin();
    //            }
    //            else
    //            {
    //                Debug.Log("PhotonNetwork.Reconnect called");
    //                reconnectCalled = PhotonNetwork.Reconnect();
    //            }
    //            break;
    //        case DisconnectCause.OperationNotAllowedInCurrentState:
    //        case DisconnectCause.CustomAuthenticationFailed:
    //        case DisconnectCause.DisconnectByClientLogic:
    //        case DisconnectCause.InvalidAuthentication:
    //        case DisconnectCause.ExceptionOnConnect:
    //        case DisconnectCause.MaxCcuReached:
    //        case DisconnectCause.InvalidRegion:
    //        case DisconnectCause.None:
    //            break;
    //        default:
    //            throw new ArgumentOutOfRangeException("cause", lastDisconnectCause, null);
    //    }
    //    lastDisconnectCause = DisconnectCause.None;
    //    wasInRoom = false;
    //}

    //public override void OnJoinRoomFailed(short returnCode, string message)
    //{
    //    if (!rejoinCalled)
    //    {
    //        return;
    //    }
    //    rejoinCalled = false;
    //    Debug.LogErrorFormat("Quick rejoin failed with error code: {0} & error message: {1}", returnCode, message);
    //}

    //public override void OnJoinedRoom()
    //{
    //    if (rejoinCalled)
    //    {
    //        Debug.Log("Rejoin successful");
    //        Debug.Log("PhotonNetwork.CurrentRoom.AutoCleanUp" + PhotonNetwork.CurrentRoom.AutoCleanUp);
    //        Debug.Log("Current room details" + PhotonNetwork.CurrentRoom.Name);
    //        AuthenticationValues values = new AuthenticationValues();
    //        values.UserId = PhotonNetwork.LocalPlayer.UserId;
    //        PhotonNetwork.AuthValues = values;
    //        rejoinCalled = false;
    //    }
    //}

    //private void CheckAndRejoin()
    //{
    //    Debug.Log("(skipRejoinChecks" + skipRejoinChecks);
    //    skipRejoinChecks = true;
    //    if (skipRejoinChecks)
    //    {
    //        Debug.Log("PhotonNetwork.ReconnectAndRejoin called");
    //        rejoinCalled = PhotonNetwork.ReconnectAndRejoin();
    //    }
    //    else
    //    {
    //        bool wasLastActivePlayer = true;
    //        if (!persistenceEnabled)
    //        {
    //            for (int i = 0; i < PhotonNetwork.PlayerListOthers.Length; i++)
    //            {
    //                if (!PhotonNetwork.PlayerListOthers[i].IsInactive)
    //                {
    //                    wasLastActivePlayer = false;
    //                    break;
    //                }
    //            }
    //        }
    //        if ((PhotonNetwork.CurrentRoom.PlayerTtl < 0 || PhotonNetwork.CurrentRoom.PlayerTtl > minTimeRequiredToRejoin) // PlayerTTL checks
    //          && (!wasLastActivePlayer || PhotonNetwork.CurrentRoom.EmptyRoomTtl > minTimeRequiredToRejoin || persistenceEnabled)) // EmptyRoomTTL checks
    //        {
    //            Debug.Log("PhotonNetwork.ReconnectAndRejoin called");
    //            rejoinCalled = PhotonNetwork.ReconnectAndRejoin();
    //        }
    //        else
    //        {
    //            Debug.Log("PhotonNetwork.ReconnectAndRejoin not called, PhotonNetwork.Reconnect is called instead.");
    //            reconnectCalled = PhotonNetwork.Reconnect();
    //        }
    //    }
    //}

    //public override void OnConnectedToMaster()
    //{
    //    if (reconnectCalled)
    //    {
    //        Debug.Log("Reconnect successful");
    //        reconnectCalled = false;
    //    }
    //}
    public override void OnLeftRoom()
    {
        Debug.Log("left room callback");
        ResetDataOnDisconnect(PhotonNetwork.LocalPlayer);
        UnityEngine.SceneManagement.SceneManager.LoadScene((int)SceneIndexes.MAINMENU);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        LobbyController.otherPlayerLeft = true;
        PhotonNetwork.RemoveRPCs(otherPlayer);
        PhotonChatManager.instance.chatClient.Disconnect();
        //ResetDataOnDisconnect(PhotonNetwork.LocalPlayer);
        Debug.Log("other player left callback");
        StartCoroutine(OtherPlayerLeftCoroutine(otherPlayer.NickName));
    }
    IEnumerator OtherPlayerLeftCoroutine(string otherPlayerName)
    {
        DisconnectedPanel.SetActive(true);
        DisconnectedPanel.transform.SetAsLastSibling();
        DCText.text = otherPlayerName+" Left\nReturning to Main Menu...";
        //Timer.EndRoundTimerEvent();
        yield return new WaitForSeconds(3);

        DisconnectedPanel.SetActive(false);
        UnityEngine.SceneManagement.SceneManager.LoadScene((int)SceneIndexes.MAINMENU);
    }
    public void ResetDataOnDisconnect(Photon.Realtime.Player player)
    {
        if(PhotonChatManager.instance!=null)
            PhotonChatManager.instance.DisconnectChat();
        if (LobbyController.isAIEnable)
            LobbyController.isAIEnable = false;
        PhotonNetwork.RemoveRPCs(player);
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();
        if (PhotonNetwork.InLobby)
            PhotonNetwork.LeaveLobby();
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
    }
}

