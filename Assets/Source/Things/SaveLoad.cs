using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class SaveLoad
{
    public static List<string> savedGames = new List<string>();

    public static void Load()
    {
        savedGames.Clear();

        string[] fileEntries = Directory.GetFiles(Application.dataPath + HelperConstants.saveDirectory);
        foreach (string fileName in fileEntries)
        {
            savedGames.Add(Path.GetFileName(fileName));
        }
    }
}