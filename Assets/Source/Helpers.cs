using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using ue = UnityEngine;

public static class Helpers
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

    public static float AvgSpeedWithPositions(Deque<ue.Vector2> deque)
    {
        float distance = 0;
        for (int i = 0; i < deque.Length - 1; i++)
        {
            distance += (deque[i] - deque[i + 1]).magnitude;
        }
        if (deque.Length > 0)
        {
            return distance / deque.Length;
        }
        return 0;
    }

    #region Normalize overloads

    /// <param name="x">to normalize</param>
    /// <param name="minX">min value of x</param>
    /// <param name="maxX">max value of x</param>
    /// <param name="isZeroOneRange">if true then normalizes to 0..1, else - -1..1</param>
    /// <returns></returns>
    public static double NormalizeNumber(double x, double minX, double maxX, bool isZeroOneRange = true)
    {
        Debug.Assert(minX <= x && x <= maxX);

        double ans = (x - minX) / (maxX - minX);
        if (!isZeroOneRange)
        {
            ans = -1 + 2 * ans;
            Debug.Assert(-1 <= ans && ans <= 1);
        }
        else
        {
            Debug.Assert(0 <= ans && ans <= 1);
        }
        return ans;
    }

    /// <param name="x">to normalize</param>
    /// <param name="min">min value of x</param>
    /// <param name="max">max value of x</param>
    /// <param name="isZeroOneRange">if true then normalizes to 0..1, else - -1..1</param>
    /// <returns></returns>
    public static float NormalizeNumber(float x, float min, float max, bool isZeroOneRange = true)
    {
        Debug.Assert(min <= x && x <= max, $"{x.ToString()} not in range {min}..{max}");

        float ans = (x - min) / (max - min);
        if (!isZeroOneRange)
        {
            ans = -1 + 2 * ans;
            Debug.Assert(-1 <= ans && ans <= 1, $"{ans.ToString()} not in range -1..1");
        }
        else
        {
            Debug.Assert(0 <= ans && ans <= 1, $"{ans.ToString()} not in range 0..1");
        }
        return ans;
    }

    #endregion Normalize overloads

    public class Deque<T>
    {
        private List<T> internalList;
        private int size;

        public int Length { get; private set; }

        public Deque(int size)
        {
            internalList = new List<T>(size);
            this.size = size;
            Length = 0;
        }

        public void Add(T elem)
        {
            internalList.Add(elem);
            if (internalList.Count > size)
            {
                internalList.RemoveAt(0);
            }
            Length = internalList.Count;
        }

        public T this[int index]
        {
            get
            {
                return internalList[index];
            }
        }
    }
}

public static class HelperConstants
{
    public const string saveDirectory = "/save/";
    public const int totalAmountOfSensors = 17;
    public const int totalAmountOfOutputsOfNet = 4;

    public const int dataRefreshFrequency = 3;
    public static float speedMultOfWa = 0.2f;
    public static float projectileAccel = 0.6f;

    public static int complexityThreshold = 500;
    public static int warriorSpawnOffset = 4;
    public static float warriorRotationSpeed = 2f;

    public const float warrPushForce = 0.4f;

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

    public static float brushSize = 0.6f;

    public const string warriorTag = "Warrior";
    public const string obstacleTag = "Obstacle";
    public const string projectileTag = "Projectile";
    public const string tileTag = "Tile";
}
