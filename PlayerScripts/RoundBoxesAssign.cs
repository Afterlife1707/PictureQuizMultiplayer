using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using UnityEngine.UI;

//SCRIPT NOT USED// TESTING FOR NOW
public class RoundBoxesAssign : MonoBehaviourPun
{
    Transform roundBoxes;
    public static Action AssignRoundsEvent, RoundWonEvent;
    public List<GameObject> thisPlayerRounds = new List<GameObject>();


    private void OnEnable()
    {
        AssignRoundsEvent += AssignRoundBox;
        RoundWonEvent += RoundWon;
    }
    private void OnDisable()
    {
        AssignRoundsEvent -= AssignRoundBox;
        RoundWonEvent -= RoundWon;
    }

    public void AssignRoundBox()
    {
        roundBoxes = GameManager.instance.roundBoxes;
        PhotonView[] photonViews = PhotonNetwork.PhotonViews;
        for (int i = 0; i < photonViews.Length; i++)
        {
            if (photonView.OwnerActorNr == i + 1)
            {
                for (int j = 0; j < GameUtility.NumberOfRounds; j++)
                {
                    thisPlayerRounds.Add(roundBoxes.transform.GetChild(i).GetChild(j).gameObject);
                }
            }
        }
    }
    public void RoundWon()
    {
        thisPlayerRounds[GameManager.instance.RoundCountForRoundButtons].GetComponent<Image>().color = Color.green;
        if (LobbyController.isAIEnable)
        {
            List<GameObject> AIRounds = GameManager.instance.p2Rounds;
            AIRounds[GameManager.instance.RoundCountForRoundButtons].GetComponent<Image>().color = Color.red;
        }
    }
}
