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
            str += String.Format("{0,6}", Math.Round(arr[i], 3).ToString());
        }
        return str;
    }

    public static void CopyElements<T, T1>(Dictionary<T, T1> dictionaryFrom, Dictionary<T, T1> dictionaryTo)
    {
        dictionaryFrom.ToList().ForEach(x => dictionaryTo.Add(x.Key, x.Value));
    }
}

static class HelperConstants
{
    public const int totalAmountOfSensors = 30;
    public const int totalAmountOfOutputsOfNet = 5;
    public const float speedMultOfWa = 0.2f;
    public const int complexityThreshold = 100;
    public const int warriorSpawnOffset = 4;
    public const float warriorRotationSpeed = 10f;
    public const float projectileSpeed = 0.6f;
    public const int evaluationsPerGeneration = 3;
    public const int ticksPerEvaluation = 1000;
    public const int warriorsPerPlayer = 50;
}

