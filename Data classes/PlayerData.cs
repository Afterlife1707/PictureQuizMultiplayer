using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData 
{
    private string _PlayerName;
    private int _PlayerID;
    private int _PlayerProgressLvl;
    static int arrSize = 50;
    public string PlayerName { get { return _PlayerName; } set { _PlayerName = value; } }
    public int PlayerID { get { return _PlayerID; } set { _PlayerID = value; } }
    public int PlayerProgressLvl { get { return _PlayerProgressLvl; } set { _PlayerProgressLvl = value; } }
    public int[] imagesPlayedIDArr;

    public PlayerData(PlayerSetup playerSetupData)
    {
        //PlayerProgressLvl = playerSetupData.myGameManager.currentLevelID;
        //imagesPlayedIDArr = new int[arrSize];
        //for (int i = 0; i < imagesPlayedIDArr.Length; i++)
        //{
        //    Debug.Log("array for loop");
        //    if (imagesPlayedIDArr[i] == 0)
        //    {
        //        imagesPlayedIDArr[i] = playerSetupData.currentImageID;
        //        Debug.Log("id saved: "+playerSetupData.currentImageID);
        //        break;
        //    }
        //}
    }
}
