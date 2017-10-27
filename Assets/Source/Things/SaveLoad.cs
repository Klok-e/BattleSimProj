using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public static class SaveLoad
{
    public static List<string> savedGames = new List<string>();

    //it's static so we can call it from anywhere
    public static void Save(string toSave)
    {
        SaveLoad.savedGames.Add(toSave);
        BinaryFormatter bf = new BinaryFormatter();
        //Application.persistentDataPath is a string, so if you wanted you can put that into debug.log if you want to know where save games are located
        FileStream file = File.Create(Application.dataPath + "/populationFileNames.nms"); //you can call it anything you want
        bf.Serialize(file, SaveLoad.savedGames);
        file.Close();
    }

    public static void Load()
    {
        if (File.Exists(Application.dataPath + "/populationFileNames.nms"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.dataPath + "/populationFileNames.nms", FileMode.Open);
            SaveLoad.savedGames = (List<string>)bf.Deserialize(file);
            file.Close();
        }
    }
}