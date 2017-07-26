using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CSVConvert
{
    public static string[] ConvertCSVHead(string CSVText)
    {
        string headText = CSVText.Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries)[0];

        return headText.Split(',');
    }

    public static string[][] ConvertCSVBody(string CSVText)
    {
        string[][] returnValue;

        string[] bodyText = CSVText.Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        if (bodyText.Length <= 0)
        {
            return null;
        }

        int cnt = 0;
        for (int i = 0; i < bodyText.Length; i++)
        {
            if (string.IsNullOrEmpty(bodyText[i]))
            {
                continue;
            }
            cnt++;
        }

        returnValue = new string[cnt][];

        int pointer = 0;

        for (int i = 0; i < bodyText.Length; i++)
        {
            if (string.IsNullOrEmpty(bodyText[i]))
            {
                continue;
            }
            returnValue[pointer] = bodyText[pointer].Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            pointer++;

        }
        return returnValue;
    }

    public static T[] ConvertToArray<T>(string arrayText)
    {
        T[] returnValue;

        string[] array = arrayText.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);

        int cnt = 0;

        for (int i = 0; i < array.Length; i++)
        {
            if (!string.IsNullOrEmpty(array[i]))
            {
                cnt++;
            }
        }

        returnValue = new T[cnt];

        int pointer = 0;

        for (int i = 0; i < array.Length; i++)
        {
            if (!string.IsNullOrEmpty(array[i]))
            {
                continue;
            }

            pointer++;
            returnValue[pointer] = (T)Convert.ChangeType(array[i], typeof(T));
        }

        return returnValue;
    }
}
