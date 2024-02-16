using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine.Analytics;

public class GameOver : MonoBehaviourPun
{
    public static Action GameOverEvent, FeedbackEvent;
    [SerializeField] TMP_Text p1FinalScoreText, p2FinalScoreText, winnerText, scoreText, experienceText;
    [SerializeField] List<TMP_Text> finalScoreTexts;
    [SerializeField] GameObject feedbackPanel;

    Dictionary<Player, int> playerScoreDict = new Dictionary<Player, int>();

    private void OnEnable()
    {
        GameOverEvent += RPCGameEnded;
        FeedbackEvent += ShowFeedback;
    }
    private void OnDisable()
    {
        GameOverEvent -= RPCGameEnded;
        FeedbackEvent -= ShowFeedback;
    }
    public void LeaveRoom()//on restart button clicked
    {
        AnalyticsResult analyticsResult = Analytics.CustomEvent("Restart");
        Debug.Log(analyticsResult + " restart");
        StartCoroutine(DisconnectAndLoad());
    }
    IEnumerator DisconnectAndLoad() //disconnect from photon and load the main lobby
    {
        if(PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
        }
        PhotonChatManager.instance.DisconnectChat();
        //while (PhotonNetwork.InRoom)
        while (PhotonNetwork.IsConnected)
            yield return null;
        SceneManager.LoadScene((int)SceneIndexes.MAINMENU);
        LobbyController.isAIEnable = false;
    }
    public void ExitGame()
    {
        Application.Quit(0);
    }
    public void RPCGameEnded()
    {
        photonView.RPC("OnGameEnded", RpcTarget.AllBuffered);
    }
    [PunRPC]
    void OnGameEnded()
    {
        StartCoroutine(GameEnd());
    }
    void ShowFeedback()
    {
        //will show only once per user
        if (!PlayerPrefs.HasKey("FirstTime")) //return true if the key exist
        {
            PlayerPrefs.SetInt("FirstTime", 0);
            feedbackPanel.SetActive(true);
            feedbackPanel.transform.SetAsLastSibling();
            LobbyController.showFeedback = true;
        }
        else
        {
            LobbyController.showFeedback = false;
        }
    }
    IEnumerator GameEnd()
    {
        yield return new WaitForSeconds(GameUtility.WaitDelayTime);
        UIManager.DisableBlockerEvent?.Invoke();
        GameEvents.GameOverPanelEvent?.Invoke();
        TimerManager.isTimeUp = false;
        //fetch and display score
        if (LobbyController.isSinglePlayer)//singleplayer
        {
            int score = (int)PhotonNetwork.LocalPlayer.CustomProperties["Score"];
            winnerText.text = "Your Score" + " : " + score;
            p1FinalScoreText.text = p2FinalScoreText.text = "";
        }
        else//multiplayer
        {
            FetchScoreIntoDict();
            if (LobbyController.isAIEnable)
            {
                FetchAndDisplayScoreWithAI(); //with ai
            }
            else
            {
                DisplayScoreMultiplayer();
            }
        }
        //experienceText.text = "XP Gained : " + GameManager.instance.experienceGathered;
        //disable player objects
        PhotonView[] photonViews = PhotonNetwork.PhotonViews;
        foreach (PhotonView photonView in photonViews)
        {
            var playerPrefabObject = photonView.gameObject;
            if (playerPrefabObject.name != "GameManager")
                    playerPrefabObject.SetActive(false);
        }
        //FEEDBACK FORM POP UP LOGIC
        float randWaitForFeedback = UnityEngine.Random.Range(2, 3);
        yield return new WaitForSeconds(randWaitForFeedback);
        ShowFeedback();
    }

    void FetchAndDisplayScoreWithAI()
    {
        int playerScore = 0;
        foreach (KeyValuePair<Player, int> playerScoreKVP in playerScoreDict)
        {
            playerScore = playerScoreKVP.Value;
        }
        int aiScore = AI.instance.AIScore;
        string playerName = PhotonNetwork.NickName;
        string aiName = GameManager.instance.player2NameText.text;

        p1FinalScoreText.text = playerName + " : " + playerScore;
        p2FinalScoreText.text = aiName + " : " + aiScore;
        winnerText.text = (playerScore == aiScore ? "Its a Draw" : (playerScore > aiScore) ? playerName + " Wins" : aiName + " Wins");
    }

    void FetchScoreIntoDict()
    {
        int i = 0;
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            ++i;
            if (player.ActorNumber == i)
            {
                playerScoreDict.Add(player, (int)player.CustomProperties["Score"]);
            }
        }
    }
    void DisplayScoreMultiplayer()
    {
        int i = 0, tempHighScore = 0;
        string tempWinner = "";
        bool isDraw = false;
        foreach (KeyValuePair<Player, int> playerScoreKVP in playerScoreDict)
        {
            //setting name and score in variables
            string tempName = "P" + playerScoreKVP.Key.ActorNumber + "FinalScore";
            string playerNickname = playerScoreKVP.Key.NickName;
            int playerScore = playerScoreKVP.Value;
            if (tempName == finalScoreTexts[i].name)
            {
                finalScoreTexts[i].text = playerNickname + " : " + playerScore;
            }
            //checking highscore
            if (playerScore > tempHighScore)//if this player's score is greater than the last
            {
                //set this player as temporary winner for this loop
                tempWinner = playerNickname;
                tempHighScore = playerScore;
                isDraw = false;
            }
            else if (playerScore == tempHighScore)
            {
                isDraw = true;
            }
            i++;
        }

        winnerText.text = (isDraw ? "Its a Draw" : tempWinner + " Wins");
    }
}
