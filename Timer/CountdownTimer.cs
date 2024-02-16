using Photon.Pun;
using UnityEngine;
using System;
using TMPro;

//THIS SCRIPT WAS DERIVED FROM THE COUNTDOWNTIMER BY PHOTON UTILITY SCRIPTS, CAN BE FOUND IN Photon\PhotonUnityNetworking\UtilityScripts\Room
//IT IS FOR THE WORKING OF THE COUNTDOWN TIMER WHICH IS CALLED IN THE TIMER MANAGER
public class CountdownTimer : MonoBehaviourPunCallbacks
{
    public delegate void CountdownTimerHasExpired();
    public static event CountdownTimerHasExpired OnCountdownTimerHasExpired;
    public static Action EndRoundTimerEvent;

    public const string CountdownStartTime = "StartTime";

    [HideInInspector] public float TimeLeft;
    private float timeRemaining;
    public float TimeRemaining { get { return timeRemaining; } set { timeRemaining = value; } }

    [HideInInspector] public bool isTimerRunning;

    private int startTime;
    [SerializeField] int timeToWaitForAI;
    [Header("Reference to a Text component for visualizing the countdown")]
    public TMP_Text Text;

    public static CountdownTimer instance;
    TimerData timerData = new TimerData();
    public void Start()
    {
        instance = this;
        TimeLeft = timerData.TimeLeft;
        if (this.Text == null) Debug.LogError("Reference to 'Text' is not set. Please set a valid reference.", this);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        EndRoundTimerEvent += EndRoundTimer;
        Initialize();
    }

    public override void OnDisable()
    {
        base.OnDisable();
        EndRoundTimerEvent -= EndRoundTimer;
    }


    public void Update()
    {
        //if timer paused, return
        if (!this.isTimerRunning) return;
        timeRemaining = GetTimeRemaining();
        this.Text.text = "" + (int)timeRemaining;
        if (LobbyController.isAIEnable)
        {
            //START AI AFTER DELAY
            float timeAfterAIStarts = timerData.TimeLeft - timeToWaitForAI;
            if (timeRemaining <= timeAfterAIStarts && !AI.AIRunning)
            {
                AI.StartAIEvent?.Invoke();
            }
        }
        if (timeRemaining > 1f) return;
        OnTimerEnds();//if time remaining<0, end timer
    }

    void EndRoundTimer()
    {
        this.isTimerRunning = false;
        this.Text.text = "" + 0;
        photonView.RPC("RPCSetTimeUpBool", RpcTarget.All);
    }
    private void OnTimerRuns()
    {
        this.isTimerRunning = true;
        this.enabled = true;
    }
    [PunRPC]
    void RPCSetTimeUpBool()
    {
        TimerManager.isTimeUp = true;
    }

    private void OnTimerEnds()
    {
        UIManager.EnableBlockerEvent?.Invoke();
        TimerManager.isTimeUp = false;
        this.isTimerRunning = false;
        this.Text.text = "" + 0;
        if (OnCountdownTimerHasExpired != null) OnCountdownTimerHasExpired();
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
       // Debug.Log("CountdownTimer.OnRoomPropertiesUpdate " + propertiesThatChanged.ToStringFull());
        Initialize();
    }


    private void Initialize()
    {
        int propStartTime;
        TimeLeft = timerData.TimeLeft;
        if (TryGetStartTime(out propStartTime))
        {
            this.startTime = propStartTime;
           // Debug.Log("Initialize sets StartTime " + this.startTime + " server time now: " + PhotonNetwork.ServerTimestamp + " remain: " + TimeRemaining());


            this.isTimerRunning = GetTimeRemaining() > 0;
            if (this.isTimerRunning)
                OnTimerRuns();
            else
                OnTimerEnds();
        }
    }


    private float GetTimeRemaining()
    {
        int timer = PhotonNetwork.ServerTimestamp - this.startTime;
        
        return this.TimeLeft - timer / 1000f;
    }


    public static bool TryGetStartTime(out int startTimestamp)
    {
        startTimestamp = PhotonNetwork.ServerTimestamp;

        object startTimeFromProps;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(CountdownStartTime, out startTimeFromProps))
        {
            startTimestamp = (int)startTimeFromProps;
            return true;
        }

        return false;
    }

    public static void SetStartTime()
    {
        int startTime = 0;
        bool wasSet = TryGetStartTime(out startTime);

        
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
            {
                {CountdownStartTime, (int)PhotonNetwork.ServerTimestamp}
            };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);


       // Debug.Log("Set Custom Props for Time: " + props.ToStringFull() + " wasSet: " + wasSet);
    }
}

