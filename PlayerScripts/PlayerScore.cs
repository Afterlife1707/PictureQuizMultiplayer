using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
//THIS SCRIPT IS FOR THE PLAYER SCORE PART
//SCORE IS UPDATED AND SAVED IN THE PHOTON PLAYER CUSTOM PROPERTIES
public class PlayerScore : MonoBehaviourPun
{
    public Action<int> UpdateScoreEvent;
    [SerializeField]
    TMPro.TMP_Text scoreText;
    private int score;
    public int getScore
    {
        get { return score; }
    }
    public ExitGames.Client.Photon.Hashtable MyCustomProperties = new ExitGames.Client.Photon.Hashtable();

    private void OnEnable()
    {
        UpdateScoreEvent += UpdateScore;
    }
    private void OnDisable()
    {
        UpdateScoreEvent -= UpdateScore;
    }
    private void Start()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        scoreText = GameObject.FindGameObjectWithTag("ScoreText").GetComponent<TMPro.TMP_Text>();
        //initializing default value
        SetCustomPropertiesForPlayer(0);
    }

    public void UpdateScore(int scoreReceived)
    {
        score += scoreReceived; 
        if (score <= 0)
        {
            score = 0;
        }
        //set score to text
        scoreText.text = ""+ score.ToString();
        //save score in custom properties
        SetCustomPropertiesForPlayer(score);
    }

    private void SetCustomPropertiesForPlayer(int scoreUpdate)
    {
        MyCustomProperties["Score"] = scoreUpdate;
        PhotonNetwork.LocalPlayer.SetCustomProperties(MyCustomProperties);
    }
}