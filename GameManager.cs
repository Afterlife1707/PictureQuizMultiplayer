using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;

//THIS SCRIPT IS ATTACHED TO THE GAME MANAGER GAMEOBJECT AND IS USED FOR CONTROLLING THINGS LIKE LOADING IMAGES, LOADING NEXT ROUND
//OPTIONAL PART
[RequireComponent(typeof(UIManager), typeof(GameEvents), typeof(ImageSpawner))]
[RequireComponent(typeof(TimerManager), typeof(Save))]
[RequireComponent(typeof(DisconnectRecovery), typeof(GameOver), typeof(GameUtility))]
[RequireComponent(typeof(CountdownTimer))]
//*END OPTIONAL PART
public class GameManager : MonoBehaviourPunCallbacks
{
    public List<GameObject> p1Rounds;
    public List<GameObject> p2Rounds;
    int currentImageID;
    public TMP_Text player1NameText, player2NameText;
    public Transform nameTexts,roundBoxes;
    public GameObject PhotonChatManagerObj;
    [SerializeField]int currentImageNumber;
    public Dictionary<int, ImageClass> imageDict;
    [SerializeField] GameObject tipText;
    [SerializeField] Animator p1Face,p2Face;
    [SerializeField] GameObject bubble1,bubble2;
    [SerializeField] Image bubble1Image, bubble2Image;
    int tempRoundCheck;


    [SerializeField] GameObject singlePlayerPanel, multiPlayerPanel, timer;
    //single player
    public List<GameObject> singlePlayerRounds;//not used as of now
    public TMP_Text singlePlayerNameText;
    [SerializeField] Button skipBtn;
    [SerializeField] Animator singlePFace;
    [SerializeField] TMP_Text singlePRoundNum;
    [SerializeField] GameObject bubbleS;
    [SerializeField] Image bubbleSImage;
    //*end single player

    //references
    public static GameManager instance;
    DataController dataController;
    private ImageClass[] imageData;
    private List<ImageClass> listOfImageClassPlayedObj;
    [SerializeField] GameEvents gameEvents;
    //props
    private string imageName;
    public string ImageName { get { return imageName; } set { imageName = value; } }
    private int roundCountForRoundButtons = 0;
    public int RoundCountForRoundButtons { get { return roundCountForRoundButtons; } set { roundCountForRoundButtons = value; } }
    private int roundNumber = 1;
    public int RoundNumber { get { return roundNumber; } set { roundNumber = value; } }
    public TMP_Text chatArea;
    private int imageNameLength = 0;
    public int ImageNameLength { get { return imageNameLength; } set { imageNameLength = value; } }
    public int experienceGathered = 0;

    public void SetListOfImageClassPlayedObj(List<ImageClass> listOfImageClassPlayedObj) {
        this.listOfImageClassPlayedObj = listOfImageClassPlayedObj;   
    }

    public List<ImageClass> GetListOfImageClassPlayedObj() {
        return listOfImageClassPlayedObj;
    }

    void OnEnable()
    {
        dataController = DataController.GetDataController;
        //dataController = FindObjectOfType<DataController>(); //alternate way
    }
   
    void OnDisable()
    {
        dataController = null;
    }
    private void OnApplicationQuit()
    {
        PhotonNetwork.Disconnect();
    }
    public void LoadImageData()
    {
        imageData = dataController.LoadImageData();
        imageDict = imageData.ToDictionary(images => images.imageID, images => images);
        //check if the image are already viewed by the user
        ImageSpawner tempImageSpawnObj = GetComponent<ImageSpawner>();
        tempImageSpawnObj.setfilteredImageClassData(tempImageSpawnObj.GetFilteredAllImageClassObj());
    }

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        if(LobbyController.isSinglePlayer)//singleplayer, disable timer and online chat
        {
            singlePlayerPanel.SetActive(true);
            GetComponent<TimerManager>().enabled = false;
            GetComponent<CountdownTimer>().enabled = false;
            timer.SetActive(false);
            PhotonChatManagerObj.SetActive(false);
        }
        else//multiplayer
        {
            multiPlayerPanel.SetActive(true);
        }
        DisplayNamesInChat();
        LoadImageData();

        //load new image
        LoadNewImage();
        if (!LobbyController.isSinglePlayer)
            TimerManager.StartTimerEvent?.Invoke();
        //show tip for first time player
        if(!PlayerPrefs.HasKey("Tip"))
        {
            PlayerPrefs.SetInt("Tip", 1);
            StartCoroutine(ShowTip());
        }
    }
    IEnumerator ShowTip()
    {
        tipText.SetActive(true);
        yield return new WaitForSeconds(5);
        tipText.SetActive(false);
    }
    void DisplayNamesInChat()
    {
        chatArea.text = "";
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            chatArea.text += player.NickName + ": joined\n";
        }
        if (!LobbyController.isAIEnable)
            chatArea.text += "ROUND " + roundNumber + " STARTED\n";
    }
    public void LoadNewImage()
    {
        if (roundNumber>1 && tempRoundCheck != roundNumber)//round 1 text is set before
        {
            chatArea.text += "-------------\n";
            tempRoundCheck = roundNumber;
            chatArea.text += "ROUND " + roundNumber + " STARTED\n";
        }
        if (PhotonNetwork.IsMasterClient && photonView.IsMine)
        {
            ImageSpawner.LoadImagesEvent?.Invoke();
            imageName = GetComponent<ImageSpawner>().imageName;
            imageNameLength = imageName.Length;
            currentImageID = GetComponent<ImageSpawner>().currentImageID;
        }
    }
    #region ROUND FUNCTIONS
    public IEnumerator CRDisplayUIAfterRound()
    {
        UIManager.instance.SetResolutionAlpha = 1;
        yield return new WaitForSeconds(GameUtility.WaitDelayTime); //wait while user sees pop up
        UIManager.instance.SetResolutionAlpha = 0;
        UIManager.instance.Effect.gameObject.SetActive(false);
    } 
    IEnumerator CRWaitAndLoadNextRound()
    {
        yield return new WaitForSeconds(GameUtility.WaitDelayTime); //wait before loading next round
        LoadNewImage();
        //reseting values
        PlayerSetup.ResetValuesEvent?.Invoke(); //reset player setup values
        if (LobbyController.isSinglePlayer)
        {
            skipBtn.enabled = true;
            singlePRoundNum.text = "Round : " + roundNumber;
            if(roundNumber==5)
            {
                GameOver.FeedbackEvent?.Invoke();//show feedback on round 5 for singleplayer
            }
        }
        else
        {
            TimerManager.StartTimerEvent?.Invoke();
            TimerManager.isTimeUp = false;
        }
        isAnsweredTwice = false;
        UIManager.instance.RPCDisableBlocker();
        if (LobbyController.isAIEnable)
        {
            AI.AIWonBool = false;
            AI.AICanAnswer = true;
        }
    }
    bool isAnsweredTwice = false;
    [PunRPC]
    public void NextRound()
    {
        if (isAnsweredTwice)
        {
            return;
        }
        //reset values before next round
        SaveImageID();
        if (LobbyController.isAIEnable)
        {
            AI.StopAIEvent?.Invoke();
        }
        UIManager.instance.RPCResetPanels();
        StopCoroutine(CRDisplayUIAfterRound());
        roundNumber++;
        isAnsweredTwice = true;
        if(LobbyController.isSinglePlayer)//singleplayer //unlimited rounds
        {
            StartCoroutine(CRWaitAndLoadNextRound());
        }
        else if(roundNumber <= GameUtility.NumberOfRounds) //multiplayer //if there are more rounds left
        {
            StartCoroutine(CRWaitAndLoadNextRound());
        }
        else//multiplayer //game over
        {
            if(!LobbyController.isSinglePlayer) //if multiplayer, timer functions since timer is disabled on singleplayer
            {
                CountdownTimer.EndRoundTimerEvent?.Invoke();
                TimerManager.ResetTimerEvent?.Invoke();
            }
            GameOver.GameOverEvent?.Invoke();
        }
    }
    public void SkipRound()//singleplayer
    {
        skipBtn.enabled = false;
        UIManager.UIEventAfterEachRound(0, "SKIPPED", false);
        chatArea.text += "SKIPPED\n";
        //Color orange = new Color(255, 165, 0);
        //singlePlayerRounds[roundCountForRoundButtons].GetComponent<Image>().color = orange;
        NextRound();
        PlayerSetup.IncrementRoundNumberEvent?.Invoke();
    }
    void SaveImageID()
    {
        //save played image id to the file, binary formatter
        //check Save class for reference
        List<ImageClass> tempList = new List<ImageClass>();
        Save saveObject = GetComponent<Save>();

        if (GetListOfImageClassPlayedObj() == null)
        {
            tempList.Add(new ImageClass(currentImageID));
        }
        else
        {
            tempList = GetListOfImageClassPlayedObj();
            tempList.Add(new ImageClass(currentImageID));
        }
        SetListOfImageClassPlayedObj(tempList);
        saveObject.SavePlayer(GetListOfImageClassPlayedObj());

    }
   
    public void RPCNextRound(PlayerSetup.Result result)
    {
        if (result == PlayerSetup.Result.WonLost)
        {
            photonView.RPC("NextRound", RpcTarget.AllBuffered);
        }
        else if (result == PlayerSetup.Result.Draw)//if draw both players call function for themselves
        {
            UpdateRoundColorDraw();
            NextRound();
        }
    }
    void UpdateRoundColorDraw()
    {
        //in case of draw both player's round boxes will turn to this color
        Color orange = new Color(255, 165, 0);
        p1Rounds[roundCountForRoundButtons].GetComponent<Image>().color = orange;
        p2Rounds[roundCountForRoundButtons].GetComponent<Image>().color = orange;
    }
    #endregion

    #region SMILING ANIMATIONS
    public void PlaySPSmile()
    {
        singlePFace.SetTrigger("Smile");
        //below part for cloud with emoticon, if required
        //bubbleS.SetActive(true);
        //if(singlePFace.GetCurrentAnimatorStateInfo(0).normalizedTime>1)
        //{
        //    bubbleS.SetActive(false);
        //}
    }
    public void PlayP1Smile()
    {
        p1Face.SetTrigger("Smile");
        //bubble1.SetActive(true); 
        //if (p1Face.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
        //{
        //    bubble1.SetActive(false);
        //}
    }
    public void PlayP2Smile()
    {
        p2Face.SetTrigger("Smile");
        //bubble2.SetActive(true);
        //if (p2Face.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
        //{
        //    bubble2.SetActive(false);
        //}
    }
    #endregion
}