using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System;

//THIS SCRIPT IS FOR THE BOT MODE WHEN NO PLAYER IS FOUND AFTER LOOKING FOR OPPONENT AND IS ATTACHED TO THE AI GAMEOBJECT
public class AI : MonoBehaviourPun
{
    public GameManager gameManager;
    [SerializeField] TMP_Text chatArea;
    [SerializeField] Transform nameTextTransform;
    private string AIMessage = "";
    public static Action StartAIEvent, StopAIEvent;
    [SerializeField] int imageNameTooLong = 8;
    [SerializeField] int randChanceToAnswerCorrect;
    [SerializeField] float randTimeToAnswer;
    public static AI instance;
    public static bool firstTimePlaying = false, AIRunning = false, AICanAnswer = true, AIWonBool = false;
    public int UserRoundAdvantage =0;
    int AIRoundAdvantage=0;
    string botName = "";
    int AIChances = 3;
    private int aiScore;
    public int AIScore { get { return aiScore; } set { aiScore = value; } }

    private void OnEnable()
    {
        StartAIEvent += StartAI;
        StopAIEvent += StopAI;
    }
    private void OnDisable()
    {
        StartAIEvent -= StartAI;
        StopAIEvent -= StopAI;
    }
    
    private void Start()
    {
        if (LobbyController.isAIEnable)
        {
            //setting default values
            AIWonBool = false;
            AICanAnswer = true;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            int noOfPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
            string[] botNames = { "Ankit", "Maria", "Aarav", "Barkat", "Juliana", "Domi", "Lucas", "Yara", "Shalin", "Sonu", "Vanhi", "Haider", "Meghan", "Luke",
            "Sam","Eli","Sophie","Maya","Lily","Chloe","Max","Mia","Emma","Brad","Jake","Alice","Ivy","Ezra","Lee","Zara","Zia","Nia","Ira","Grace"};
            int randName = UnityEngine.Random.Range(0, botNames.Length);
            //ai name will be set to the last name text(eg: if there is 1 player, ai will be in the 2nd name text, (game is 2 player as of now))
            Transform aiNameTextTrans = nameTextTransform.GetChild(noOfPlayers).GetChild(0);
            aiNameTextTrans.GetComponent<TMP_Text>().text = botNames[randName];

            chatArea.text += botNames[randName]+": joined \nROUND "+GameManager.instance.RoundNumber+" STARTED\n";
            botName = botNames[randName];

            //can be used to check if user has played before
            if(PlayerPrefs.HasKey("firstTime"))
            {
               firstTimePlaying = false;
            }
            instance = this;
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
    public void StartAI()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("ai started");
            AIRunning = true;
            StartCoroutine("CRRandomAnswerAI");
        }
    }
    public void StopAI()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("ai stoppped");
            AIRunning = false;
            ResetRandomValues();
            StopCoroutine("CRRandomAnswerAI");
        }
    }
    void ResetRandomValues()
    {
        AIChances = 3;
        AIMessage = "";
        randChanceToAnswerCorrect = 0;
        randTimeToAnswer = 0;
    }
    void AIWon()
    {
        //Debug.Log("ai correct");
        AIWonBool = true;
        UIManager.instance.EnableBlocker();
        GameManager.instance.PlayP2Smile();
        StopCoroutine("CRRandomAnswerAI");
        //Timer.EndRoundTimerEvent?.Invoke();
        CountdownTimer.EndRoundTimerEvent?.Invoke();
        TimerManager.ResetTimerEvent?.Invoke();
        AIRoundAdvantage++;
        AIMessage = gameManager.ImageName;
        photonView.RPC("SetAIAnswerToChat", RpcTarget.AllBuffered, AIMessage);
        aiScore += 10;
        UpdateRoundButtonsOnAIWin();
        GameManager.instance.RoundCountForRoundButtons++;
    }
    public IEnumerator CRRandomAnswerAI()
    {
        //main ai logic
        while (CountdownTimer.instance.isTimerRunning) 
        {
            AIChances--;
            if(AIChances>=0)
            {
                float multiplier = Extensions.MultiplierBasedOnStringLength(gameManager.ImageNameLength); //according to the length of the word, ai will take longer/shorter time to answer
                randTimeToAnswer = multiplier * UnityEngine.Random.Range(2, 5); 
                randChanceToAnswerCorrect = UnityEngine.Random.Range(1, 10); //if this is >=5, AI will answer correctly
                //different conditions
                if (firstTimePlaying || AIRoundAdvantage >= 3) //if player is first time playing, or has lost 3 rounds to ai, it will give wrong answer
                {
                    AIRoundAdvantage--;
                    if (AIRoundAdvantage <= 0)
                        AIRoundAdvantage = 0;
                    randChanceToAnswerCorrect = 6;//will always give wrong answer if user is playing first time or if ai has won 3 rounds(if image name is more than 3letters)
                }
                if (UserRoundAdvantage >= 3 || AIChances == 0) //if player has won 3 rounds or ai has given 3 incorrect answers in 1 round, it will give correct answer
                {
                    randChanceToAnswerCorrect = 5; //if user has won 3 rounds they will lose and ai will win
                    UserRoundAdvantage--;
                    if (UserRoundAdvantage <= 0)
                        UserRoundAdvantage = 0;
                }
                if (gameManager.ImageNameLength >= imageNameTooLong) //if the image name is too long ai will not answer correctly and will take too long
                {
                    //temp code
                    randChanceToAnswerCorrect = 20;
                    randTimeToAnswer = 20;
                }
                if (gameManager.ImageNameLength <= 4) //words like DOG, CAT, DOOR cannot be jumbled while keeping 1st and last letter same so ai will answer them correctly but will take at least 10 secs
                {
                    randTimeToAnswer = 10;
                    randChanceToAnswerCorrect = 5;
                }
                if (randChanceToAnswerCorrect <= 5 && AICanAnswer)
                {
                    //correct answer
                    yield return new WaitForSeconds(randTimeToAnswer); //wait before answering to make it seem like ai was typing
                    if (!AICanAnswer)//check again if ai can answer to see if user has answered during this time so both dont answer at same time
                        yield break;
                    AIWon();
                    ResetRandomValues();

                    //ui and sound part and finally next round
                    UIManager.UIEventAfterEachRound(0, "YOU LOST!", false);
                    AudioManager.instance.PlaySoundTrack(AudioEnum.roundLost);
                    StartCoroutine(GameManager.instance.CRDisplayUIAfterRound());
                    gameManager.RPCNextRound(PlayerSetup.Result.WonLost);
                    //GameEvents.instance.mainCanvas.interactable = false;//stop user from typing
                }
                else
                {
                    //wrong answers
                    yield return new WaitForSeconds(randTimeToAnswer); //wait before answering to make it seem like ai was typing
                    string tempString = Extensions.GetRandomString(gameManager.ImageName); //function which returns jumbled word while keeping 1st and last letter same
                    yield return null;
                    AIMessage = tempString;
                    photonView.RPC("SetAIAnswerToChat", RpcTarget.AllBuffered, AIMessage);
                }
            }
        }
    }
    void UpdateRoundButtonsOnAIWin()
    {
        gameManager.p1Rounds[GameManager.instance.RoundCountForRoundButtons].GetComponent<Image>().color = Color.red;
        gameManager.p2Rounds[GameManager.instance.RoundCountForRoundButtons].GetComponent<Image>().color = Color.green;
    }
    [PunRPC]
    public void SetAIAnswerToChat(string answer)
    {
        chatArea.text += botName +": "+ answer + '\n';
    }
}