using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AlphabetAnswerBoxClick : MonoBehaviour
{
    public void OnAlphabetBoxClicked()
    {
        //input alphabet on click

        AudioManager.instance.PlaySoundTrack(AudioEnum.alphabetClicked);
        string tempAlphabet = GetComponentInChildren<TMP_Text>().text;

        GameObject playerObj = this.transform.parent.parent.gameObject;
        PlayerSetup playerSetup = playerObj.GetComponent<PlayerSetup>();
        AlphabetInput alphabetInput = playerObj.GetComponent<AlphabetInput>();
        alphabetInput.AddToSpaces(tempAlphabet);

        //ai logic
        //start the ai if user has typed till midway of the answer and is correct so far
        if (LobbyController.isAIEnable && !AI.AIRunning)
        {
            string imageName = playerSetup.myGameManager.ImageName;
            int startPoint = (int)Extensions.GetStartPointBasedStringLength(imageName.Length); //at what point the ai will start
            if (alphabetInput.answerBoxNumber == playerSetup.myGameManager.ImageNameLength - startPoint)//if user has reached the start point
            {
                string tempAnswer = "";
                string correctAnsWithoutRest = imageName.Remove(imageName.Length - startPoint, startPoint); // substring the image name to halfway
                int answerBoxIndex = (int)PlayerSetup.UserPrefabChildren.AnswerBoxes;
                Transform thisPlayerAnsBoxTrans = playerSetup.transform.GetChild(answerBoxIndex);
                
                for (int i = 0; i < thisPlayerAnsBoxTrans.childCount - startPoint; i++)
                {
                    tempAnswer += thisPlayerAnsBoxTrans.GetChild(i).GetComponentInChildren<TMP_Text>().text;//fetch what user has typed so far without the rest of the word
                    if (tempAnswer == correctAnsWithoutRest)
                    {
                        AI.StartAIEvent?.Invoke();
                        break;
                    }
                }
            }
        }
    }

    public void ClearAnswerBoxText()
    {
        //clear the answer box on click
        if (this.GetComponentInChildren<TMP_Text>().text != "")
        {
            int alphabetIndex = this.transform.GetSiblingIndex();
            GetComponentInChildren<TMP_Text>().text = "";
            GameObject playerObj = this.transform.parent.parent.gameObject;
            AlphabetInput alphabetInput = playerObj.GetComponent<AlphabetInput>();
            alphabetInput.AnswerBoxDict[alphabetIndex] = null;
            alphabetInput.answerBoxNumber = alphabetIndex;
        }
    }
}
