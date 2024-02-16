using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Photon.Pun;
//THIS SCRIPT IS FOR FUNCTIONS THAT CAN BE USED AS DESIRED ANYWHERE
public static class Extensions
{
    public static string Scramble(this string s)
    {
        return new string(s.ToCharArray().OrderBy(x => Guid.NewGuid()).ToArray());
    }
    public static string RemoveDuplicates(string input)
    {
        return new string(input.ToCharArray().Distinct().ToArray());
    }
    public static string GetRandomString(string input)
    {
        int stringLength = input.Length;
        char firstLetter = input[0];
        char lastLetter = input[stringLength - 1];
        string tempString = firstLetter.ToString();
        char[] charArr = new char[stringLength];
        charArr[stringLength - 1] = lastLetter;
        for (int i = 1; i < stringLength - 1; i++)
        {
            int rand = UnityEngine.Random.Range(1, stringLength - 1);
            charArr[i] = input[rand];
            tempString += charArr[i];
        }
        tempString += lastLetter;
        return tempString;
    }

    public static float MultiplierBasedOnStringLength(int length)
    {
        float multiplier = 0;
        if(length <= 2)
        {
            multiplier = 0.5f;
        }
        else if(length>=3 && length<=6)
        {
            multiplier = 1f;
        }
        else if(length>=7)
        {
            multiplier = 1.5f;
        }
        return multiplier;
    }
    public static int GetStartPointBasedStringLength(int length)
    {
        int startPoint=0;
        if(length>=5)
        {
            startPoint = length / 2;
        }
        else
        {
            startPoint = 1;
        }
        return startPoint;
    }
}
