using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Analytics;
using Photon.Pun;
using System;
//THIS SCRIPT IS ATTACHED TO THE INPUT PANEL IN THE MAIN LOBBY SCENE
//IT IS FOR ENABLING THE START BUTTON IF USER HAS TYPED SOMETHING AND THE NAME IS SAVED IN PLAYERPREFS
public class PlayerInput : MonoBehaviour
{
    [SerializeField] TMP_InputField nameInputField = null;
    [SerializeField] Button singleBtn=null,multiPBtn = null;
    private const string PlayerPrefsNameKey = "PlayerName";

    void Start()
    {
        SetUpInputField();
    }
    void SetUpInputField()
    {
        //check if existing playerpref name
        if (!PlayerPrefs.HasKey(PlayerPrefsNameKey))
        {
            return;
        }
        string defaultName = PlayerPrefs.GetString(PlayerPrefsNameKey);//PlayerName
        nameInputField.text = defaultName;
        SetPlayerName(defaultName);
    }

    public void SetPlayerName(string name)
    {
        //if no name start button is not interactable
        singleBtn.interactable = !string.IsNullOrEmpty(name);
        multiPBtn.interactable = !string.IsNullOrEmpty(name);
    }

    public void SavePlayerName()
    {
        //on value changed in input field
        string playerName = nameInputField.text;
        PhotonNetwork.NickName = playerName;
        PlayerPrefs.SetString(PlayerPrefsNameKey, playerName);
    }
    public void SendCustomEvent()
    {
        AnalyticsResult analyticsResult = Analytics.CustomEvent("InputFieldUsed");//custom event sent for analytics
        Debug.Log(analyticsResult+" inputField");
    }
}
