using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System;

//THIS SCRIPT IS FOR THE TIMER RELATED EVENTS
public class TimerManager : MonoBehaviourPunCallbacks
{
    [SerializeField] GameManager gameManager;
    [SerializeField] AudioSource timerSound;
    [SerializeField] AudioClip timerClick;
    public static Action StartTimerEvent, ResetTimerEvent;
    private Coroutine cr_TimerSound;
    public static bool isTimeUp = false;
    public override void OnEnable()
    {
        base.OnEnable();
        StartTimerEvent += StartTimer;
        ResetTimerEvent += ResetTimer;
        CountdownTimer.OnCountdownTimerHasExpired += TimeUp;
    }
    public override void OnDisable()
    {
        base.OnDisable();

        StartTimerEvent -= StartTimer;
        ResetTimerEvent -= ResetTimer;
        CountdownTimer.OnCountdownTimerHasExpired -= TimeUp;
    }
    private void StartTimer()//start timer called each round
    {
        // if(gameManager.RoundNumber>1)
        CountdownTimer.SetStartTime();
        if (cr_TimerSound == null)
        {
            cr_TimerSound = StartCoroutine(PlaySound());
        }
    }
    private void ResetTimer()//reset the coroutine
    {
        isTimeUp = true;
        if (cr_TimerSound != null) StopCoroutine(cr_TimerSound);
        cr_TimerSound = null;
    }
    public IEnumerator PlaySound()
    {
       yield return new WaitForSeconds(1f);
       timerSound.PlayOneShot(timerClick);
       if (CountdownTimer.instance.TimeLeft > 0 && CountdownTimer.instance.isTimerRunning)
        {
            cr_TimerSound = StartCoroutine(PlaySound());
        }

    }
    //private void Update()
    //{
    //    if(Input.GetKeyUp(KeyCode.Space))
    //    {
    //        StartTimerEvent?.Invoke();
    //    }
    //}
    public void Start()
    {
        isTimeUp = false;
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
            {
                {"PlayerLoadedLevel", true}
            };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }
    void TimeUp()
    {
        ResetTimer();
        UIManager.EnableBlockerEvent?.Invoke();
        GameEvents.TimeUpEvent?.Invoke();
        //GameEvents.instance.RPCTimeUp();
        gameManager.chatArea.text += "TIME UP\n";
        //LOOK INTO THIS LATER
        Color orange = new Color(255, 165, 0);
        gameManager.p1Rounds[gameManager.RoundCountForRoundButtons].GetComponent<Image>().color = orange;
        gameManager.p2Rounds[gameManager.RoundCountForRoundButtons].GetComponent<Image>().color = orange;
        StartCoroutine(CR_TimeUp());
    }
    IEnumerator CR_TimeUp()
    {
        yield return new WaitForSeconds(GameUtility.WaitDelayTime);
        gameManager.RoundCountForRoundButtons++;
        gameManager.NextRound();
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        // if there was no countdown yet, the master client (this one) waits until everyone loaded the level and sets a timer start
        int startTimestamp;
        //bool startTimeIsSet = Photon.Pun.UtilityScripts.CountdownTimer.TryGetStartTime(out startTimestamp);
        bool startTimeIsSet = CountdownTimer.TryGetStartTime(out startTimestamp);

        if (changedProps.ContainsKey("PlayerLoadedLevel"))
        {
            if (CheckAllPlayerLoadedLevel())
            {
                if (!startTimeIsSet)
                {
                    CountdownTimer.SetStartTime();
                    //Photon.Pun.UtilityScripts.CountdownTimer.SetStartTime();
                }
            }
            else
            {
                // not all players loaded yet. wait:
            }
        }

    }
    private bool CheckAllPlayerLoadedLevel()
    {
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            object playerLoadedLevel;

            if (p.CustomProperties.TryGetValue("PlayerLoadedLevel", out playerLoadedLevel))
            {
                if ((bool)playerLoadedLevel)
                {
                    continue;
                }
            }

            return false;
        }

        return true;
    }
}
