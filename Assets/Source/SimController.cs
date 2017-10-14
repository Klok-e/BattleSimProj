using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;

public class SimController : MonoBehaviour
{
    public GameObject tilePref;
    public GameObject warrPref;
    public GameObject bodyPref;
    public GameObject obstaclePref;

    List<BlockController> blocks;
    List<WarriorController> warriors;
    List<BodyController> bodies;

    System.Random random;

    public static StructureOfWarrior defaultWarStrct = new StructureOfWarrior()
    {
        blood = 1000,
        str = new List<Limb>(){
            new Limb("default", 50f, 0.01f, false),
            new Limb("default", 50f, 0.01f, false),
            new Limb("default", 50f, 0.01f, false)
            }
    };

    // Use this for initialization
    void Start()
    {
        blocks = new List<BlockController>();
        warriors = new List<WarriorController>();
        bodies = new List<BodyController>();

        CreateNewMap(10, 10, 1);
    }

    int time;
    readonly int fps = 100;
    void Update()
    {
        if (Environment.TickCount - time > 1000 / fps)
        {
            //Tick();
            time = Environment.TickCount;
        }
    }

    public void Tick()
    {
        foreach (var body in bodies)
        {
            body.Tick();
        }

        foreach (var warrior in warriors)
        {
            if (warrior.currentAction!=null)
            {
                if (!warrior.currentAction.isFinished)
                    warrior.Tick();
                else
                    warrior.Tick(warrior.ChooseAction());
            }
        }
    }

    private void CreateNewMap(int height, int width, int warrOnEachSide)
    {
        random = new System.Random();

        //generate map
        for (int i = 0; i < warrOnEachSide; i++)
        {
            var w = CreateNewWarrior(new Vector2(1, 1), defaultWarStrct, 0, new RandomAi(9001, 5, random));
        }

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                var bl = CreateNewBlock(new Vector2(i, j), (i == 0 || i == height - 1 || j == 0 || j == width - 1) ? false : true);
            }
        }
    }

    private BlockController CreateNewBlock(Vector2 pos, bool empty)
    {
        if (empty)
        {
            var newObj = Instantiate(tilePref, transform);

            var scrpt = newObj.GetComponent<BlockController>();
            scrpt.transform.position = pos;

            blocks.Add(scrpt);
            return scrpt;
        }
        else
        {
            var newObj = Instantiate(obstaclePref, transform);

            var scrpt = newObj.GetComponent<BlockController>();
            scrpt.transform.position = pos;

            blocks.Add(scrpt);
            return scrpt;
        }
    }

    private WarriorController CreateNewWarrior(Vector2 pos, StructureOfWarrior str, int team, AAi ai)
    {
        var newObj = Instantiate(warrPref, transform);

        var scrpt = newObj.GetComponent<WarriorController>();
        scrpt.transform.position = pos;

        scrpt.ai = ai;
        scrpt.team = team;
        scrpt.bloodRemaining = str.blood;
        scrpt.limbs = new List<Limb>();
        foreach (var l in str.str)
        {
            scrpt.limbs.Add(l.Copy(scrpt));
        }
        scrpt.speed = 0.01f;

        warriors.Add(scrpt);
        return scrpt;
    }

    private BodyController CreateNewBody(Vector2 pos)
    {
        var newObj = Instantiate(bodyPref, transform);

        var scrpt = newObj.GetComponent<BodyController>();
        scrpt.transform.position = pos;

        bodies.Add(scrpt);
        return scrpt;
    }
}
