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
    SimRunner sim;
    // Use this for initialization
    void Start()
    {
        blocks = new List<GameObject>();
        warriors = new List<GameObject>();
        bodies = new List<GameObject>();

        DontDestroyOnLoad(gameObject);
        sim = new SimRunner(100, 30);
        Debug.Log(String.Format("{0} {1} {2}", Thread.CurrentThread.IsBackground, Thread.CurrentThread.ManagedThreadId, Thread.CurrentThread.ThreadState));

        simThread = new Thread(sim.StartLoop);
        simThread.Start();
        sim.isRunning = true;

    }

    // Update is called once per frame
    void Update()
    {
        lock (sim.exportDataLock)
        {
            //apply data
        }
    }

    private void OnDestroy()
    {
        sim.isRunning = false;
    }
}
