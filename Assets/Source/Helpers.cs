using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


static class Helpers
{
    public static string ArrayToString(double[] arr)
    {
        string str = "";
        for (int i = 0; i < arr.Length; i++)
        {
            str +=String.Format("{0,6}",Math.Round(arr[i],3).ToString());
        }
        return str;
    }
}

static class HelperConstants
{
    public const int totalAmountOfSensors = 12;
    public const int totalAmountOfOutputsOfNet = 5;
    public const float speedMultOfWa = 0.2f;
    public const int complexityThreshold = 100;
    public const int warriorSpawnOffset = 4;
}

