using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;

//THIS IS THE SCRIPT FOR MAIN PLAYER FUNCTIONS AND IS ATTACHED TO THE USER PREFAB
//CR: Coroutines, in all the classes
public class PlayerSetup : MonoBehaviourPunCallbacks
{
    public GameManager myGameManager;
    int resetTime = 999999999;
    public ExitGames.Client.Photon.Hashtable MyTimeProperties = new ExitGames.Client.Photon.Hashtable();
    public ExitGames.Client.Photon.Hashtable MyRoundProperties = new ExitGames.Client.Photon.Hashtable();

    [SerializeField] Button answerBoxPrefab, alphabetPrefab;
    List<GameObject> thisPlayerRounds = new List<GameObject>();
    List<GameObject> otherPlayerRounds = new List<GameObject>();
    public static Action SpawnAlphabetsEvent, ResetValuesEvent, DrawEvent, IncrementRoundNumberEvent;
    public static GameObject LocalPlayerInstance;
    private const byte UPDATE_UI_CODE = 1, ALPHABET_EVENT_CODE = 2;
    [SerializeField] GameObject userPrefabObj;
    TMP_Text playerNameText;
    Transform nameTexts;
    long thisPlayerTime,otherPlayerTime;
    [SerializeField] AlphabetInput alphabetInput;
    [SerializeField] Sprite[] roundBtnSprites;

    LevelSystem levelSystem = new LevelSystem();

    private int attempt = 0;

    private void OnEnable()
    {
        ResetValuesEvent += ResetValues;
        DrawEvent += Draw;
        IncrementRoundNumberEvent += IncrementRoundForButton;
        PhotonNetwork.NetworkingClient.EventReceived += RaiseEventHandler;
    }
    private void OnDisable()
    {
        ResetValuesEvent -= ResetValues;
        DrawEvent -= Draw;
        SpawnAlphabetsEvent -= SpawnAlphabets;
        IncrementRoundNumberEvent -= IncrementRoundForButton;
        PhotonNetwork.NetworkingClient.EventReceived -= RaiseEventHandler;
    }

    private void Awake()
    {
        if (photonView.IsMine)
        {
            LocalPlayerInstance = this.gameObject;
        }
        if(LobbyController.isSinglePlayer)
        {
            GameManager.instance.singlePlayerNameText.text = PhotonNetwork.NickName;
            thisPlayerRounds = GameManager.instance.singlePlayerRounds;
        }
        else
        {
            DisplayAndAssignPlayerDetails();
        }
        
        //assigning answerbox transform, score and gamemanager for individual changes for player 
        myGameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        thisPlayerTime = otherPlayerTime = resetTime; //time custom property set for comparing who answered first
        SpawnAlphabetsEvent += SpawnAlphabets;//set here because awake is called before onEnable
        if (photonView.IsMine)
        {
            SetCustTimePropForPlayer(resetTime);//setting default time for time custom property for using later in comparing who answered first
            SetRoundNumCP(1);
            SpawnAlphabetsEvent();
           // InitialiseLevelSystem();
        }
        // SpawnAlphabets();
    }
    void InitialiseLevelSystem()
    {
        levelSystem.Level = PlayerPrefs.GetInt("Level");
        levelSystem.CurrentExperience = PlayerPrefs.GetInt("Experience");// - (levelSystem.Level*100);//eg: xp is 750, so current xp would be 750 - 7*100 = 50
    }
    #region LEFT SIDE PANEL UI
    enum PlayerID
    {
        Player1 = 1, Player2 = 2
    }
    void DisplayAndAssignPlayerDetails()
    {
        nameTexts = GameManager.instance.nameTexts;
        PhotonView[] photonViews = PhotonNetwork.PhotonViews;
        for (int i = 0; i < photonViews.Length; i++)
        {
            if (photonView.OwnerActorNr == i + 1)
            {
                Transform nameTextTrans = nameTexts.transform.GetChild(i).GetChild(0);//i will be 1,2,... accordingly and 0 is the 0th child which is the text field
                playerNameText = nameTextTrans.GetComponent<TMP_Text>();
                playerNameText.text = photonView.Owner.NickName;
                if (photonView.Owner.IsMasterClient)
                {
                    thisPlayerRounds = GameManager.instance.p1Rounds;
                    otherPlayerRounds = GameManager.instance.p2Rounds;
                }
                else
                {
                    thisPlayerRounds = GameManager.instance.p2Rounds;
                    otherPlayerRounds = GameManager.instance.p1Rounds;
                }
            }
        }
    }
    #endregion

    #region ALPHABET & ANSWERBOX CODE
    //SPAWNING THE ALPHABET AND ANSWER BOXES
    public enum UserPrefabChildren //transform of the children in prefab 
    {
        Alphabets = 0,
        AnswerBoxes = 1
    }
    void SpawnAlphabets()
    {
        //instantiate the alphabet buttons
        for (int i = 0; i < GameUtility.CountOfAlphabetsBoxes; i++)
        {
            //GameObject alphabetObj = PhotonNetwork.InstantiateRoomObject(alphabetPrefab.name, this.transform.position,Quaternion.identity);
            GameObject alphabetObj = Instantiate(alphabetPrefab.gameObject, this.transform);
            Transform userPrefabTransform = this.gameObject.transform;
            Transform alphabetsPanelTransform = userPrefabTransform.GetChild((int)UserPrefabChildren.Alphabets);
            //after spawning set alphabets panel in userprefab as the parent
            alphabetObj.transform.SetParent(alphabetsPanelTransform);
            alphabetObj.GetComponent<Image>().sprite = roundBtnSprites[i]; 
        }
        CallSpawnAnswerBoxes();//rename above alphabets and spawn the rest for others if master client
    }
    void CallSpawnAnswerBoxes()
    {
        if (PhotonNetwork.IsMasterClient && photonView.IsMine)
        { 
            //call for self
            //rename the alphabet boxes
            RenameAlphabets(GameManager.instance.ImageName, GameUtility.CountOfAlphabetsBoxes);
            //spawn the answer boxes
            SpawnAnswerBoxes(GameManager.instance.ImageName, GameManager.instance.ImageNameLength);

            //raise event for others
            try
            {
                SpawningRaiseEventForOthers();
            }
            catch (Exception)
            {
               // RaiseEventForOtherUsers();
            }
        }
    }
    void SpawningRaiseEventForOthers()
    {
        string imageName = GameManager.instance.ImageName;
        int imageNameLength = GameManager.instance.ImageNameLength;
        int noOfAlphabets = GameUtility.CountOfAlphabetsBoxes;
        int viewId = photonView.ViewID;
        object[] data = new object[] { imageName, imageNameLength, noOfAlphabets, viewId };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(ALPHABET_EVENT_CODE, data, raiseEventOptions, SendOptions.SendReliable);
    }
    void RenameAlphabets(string imageNameFromRPC, int noOfAlphabetBoxes) //default boxes have 'N' as the text, here we rename them to jumbled random alphabets with no repetition
    {
        string st = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        char[] imageName = Extensions.RemoveDuplicates(imageNameFromRPC).ToCharArray();
        foreach (var c in imageName)
        {
            st = st.Replace(c.ToString(), string.Empty);
        }
        int totalCharCount = noOfAlphabetBoxes - imageName.Length;
        st = Extensions.Scramble(st);
        st = st.Substring(0, totalCharCount);
        string temp = new string(imageName);
        string joinedString = st + temp;
        string finalGeneratedRandString = Extensions.Scramble(joinedString);
        for (int i = 0; i < noOfAlphabetBoxes; i++)
        {
            char c = finalGeneratedRandString[i];
            int alphabetBoxIndex = (int)PlayerSetup.UserPrefabChildren.Alphabets;
            Transform alphabetBoxesTransform = userPrefabObj.transform.GetChild(alphabetBoxIndex);
            if (alphabetBoxesTransform.childCount > 0)
                alphabetBoxesTransform.GetChild(i).GetComponentInChildren<TMP_Text>().text = "" + c;
        }
    }

    public void SpawnAnswerBoxes(string imageName, int imageNameLength)
    {
        GameManager.instance.ImageName = imageName;
        GameManager.instance.ImageNameLength = imageNameLength;

        if (photonView.IsMine)
        {
            userPrefabObj = GetComponent<PhotonView>().gameObject;
        }
        int answerBoxIndex = (int)PlayerSetup.UserPrefabChildren.AnswerBoxes;
        Transform ansBoxesPanelTransform = userPrefabObj.transform.GetChild(answerBoxIndex);
        // delete previous boxes if exist
        alphabetInput.ClearDict();
        for (int i = 0; i < ansBoxesPanelTransform.childCount; i++)
        {
            Destroy(ansBoxesPanelTransform.GetChild(i).gameObject);
            //PhotonNetwork.Destroy(answerBoxesTransform.GetChild(i).gameObject);
        }

        for (int i = 0; i < GameManager.instance.ImageNameLength; i++) //instantiating the spaces for user to input and adding image name text to boxes
        {
           //GameObject answerBoxObj = PhotonNetwork.InstantiateRoomObject(answerBoxPrefab.name, answerBoxesTransform.position, Quaternion.identity);
            GameObject answerBoxObj = Instantiate(answerBoxPrefab.gameObject, ansBoxesPanelTransform);
            answerBoxObj.transform.SetParent(ansBoxesPanelTransform);
            alphabetInput.AnswerBoxDict.Add(i, null); //add value to dictionary
        }
    }
    #endregion

    #region CLEARING ANSWER BOXES

    void ClearSpaces()
    {
        Transform thisPlayerAnsBoxTrans = this.transform.GetChild((int)UserPrefabChildren.AnswerBoxes); //get the answer box panel transform in the user prefab
        
        for (int i = 0; i < this.transform.GetChild((int)UserPrefabChildren.AnswerBoxes).childCount; i++)
        {
            thisPlayerAnsBoxTrans.GetChild(i).GetComponentInChildren<TMP_Text>().text = "";
        }
    }
    public void ClearSpacesOnBtnClick()
    {
        ClearSpaces();//clear spaces first
        alphabetInput.answerBoxNumber = 0;//set index to 0
        alphabetInput.ClearDict();//clear the dict
        AddAlphabetsToDict();//refill the dict
    }
    void AddAlphabetsToDict()
    {
        for (int i = 0; i < GameManager.instance.ImageNameLength; i++)
        {
            alphabetInput.AnswerBoxDict.Add(i, null); //add value to dictionary
        }
    }
    #endregion

    #region RAISE EVENT CODE
    private void RaiseEventHandler(EventData obj)
    {
        //spawning of boxes for clients called by master
        if (obj.Code == ALPHABET_EVENT_CODE)
        {
            object[] data = (object[])obj.CustomData;
            string imagename = (string)data[0];
            int imagelength = (int)data[1];
            int noOfAlphabets = (int)data[2];
            int masterViewId = (int)data[3];

            PhotonView[] photonViews = PhotonNetwork.PhotonViews;
            foreach (PhotonView PV in photonViews)
            {
                if (PV.Owner != null && PV.ViewID != masterViewId)
                {
                    RenameAlphabets(imagename, noOfAlphabets);
                    SpawnAnswerBoxes(imagename, imagelength);
                    SetCustTimePropForPlayer(resetTime); //resetting custom time property value 
                }
            }
        }
        //message to clients on winning(winner sends to others)
        if (obj.Code == UPDATE_UI_CODE)
        {
            object[] data = (object[])obj.CustomData;
            int viewId = (int)data[0];
            string str = (string)data[1];
            PhotonView[] photonViews = PhotonNetwork.PhotonViews;
            foreach (PhotonView photonView in photonViews)
            {
                if (photonView.Owner != null && photonView.ViewID != viewId)
                {
                    UpdateRoundButtonColor();
                    CountdownTimer.EndRoundTimerEvent?.Invoke();
                    TimerManager.ResetTimerEvent?.Invoke();
                    UIManager.UIEventAfterEachRound(0, "YOU LOST!", false);
                    AudioManager.instance.PlaySoundTrack(AudioEnum.roundLost);

                    if (PhotonNetwork.IsMasterClient)
                        GameManager.instance.PlayP2Smile();
                    else
                        GameManager.instance.PlayP1Smile();
                    StartCoroutine(myGameManager.CRDisplayUIAfterRound());
                }
            }
        }
    }
    #endregion

    #region GET/SET TIME CUSTOM PROPERTIES FOR COMPARING WHO ANSWERED FIRST
    private void GetTimeProperty() //get time from other player when they gave the answer
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (this.photonView.Owner.ActorNumber != player.ActorNumber)
            {
                otherPlayerTime = (int)player.CustomProperties["MyTime"];
            }
        }
    }
    public void SetCustTimePropForPlayer(int milliseconds) //set time for this player when they gave the answer
    {
        MyTimeProperties["MyTime"] = milliseconds;
        PhotonNetwork.LocalPlayer.SetCustomProperties(MyTimeProperties);
    }
    public void SetRoundNumCP(int round)
    {
        MyRoundProperties["RoundNum"] = round;
        PhotonNetwork.LocalPlayer.SetCustomProperties(MyRoundProperties);
    }
    #endregion

    #region ANSWER CHECKING 
    public void OnAllSpacesFilled()
    {
        Debug.Log("onallspace");
        if (LobbyController.isAIEnable && AI.AIWonBool)
            return;
        AI.AICanAnswer = false;//set to false so ai cant answer at this time
        if (TimerManager.isTimeUp && !LobbyController.isSinglePlayer)
            return;

        string tempAnswer = "";
        Transform thisPlayerAnswerBoxTransform = this.transform.GetChild((int)UserPrefabChildren.AnswerBoxes);

        for (int i = 0; i < this.transform.GetChild((int)UserPrefabChildren.AnswerBoxes).childCount; i++)
        {
            //collecting the text from the spaces into tempAnswer to fetch answer given by user
            tempAnswer += thisPlayerAnswerBoxTransform.GetChild(i).GetComponentInChildren<TMP_Text>().text;
        }

        if (tempAnswer == myGameManager.ImageName)//correct answer
        {
            Correct(tempAnswer);
            //AddExperience();
        }
        else //incorrect answer
        {
            Incorrect(tempAnswer);
        }
    }
    //void AddExperience()
    //{
    //    levelSystem.AddExperience(GameUtility.ExperienceForCorrectAnswer);
    //    GameManager.instance.experienceGathered += GameUtility.ExperienceForCorrectAnswer;//for displaying xp at end of game

    //    int tempXP = levelSystem.CurrentExperience;
    //    int tempLevel = levelSystem.Level;
    //    Debug.Log(tempXP);
    //    Debug.Log(tempLevel);
    //    PlayerPrefs.SetInt("Experience", tempXP);
    //    PlayerPrefs.SetInt("Level", tempLevel);
    //}
    void Correct(string tempAnswer)
    {
        Debug.Log("correct");
        int millisecondsNow = PhotonNetwork.ServerTimestamp;
        thisPlayerTime = millisecondsNow;
        SetCustTimePropForPlayer(millisecondsNow);
        //StartCoroutine(CheckIfAnswered(tempAnswer));
        StartCoroutine(CRWaitThenCheckIfCanAnswer(tempAnswer));  //wait then check so that data is loaded from both players on network
    }
    void Incorrect(string tempAnswer)
    {
        if(photonView.IsMine)
        {
            Debug.Log("incorrect");
            //send wrong answer to chat
            PhotonChatManager chatManager = GameManager.instance.PhotonChatManagerObj.GetComponent<PhotonChatManager>();
            string sendMessage = PhotonNetwork.NickName + ": " + tempAnswer;
            chatManager.SendAnswerToChat(sendMessage);
            //ui part
            StartCoroutine(myGameManager.CRDisplayUIAfterRound());
            AudioManager.instance.PlaySoundTrack(AudioEnum.roundLost);
            UIManager.UIEventAfterEachRound(0, "INCORRECT!", false);
            UIManager.instance.ResetPanels();
            AI.AICanAnswer = true;
            StartCoroutine(CRWaitAfterWrongAnswer());
        }
    }
    void Draw()
    {
        UIManager.UIEventAfterEachRound(0, "DRAW!", false);
        StartCoroutine(myGameManager.CRDisplayUIAfterRound());
        myGameManager.RPCNextRound(Result.Draw);
    }
    public enum Result { WonLost, Draw };

    IEnumerator CRWaitThenCheckIfCanAnswer(string tempAnswer)
    {
        yield return new WaitForSeconds(0.5f); //wait so value gets updated over network of the time player answered
        if (LobbyController.isAIEnable && AI.AIWonBool)
            yield break;
        GetTimeProperty();
        ClearSpaces();
        Debug.Log(otherPlayerTime + " " + thisPlayerTime);
        PhotonChatManager chatManager = GameManager.instance.PhotonChatManagerObj.GetComponent<PhotonChatManager>();
        if (otherPlayerTime - thisPlayerTime > 0 || otherPlayerTime == resetTime || LobbyController.isAIEnable)
        {
            if (TimerManager.isTimeUp)
                yield break;
            if (!LobbyController.isSinglePlayer)
            {
                CountdownTimer.EndRoundTimerEvent?.Invoke();
                TimerManager.ResetTimerEvent?.Invoke();
            }
            SetRoundNumCP(myGameManager.RoundNumber+1);
            //update UI for others by raise event
            int playerWonViewId = photonView.ViewID;
            String playerWonName = photonView.Owner.NickName;
            object[] data = new object[] { playerWonViewId, playerWonName };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent(UPDATE_UI_CODE, data, raiseEventOptions, SendOptions.SendReliable);

            //play audio for self
            AudioManager.instance.PlaySoundTrack(AudioEnum.roundWon);

            //update UI for self
            string sendMessage = PhotonNetwork.NickName + ": " + tempAnswer;
            UIManager.UIEventAfterEachRound(GameUtility.PointsForCorrectAnswer, "CORRECT!", true);
            if(!LobbyController.isSinglePlayer)
            {
                UpdateRoundButtonColor();
                chatManager.SendAnswerToChat(sendMessage);//use chat manager for multiplayer
                if(PhotonNetwork.IsMasterClient)
                    GameManager.instance.PlayP1Smile();
                else
                    GameManager.instance.PlayP2Smile();
            }
            else
            {
                myGameManager.chatArea.text += sendMessage+"\n";//directly set text for singleplayer
                myGameManager.PlaySPSmile();
            }
            photonView.RPC("IncrementRoundForButton", RpcTarget.AllBuffered);//update round number for round buttons for all players

            this.GetComponent<PlayerScore>().UpdateScoreEvent(GameUtility.PointsForCorrectAnswer);      //update score
            StartCoroutine(myGameManager.CRDisplayUIAfterRound());                                        //wait and show change alpha and confetti

            if (LobbyController.isAIEnable)
                AI.instance.UserRoundAdvantage++;
            AnalyticsResult analyticsResult = Analytics.CustomEvent("Round", new Dictionary<string, object> { {"Round", myGameManager.RoundNumber } });
            Debug.Log(analyticsResult + " nextRound");

            myGameManager.RPCNextRound(Result.WonLost);
        }
        else if (otherPlayerTime == thisPlayerTime && photonView.IsMine) //draw case
        {
            DrawEvent?.Invoke();
        }
    }
    [PunRPC]
    public void IncrementRoundForButton()
    {
        GameManager.instance.RoundCountForRoundButtons++;
    }
    void UpdateRoundButtonColor()
    {
        thisPlayerRounds[GameManager.instance.RoundCountForRoundButtons].GetComponent<Image>().color = Color.green;
        otherPlayerRounds[GameManager.instance.RoundCountForRoundButtons].GetComponent<Image>().color = Color.red;
    }

    IEnumerator CRWaitAfterWrongAnswer()
    {
        //clear answer box dictionary
        alphabetInput.AnswerBoxDict.Keys.ToList().ForEach(value => alphabetInput.AnswerBoxDict[value] = null);
        alphabetInput.answerBoxNumber = 0;
        yield return new WaitForSeconds(GameUtility.WaitDelayTime);
        ClearSpaces();
        UIManager.DisableBlockerEvent?.Invoke();
    }
    #endregion
    void ResetValues()//after each round
    {
        alphabetInput.answerBoxNumber = 0;
        thisPlayerTime = resetTime; otherPlayerTime = resetTime;
        CallSpawnAnswerBoxes();
        SetCustTimePropForPlayer(resetTime);
    }
}
