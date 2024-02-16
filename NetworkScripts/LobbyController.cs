using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using EasyUI.Toast;
using UnityEngine.Analytics;
//THIS SCRIPT IS FOR CONNECTING TO PHOTON AND LOADING THE GAME SCENE
public class LobbyController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject inputNamePanel = null;
    [SerializeField] private GameObject waitingStatusPanel = null;
    [SerializeField] private TextMeshProUGUI waitingStatusText = null;
    [SerializeField] private TMP_InputField inputTextField = null;
    [SerializeField] GameObject HowToPlay;
    private bool lookForPlayer = false, hasStartedSearching = false;
    public static bool isAIEnable = false, isSinglePlayer = false, otherPlayerLeft = false, selfDisconnected = false, showFeedback = false;
    [SerializeField] private const string GameVersion = "1.1.3";
    private const int MaxPlayersPerRoom = 2;
    [SerializeField]
    float lookForPlayerTime = 5;
    RectTransform inputPanelRect;
    Vector3 startPosInputPanel;
    float divideBy = 2f;
    DisconnectRecovery disconnectRecovery = new DisconnectRecovery();

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        //level system
        //if (!PlayerPrefs.HasKey("Experience"))
        //{
        //    PlayerPrefs.SetInt("Experience", 0);
        //}
        //if (!PlayerPrefs.HasKey("Level"))
        //{
        //    PlayerPrefs.SetInt("Level", 0);
        //}
        //else
        //{
        //    Debug.Log("Lvl "+PlayerPrefs.GetInt("Level"));
        //    Debug.Log(PlayerPrefs.GetInt("Experience")+" XP");
        //}
    }
    private void Start()
    {
        Screen.orientation = ScreenOrientation.AutoRotation;
        inputPanelRect = inputNamePanel.GetComponent<RectTransform>();
        startPosInputPanel = inputPanelRect.position;
        isSinglePlayer = false;
        isAIEnable = false;
        if (otherPlayerLeft)
        {
            otherPlayerLeft = false;
            disconnectRecovery.ResetDataOnDisconnect(PhotonNetwork.LocalPlayer);
            Toast.Show("Other Player Disconnected",2f);
           // FindOpponent();//work on this later
        }
        if (selfDisconnected)
        {
            selfDisconnected = false;
            Toast.Show("Disconnected", 3f);
        }
        
    }

    //bool OpJoinRandomOrCreateRoom;
    #region BUTTON FUNCTIONS
    public void StartGame() //called on multiplayer button
    {
        AnalyticsResult analyticsResult = Analytics.CustomEvent("MultiPlayerClicked");
        Debug.Log(analyticsResult + " multiplayer");
        PhotonNetwork.NickName = inputTextField.text;
        inputNamePanel.SetActive(false);
        waitingStatusPanel.SetActive(true);
        hasStartedSearching = true;
        waitingStatusText.text = "Connecting...";
        //if connected to photon network
        if (PhotonNetwork.IsConnected)
        {
            //OpJoinRandomOrCreateRoom = PhotonNetwork.NetworkingClient.OpJoinRandomOrCreateRoom(null, null); //look into this later
            //if(!OpJoinRandomOrCreateRoom)
                PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.GameVersion = GameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public void StartSinglePlayer()//called on singleplayer button
    {
        AnalyticsResult analyticsResult = Analytics.CustomEvent("SinglePlayerClicked");
        Debug.Log(analyticsResult + " singleplayer");
        isSinglePlayer = true;
        PhotonNetwork.OfflineMode = true;
        PhotonNetwork.NickName = inputTextField.text;
    }

    #endregion

    #region PUN CALLBACKS
    public override void OnConnectedToMaster()//connect to master
    {
        PhotonNetwork.JoinRandomRoom();
        //OpJoinRandomOrCreateRoom = PhotonNetwork.NetworkingClient.OpJoinRandomOrCreateRoom(null, null); //look into this later
        //if (!OpJoinRandomOrCreateRoom)
        //    PhotonNetwork.JoinRandomRoom();
        waitingStatusText.text = "Connected To Server";
    }

    public override void OnDisconnected(DisconnectCause cause) //called when matchmaking cancelled by player by pressing X button
    {
        Debug.Log("Ondisconnected from lobbycontroller" + cause);
        waitingStatusPanel.SetActive(false);
        inputNamePanel.SetActive(true);
        HowToPlay.SetActive(false);
    }
    public override void OnJoinRandomFailed(short returnCode, string message)//after connecting to master if join room fail
    {
        if(PhotonNetwork.CountOfPlayers/ divideBy != 0 )
        {
            byte maxPlayers = (byte)MaxPlayersPerRoom;
            string roomName = "Room" + Random.Range(1000, 10000);
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.PlayerTtl = 0;
            roomOptions.EmptyRoomTtl = 0;
            roomOptions.MaxPlayers = maxPlayers;
            roomOptions.PublishUserId = true;
            roomOptions.IsOpen = true;
            roomOptions.IsVisible = true;
            roomOptions.CleanupCacheOnLeave = false;
            PhotonNetwork.CreateRoom(roomName, roomOptions);
        }
        else
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinedRoom()//when we join a room
    {
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        if(isSinglePlayer)
        {
            RoomController.StartGameEvent();
            return;
        }
        if (playerCount != MaxPlayersPerRoom)
        {
            waitingStatusText.text = "Looking For Opponent...";
            HowToPlay.SetActive(true);
            lookForPlayer = true;
        }
        else
        {
            //waitingStatusText.text = "Opponent Found";
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)//when other player enters existing room
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == MaxPlayersPerRoom)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;//close room so no more players can join
            lookForPlayer = false;
            waitingStatusText.text = "Opponent Found";

            RoomController.StartGameEvent();
        }
    }
    #endregion

    private void Update()
    {
        //look for player for the assigned time, if player is found onPlayerEnteredRoom callback is called else start the ai mode
        if (lookForPlayer && !isSinglePlayer)
        {
            if (lookForPlayerTime > 0)
            {
                lookForPlayerTime -= Time.deltaTime;
            }
            else // AI MODE
            {
                //PhotonNetwork.CurrentRoom.IsOpen = false;
                waitingStatusText.text = "Opponent Found";
                lookForPlayerTime = 0;
                lookForPlayer = false;
                isAIEnable = true;
                RoomController.StartGameEvent();
            }
        }
        if (TouchScreenKeyboard.visible)
        {
            inputPanelRect.localPosition = new Vector3(inputPanelRect.localPosition.x, 420, inputPanelRect.localPosition.z);
        }
        else
        {
            inputPanelRect.position = startPosInputPanel;
        }
        if (Application.internetReachability == NetworkReachability.NotReachable && hasStartedSearching)
        {
            ResetMainMenu();
        }
            //Below is for exiting the game on clicking the back button of android twice 
            //Check input for the first time
            if (Input.GetKeyDown(KeyCode.Escape) && !clickedBefore)
        {
            Debug.Log("Back Button pressed for the first time");
            //Set to false so that this input is not checked again. It will be checked in the coroutine function instead
            clickedBefore = true;
            Toast.Show("Press back again to exit", 2f);
            //Start quit timer
            StartCoroutine(quitingTimer());
        }
    }
    private bool clickedBefore = false;

    IEnumerator quitingTimer()
    {
        //Wait for a frame so that Input.GetKeyDown is no longer true
        yield return null;

        //3 seconds timer
        const float timerTime = 3f;
        float counter = 0;

        while (counter < timerTime)
        {
            //Increment counter while it is < timer time(3)
            counter += Time.deltaTime;

            //Check if Input is pressed again while timer is running then quit/exit if is
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }

            //Wait for a frame so that Unity does not freeze
            yield return null;
        }
        //Reset clickedBefore so that Input can be checked again in the Update function
        clickedBefore = false;
    }

    public void ResetMainMenu()
    {
        hasStartedSearching = false;
        disconnectRecovery.ResetDataOnDisconnect(PhotonNetwork.LocalPlayer);
        UnityEngine.SceneManagement.SceneManager.LoadScene((int)SceneIndexes.MAINMENU);
    }
}