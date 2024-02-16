using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
//THIS IS THE SCRIPT FOR THE USER TYPING WHEN CLICKING ON THE ALPHABET BOXES
//VALUES ARE STORED IN A DICTIONARY SO THAT IT CAN BE EASILY CLEARED AND USER CAN TYPE AGAIN FROM THE CLEARED POSITION
public class AlphabetInput : MonoBehaviour
{
    [SerializeField] PlayerSetup playerSetup;
    [HideInInspector]public int answerBoxNumber;
    Dictionary<int, string> answerBoxDict = new Dictionary<int, string>();
    public Dictionary<int, string> AnswerBoxDict { get { return answerBoxDict; } set { answerBoxDict = value; } }
    public void ClearDict()
    {
        answerBoxDict.Clear();
    }
    public void AddToSpaces(string s) //this function adds the alphabets to the answer spaces on user input click
    {
        //add user input to answer boxes and values in the dictionary
        for (int i = 0; i < answerBoxDict.Count; i++)
        {
            //insert value into the first key with empty value
            if (answerBoxDict[i] == null)
            {
                int answerBoxIndex = (int)PlayerSetup.UserPrefabChildren.AnswerBoxes;
                Transform answerBoxesTransform = playerSetup.transform.GetChild(answerBoxIndex);
                answerBoxesTransform.GetChild(i).GetComponentInChildren<TMP_Text>().text = s;//set text to the answer boxes
                answerBoxDict[i] = s;
                answerBoxNumber = i;
                if (i == GameManager.instance.ImageNameLength - 1) //if last box reached
                {
                    playerSetup.OnAllSpacesFilled();
                    break;
                }
                break;
            }
        }
    }
}
