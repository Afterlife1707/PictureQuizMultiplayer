using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBar : MonoBehaviour
{
    public int maximum, current;
    [SerializeField] Image progress;
    [SerializeField] TMP_Text levelText;
    void Start()
    {
        SetInitialValue();
    }
    void SetInitialValue()
    {
        if(PlayerPrefs.HasKey("Experience"))
        {
            int tempXP = PlayerPrefs.GetInt("Experience");
            int tempLevel = PlayerPrefs.GetInt("Level");
            current = tempXP - (tempLevel * 100);
            levelText.text = ""+tempLevel;

            FillCurrent();
        }
    }
    void FillCurrent()
    {
        float fillAmount = (float)current / (float)maximum;
        progress.fillAmount = fillAmount;
    }
}
