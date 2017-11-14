using System;
using System.Collections.Generic;
using System.Linq;
using ue = UnityEngine;

internal static class Helpers
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

    public static ue.Vector2 RandomVector2()
    {
        return new ue.Vector2(ue.Random.Range(-1f, 1f), ue.Random.Range(-1f, 1f)).normalized;
    }
}

internal static class HelperConstants
{
    public const string saveDirectory = "/save/";
    public const int totalAmountOfSensors = 30;
    public const int totalAmountOfOutputsOfNet = 5;
    public static float speedMultOfWa = 0.15f;
    public static int complexityThreshold = 200;
    public static int warriorSpawnOffset = 4;
    public static float warriorRotationSpeed = 10f;
    public static float projectileSpeed = 0.4f;
    public static int evaluationsPerGeneration = 3;
    public static int ticksPerEvaluation = 1000;

    public static float fitnessBonusForDyingFromEnemy = 0;//0.4f
    public static float fitnessPenaltyForKillingAlly = 0;//-0.4f
    public static float fitnessForKillingAnEnemy = 0;//1f
    public static float fitnessMultiplierForApproachingToFlag = 0;//0.003f
    //public static float fitnessMultiplierForBeingNearFlag = 0;//0.00001f
    //public static int warriorsPerPlayer = 50;

    //public const int minNumberOfWarriors = 10;
    //public const int minSizeOfMap = 10;

    public static float brushSize = 1;
}
