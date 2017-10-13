using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battle_sim_imp;
using System.Threading;
using System;

public class SimController : MonoBehaviour
{
    public GameObject blockPref;
    public GameObject warrPref;
    public GameObject bodyPref;

    List<GameObject> blocks;
    List<GameObject> warriors;
    List<GameObject> bodies;

    Thread simThread;
    SimRunner simRunner;
    // Use this for initialization
    void Start()
    {
        blocks = new List<GameObject>();
        warriors = new List<GameObject>();
        bodies = new List<GameObject>();

        simRunner = new SimRunner(100, 20,1);
        Debug.Log(String.Format("{0} {1} {2}", Thread.CurrentThread.IsBackground, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.ThreadState));

        simThread = new Thread(simRunner.StartLoop);
        simRunner.isRunning = true;
        simThread.Start();
    }

    // Update is called once per frame
    void Update()
    {
        lock (simRunner.exportDataLock)
        {
            if (blocks.Count > simRunner.blockPoss.Count)
            {
                throw new NotImplementedException();
            }
            if (warriors.Count > simRunner.warrPoss.Count)
            {
                throw new NotImplementedException();
            }
            if (bodies.Count > simRunner.bodyPoss.Count)
            {
                throw new NotImplementedException();
            }

            for (int i = 0; i < simRunner.blockPoss.Count; i++)
            {
                var blockItem = simRunner.blockPoss[i];

                var setEmpty = blockItem.empty;
                var setPos = blockItem.pos;
                try
                {
                    var blockObjUnity = blocks[i].GetComponent<BlockController>();

                    blockObjUnity.isEmpty = setEmpty;
                    blockObjUnity.transform.position = setPos;
                }
                catch (ArgumentOutOfRangeException e)
                {
                    blocks.Add(CreateNewBlock(setPos, setEmpty));
                }
            }
            for (int i = 0; i < simRunner.warrPoss.Count; i++)
            {
                var warrItem = simRunner.warrPoss[i];

                var setPos = warrItem.pos;
                try
                {
                    var warrObjUnity = warriors[i].GetComponent<WarriorController>();

                    warrObjUnity.transform.position = setPos;
                }
                catch (ArgumentOutOfRangeException e)
                {
                    warriors.Add(CreateNewWarrior(setPos));
                }
            }
            for (int i = 0; i < simRunner.bodyPoss.Count; i++)
            {
                var bodyItem = simRunner.bodyPoss[i];

                var setPos = bodyItem.pos;
                try
                {
                    var bodyObjUnity = bodies[i].GetComponent<BodyController>();

                    bodyObjUnity.transform.position = setPos;
                }
                catch (ArgumentOutOfRangeException e)
                {
                    bodies.Add(CreateNewBody(setPos));
                }
            }
        }
    }

    private GameObject CreateNewBlock(Vector2 pos, bool empty)
    {
        var newObj = Instantiate(blockPref, transform);

        var scrpt = newObj.GetComponent<BlockController>();
        scrpt.isEmpty = empty;
        scrpt.transform.position = pos;

        return newObj;
    }

    private GameObject CreateNewWarrior(Vector2 pos)
    {
        var newObj = Instantiate(blockPref, transform);

        var scrpt = newObj.GetComponent<BlockController>();
        scrpt.transform.position = pos;

        return newObj;
    }

    private GameObject CreateNewBody(Vector2 pos)
    {
        var newObj = Instantiate(blockPref, transform);

        var scrpt = newObj.GetComponent<BlockController>();
        scrpt.transform.position = pos;

        return newObj;
    }

    private void OnDestroy()
    {
        simRunner.isRunning = false;
    }
}
