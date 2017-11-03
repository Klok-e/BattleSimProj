using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using SharpNeat.Core;
using SharpNeat.Phenomes;
using SharpNeat.Genomes.Neat;
using UnityEngine.UI;
using System.Linq;

public class BattleManager : MonoBehaviour
{
    public static BattleManager battleManagerInst;

    public GameObject tilePref;
    public GameObject warrPref;
    public GameObject projectilePref;
    public GameObject obstaclePref;
    public GameObject playerPref;


    [HideInInspector] public GameObject blocksParent;
    [HideInInspector] public GameObject warriorsParent;
    [HideInInspector] public GameObject projectilesParent;
    [HideInInspector] public GameObject playersParent;

    public static StructureOfWarrior defaultWarStrct = new StructureOfWarrior()
    {
        blood = 1000,
        str = new List<Limb>(){
            new Limb("default", 50f, 0.01f, false),
            new Limb("default", 50f, 0.01f, false),
            new Limb("default", 50f, 0.01f, false)
            }
    };

    System.Random random;

    int warriorsTotal;

    [HideInInspector] private int height;
    [HideInInspector] private int width;

    private List<IBlackBox>[] playersNets;

    public LoadBattleMenu loadMenuInst;

    private void Start()
    {
        battleManagerInst = this;
        SaveLoad.Load();
        loadMenuInst.Refresh();
        playersNets = new List<IBlackBox>[2];//bcos 2 players
    }

    #region UI related methods
    public void InitializeEverything()
    {
        InitializeMap();
        InitPlayers();
    }

    public void StartGame()
    {
        foreach (Transform item in playersParent.transform)
        {
            item.GetComponent<BattlePlayer>().SpawnWarriors();
        }
        //TODO: this
    }
    #endregion

    void InitializeMap()
    {
        Debug.Log("Initializing sim");
        blocksParent = new GameObject("blocksParent");
        warriorsParent = new GameObject("warriorsParent");
        projectilesParent = new GameObject("projectilesParent");
        playersParent = new GameObject("playersParent");

        CreateNewMap(width, height);
    }

    void InitPlayers()
    {
        AddPlayer(1, new Vector2(2, 2), playersNets[0]);
        AddPlayer(2, new Vector2(width - 2, height - 2), playersNets[1]);
    }

    public bool warriorsSet;
    public void SetMapSizeAndWarriors(int width, int height, int warriors)
    {
        this.height = height;
        this.width = width;
        warriorsTotal = warriors;
        warriorsSet = true;
    }

    public bool loadedNets;
    public void LoadGenomesToPlIndex(string filename, int ind)
    {
        Debug.Assert(0 <= ind && ind <= 1);//0 or 1 only
        var exp = new SimEditor.BattleExperiment();
        var dec = exp.CreateDecoder();

        var pop = exp.LoadPopulation(filename);

        playersNets[ind] = new List<IBlackBox>();

        while (playersNets[ind].Count < warriorsTotal)
        {
            for (int i = 0; i < pop.Count; i++)
            {
                if (playersNets[ind].Count >= warriorsTotal)
                {
                    break;
                }
                playersNets[ind].Add(dec.Decode(pop[i]));
            }
        }

        if (playersNets[0].Count == warriorsTotal && playersNets[1].Count == warriorsTotal)
        {
            loadedNets = true;
        }
        Debug.Assert(loadedNets);
    }

    public void CreateNewMap(int width, int height)
    {
        random = new System.Random();
        //generate map
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var bl = CreateNewBlock(new Vector2(i, j), (i == 0 || i == width - 1 || j == 0 || j == height - 1) ? false : true);
            }
        }
        Debug.Log("Map created");
    }

    #region Create game object methods
    private void AddPlayer(int team, Vector2 pos, List<IBlackBox> nets)
    {
        var pl = Instantiate(playerPref, playersParent.transform);
        var scrpt = pl.GetComponent<BattlePlayer>();

        scrpt.Initialize(pos, team, nets);
    }

    private BlockController CreateNewBlock(Vector2 pos, bool empty)
    {
        GameObject newObj;
        if (empty)
        {
            newObj = Instantiate(tilePref, blocksParent.transform);
        }
        else
        {
            newObj = Instantiate(obstaclePref, blocksParent.transform);
        }
        var scrpt = newObj.GetComponent<BlockController>();
        scrpt.transform.position = pos;

        return scrpt;
    }

    private WarriorController CreateNewWarrior(Vector2 pos, StructureOfWarrior str, int team, NeuralAI ai, BattlePlayer pla)
    {
        var newObj = Instantiate(warrPref, warriorsParent.transform);

        var scrpt = newObj.GetComponent<WarriorController>();
        //scrpt.Initialize(pos, str, team, ai, pla);

        return scrpt;
    }

    public ProjectileController CreateNewProjectile(Vector2 start, Vector2 direction, float damage, WarriorController shooter)
    {
        var newObj = Instantiate(projectilePref, projectilesParent.transform);

        var scrpt = newObj.GetComponent<ProjectileController>();
        scrpt.Initialize(start, direction, damage, shooter);

        return scrpt;
    }
    #endregion
}