using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System;
//THIS IS THE SCRIPT FOR MANAGING THE UI AND IS ATTACHED TO GAME MANAGER GAMEOBJECT
public class UIManager : MonoBehaviourPun
{
    public static UIManager instance;
    [SerializeField] AudioSource timerSound, soundAudio;
    [SerializeField] GameObject settingsPanel, InputBlocker;
    [SerializeField] Button soundBtn, musicBtn; 
    bool musicToggle = false;
    bool soundToggle = false;

    [SerializeField] GameObject PanelAfterEachRound, GameOverPanel, TimeUpPanel;
    [SerializeField] CanvasGroup resolution, mainCanvas;
    [SerializeField] TMPro.TMP_Text scoreThisRoundText, CorrectIncorrectText;
    [SerializeField] Button restartBtn, exitBtn;
    public static Action<int,string,bool> UIEventAfterEachRound;
    public static Action EnableBlockerEvent, DisableBlockerEvent;
    [SerializeField] ParticleSystem effect;
    public ParticleSystem Effect { get { return effect; } set { effect = value; } }
    public int SetResolutionAlpha { set{ resolution.alpha = value; } }
    private void OnEnable()
    {
        EnableBlockerEvent += RPCEnableBlocker;
        DisableBlockerEvent += DisableBlocker;
        UIEventAfterEachRound += OnAnsweredUpdateUI;
    }
    private void OnDisable()
    {
        EnableBlockerEvent -= RPCEnableBlocker;
        DisableBlockerEvent -= DisableBlocker;
        UIEventAfterEachRound -= OnAnsweredUpdateUI;
    }
    private void Start()
    {
        instance = this;
        resolution.alpha = 0;
        //backGroundMusic = GameObject.FindGameObjectWithTag("bgMusic").GetComponent<AudioSource>();
        soundAudio = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioSource>();
    }
    public void SoundToggle()
    {
        //toggling sound 
        Color color = soundBtn.image.color;
        color.a = 0.5f;
        soundToggle = !soundToggle;
        if (soundToggle)
        {
            color.a = 0.5f;
            soundAudio.mute = true;
            timerSound.mute = true;
            soundBtn.image.color = color;
        }
        else
        {
            color.a = 1;
            soundAudio.mute = false;
            timerSound.mute = false;
            soundBtn.image.color = color;
        }
    }
    //public void MusicToggle()
    //{
    //    Color color = musicBtn.image.color;
    //    color.a = 0.5f;
    //    musicToggle = !musicToggle;
    //    if (musicToggle)
    //    {
    //        color.a = 0.5f;
    //        backGroundMusic.mute=true;
    //        musicBtn.image.color = color;
    //    }
    //    else
    //    {
    //        color.a = 1;
    //        backGroundMusic.mute = false;
    //        musicBtn.image.color = color;
    //    }
    //}
    public void onSettingsClicked()
    {
        if(settingsPanel.activeSelf)
        {
            settingsPanel.SetActive(false);
        }
        else
        {
            settingsPanel.SetActive(true);
        }
    }
    [PunRPC]
    public void EnableBlocker()
    {
        InputBlocker.SetActive(true);
        InputBlocker.transform.SetAsLastSibling();
    }
    [PunRPC]
    public void DisableBlocker()
    {
        InputBlocker.SetActive(false);
        //PanelAfterEachRound.SetActive(false);
    }
    public void RPCEnableBlocker()
    {
        photonView.RPC("EnableBlocker", RpcTarget.All);
    }
    public void RPCDisableBlocker()
    {
        if(PhotonNetwork.IsMasterClient)
            photonView.RPC("DisableBlocker", RpcTarget.All);
    }
    void OnAnsweredUpdateUI(int scoreThisRound, string correctIncorrectText, bool correct)
    {
        EnableBlocker(); //enable blocker so user cant touch the screen
        PanelAfterEachRound.GetComponent<PopUpAnimation>().OnPopUp();
        PanelAfterEachRound.SetActive(true);
        CorrectIncorrectText.text = correctIncorrectText;
        scoreThisRoundText.text = "" + scoreThisRound;
        if (correct)
        {
            effect.gameObject.SetActive(true);
        }
        else
        {
            effect.gameObject.SetActive(false);
        }
    }
    [PunRPC]
    public void ResetPanels()
    {
        TimeUpPanel.SetActive(false);
    }
    public void RPCResetPanels()
    {
        photonView.RPC("ResetPanels", RpcTarget.AllBuffered);
    }
}
