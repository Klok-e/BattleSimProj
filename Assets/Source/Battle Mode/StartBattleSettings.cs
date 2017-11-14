using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BattleMode
{
    [Serializable]
    internal class StartBattleSettings
    {
        public static StartBattleSettings singleton;

        public int warriorsTotal;

        public int height;
        public int width;
        public List<string> playersNetsPaths;

        public StartBattleSettings()
        {
            Debug.Log("Initializing StartBattleSettings...");
            singleton = this;
            try
            {
                var n = SaveLoadStartBattle.LoadFromDisc();
                if (n != null)
                {
                    singleton = n;
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        public List<List<IBlackBox>> DecodeAllGenomes()
        {
            var decoded = new List<List<IBlackBox>>();
            foreach (var item in playersNetsPaths)
            {
                decoded.Add(LoadAndDecodeGenomes(item));
            }
            return decoded;
        }

        private List<IBlackBox> LoadAndDecodeGenomes(string filename)
        {
            var exp = new SimEditor.BattleExperiment();
            var dec = exp.CreateDecoder();

            var pop = exp.LoadPopulation(filename, warriorsTotal);

            var playersNets = new List<IBlackBox>();

            foreach (var genome in pop)
            {
                playersNets.Add(dec.Decode(genome));
            }

            return playersNets;
        }

        public void ResetNets()
        {
            playersNetsPaths = new List<string>();
        }

        public void AddNets(string genomesPath)
        {
            playersNetsPaths.Add(genomesPath);
        }

        public void SetSize(int wid, int hei)
        {
            width = wid;
            height = hei;
        }

        public void SetWarriors(int warr)
        {
            warriorsTotal = warr;
        }

        public void SaveThis()
        {
            SaveLoadStartBattle.SaveOnDisc(this);
        }

        private static class SaveLoadStartBattle
        {
            private const string saveFile = "/startBattleSettings.sttng";

            public static void SaveOnDisc(StartBattleSettings toSave)
            {
                BinaryFormatter bf = new BinaryFormatter();
                //Application.persistentDataPath is a string, so if you wanted you can put that into debug.log if you want to know where save games are located
                FileStream file = File.Create(Application.dataPath + saveFile); //you can call it anything you want
                bf.Serialize(file, toSave);
                file.Close();
                Debug.Log("Saved in " + saveFile);
            }

            public static StartBattleSettings LoadFromDisc()
            {
                if (File.Exists(Application.dataPath + saveFile))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    FileStream file = File.Open(Application.dataPath + saveFile, FileMode.Open);
                    var ans = (StartBattleSettings)bf.Deserialize(file);
                    file.Close();
                    Debug.Log("Loaded from " + saveFile);
                    return ans;
                }
                return null;
            }
        }
    }
}
