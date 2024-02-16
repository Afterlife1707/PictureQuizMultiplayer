using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
//THIS IS THE SCRIPT FOR FUNCTIONS WHEN USER PAUSES OR GOES OUT OF THE APP ETC.
public class ApplicationPause : MonoBehaviourPun
{
    DateTime pauseDateTime;
    int roundAtPause, roundFromOtherUser;
    private void OnApplicationPause(bool pause)
    {
        if (pause && CountdownTimer.instance.isTimerRunning)
        {
            if (LobbyController.isSinglePlayer)
            {
                return;
            }
            else
            {
                SwitchOrDisconnect();
                GoToMainMenu();
            }
        }
        //if (CountdownTimer.instance.isTimerRunning)
        //{
        //    if (pause)
        //    {
        //        Debug.Log("paused");
        //        if (!LobbyController.isAIEnable)
        //        {
        //            SwitchOrDisconnect();
        //        }
        //        pauseDateTime = DateTime.Now;
        //        roundAtPause = GameManager.instance.RoundNumber;
        //    }
        //    else//on unpause
        //    {
        //        Debug.Log("unpaused");
        //        float AFKTime = (float)(DateTime.Now - pauseDateTime).TotalSeconds;
        //        CountdownTimer.instance.TimeRemaining -= AFKTime;
        //        if (AFKTime < 20)//if returned within 20 secs
        //        {
        //            GetRoundNumFromOtherPlayer();
        //            Debug.Log("paused at:"+ roundAtPause + "current:" + roundFromOtherUser);
        //            if(roundAtPause != roundFromOtherUser)
        //            {
        //                GoToMainMenu();
        //            }
        //            //if (CountdownTimer.instance.TimeRemaining <= 0)
        //            //{
        //            //    GoToMainMenu();
        //            //}
        //        }
        //        else//if didnt return within 20secs
        //        {
        //            GoToMainMenu();
        //        }
        //    }
        //}
    }
    void GetRoundNumFromOtherPlayer()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (this.photonView.Owner.ActorNumber != player.ActorNumber)
            {
                roundFromOtherUser = (int)player.CustomProperties["RoundNum"];
            }
        }
    }
    void GoToMainMenu()
    {
        LobbyController.selfDisconnected = true;
        PhotonNetwork.LeaveRoom();
    }

    private void SwitchOrDisconnect()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.PlayerList.Length > 1 && PhotonNetwork.KeepAliveInBackground > 0f)
            {
                PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer.GetNext());
                Debug.Log("master switched");
            }
            else
            {
                PhotonNetwork.Disconnect();
            }
            PhotonNetwork.SendAllOutgoingCommands(); 
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            Debug.Log("initial player details" + player.IsMasterClient + "initial player" + player.NickName);

        }
    }
}
