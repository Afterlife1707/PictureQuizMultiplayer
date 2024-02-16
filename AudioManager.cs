using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//THIS IS THE SCRIPT FOR MANAGING THE SOUND EFFECTS IN THE GAME
//FOR TOGGLING THE MUSIC/SOUNDS ALL TOGETHER, IT IS IN UIMANAGER.CS, AS A BUTTON CLICK IN THE SETTINGS TAB 
public class AudioManager : MonoBehaviour
{
    [SerializeField]
    public AudioSource A_Source;
    [SerializeField]
    AudioClip roundWon, roundLost, gameLost, gameWon, alphabetClicked, timerTick;
    public static AudioManager instance;

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
        DontDestroyOnLoad(gameObject);
    }
    public void PlaySoundTrack(AudioEnum audio)
    {
        //Stop any playing music
        A_Source.Stop();
        //play the audio depending on the case
        switch (audio)
        {
            case AudioEnum.roundWon:
                A_Source.PlayOneShot(roundWon);
                break;

            case AudioEnum.roundLost:
                A_Source.PlayOneShot(roundLost);
                break;

            case AudioEnum.gameLost:
                A_Source.PlayOneShot(gameLost);
                break;

            case AudioEnum.gameWon:
                A_Source.PlayOneShot(gameWon);
                break;
            case AudioEnum.alphabetClicked:
                A_Source.PlayOneShot(alphabetClicked);
                break;

        }
    }
}
