using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Photon.Pun;
using Photon.Realtime;

public class GameEvents : MonoBehaviourPunCallbacks
{
    public static Action TimeUpEvent,GameOverPanelEvent;
    [SerializeField] GameObject PanelAfterEachRound, GameOverPanel, TimeUpPanel;

    private void OnEnable()
    {
        GameOverPanelEvent += PanelsOnGameOver;
        TimeUpEvent += OnTimeUp;
    }
    private void OnDisable()
    {
        GameOverPanelEvent -= PanelsOnGameOver;
        TimeUpEvent -= OnTimeUp;
    }
    void OnTimeUp()//this function is for the time up pop that appears when timer hits 0
    {
        TimeUpPanel.GetComponent<PopUpAnimation>().OnPopUp();
        TimeUpPanel.gameObject.SetActive(true);
    }
    
    void PanelsOnGameOver()//this is for when the game ends
    {
        PanelAfterEachRound.SetActive(false);
        GameOverPanel.SetActive(true);
        UIManager.instance.Effect.gameObject.SetActive(false);
    }
}