using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSystem 
{
    //Level system is linear for now
    private int level;
    public int Level { get { return level; } set { level = value; } }
    private int currentExperience;
    public int CurrentExperience { get { return currentExperience; } set { currentExperience = value; } }
    private int experienceToNextLevel;
    
    public LevelSystem()
    {
        level = 0;
        currentExperience = 0;
        experienceToNextLevel = 100;
    }
    public void AddExperience(int amount)
    {
        currentExperience += amount;
        int tempXP = currentExperience;
        if(currentExperience % 100 == 0 && currentExperience != 0)
        {
            //next lvl
            level++;
            //currentExperience -= experienceToNextLevel;
        }
    }
}
