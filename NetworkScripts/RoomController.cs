using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class RoomController : MonoBehaviourPunCallbacks
{
    public static Action StartGameEvent;
    private const int MaxPlayersPerRoom = 2;
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
        StartGameEvent += StartGame;
    }
    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        StartGameEvent -= StartGame;
    }
    //public override void OnJoinedRoom()
    //{
    //    if(PhotonNetwork.CurrentRoom.PlayerCount== MaxPlayersPerRoom && PhotonNetwork.IsMasterClient)
    //    {
    //        StartGame();
    //    }
    //}
    public void StartGame()
    {
        PhotonNetwork.LoadLevel((int)SceneIndexes.GAME);
    }
}
